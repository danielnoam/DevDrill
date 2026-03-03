using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace DNExtensions.Utilities
{
    /// <summary>
    /// A snapping carousel that centers one item at a time within a viewport.
    /// Items are direct children of the viewport and author their anchors against it,
    /// so the editor preview matches runtime exactly.
    /// </summary>
    /// <remarks>
    /// Add items as direct children of the viewport. Set their anchors and offsets normally.
    /// Clip with a <see cref="Mask"/> or <see cref="RectMask2D"/> on the viewport.
    /// </remarks>
    [AddComponentMenu("")]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    public class CarouselView : UIBehaviour, IInitializePotentialDragHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        /// <summary>Event fired when the centered item index changes, passing the new index.</summary>
        [Serializable]
        public class IndexChangedEvent : UnityEvent<int> { }

        /// <summary>Determines which item is selected when the carousel first starts.</summary>
        private enum InitialItem
        {
            /// <summary>Selects the first item.</summary>
            First,
            /// <summary>Selects the last item.</summary>
            Last,
            /// <summary>Selects the middle item. Requires 3 or more items; falls back to the first item otherwise.</summary>
            Middle,
        }

        [Header("References")]
        [Tooltip("The viewport that contains the carousel items and handles clipping.")]
        [SerializeField] private RectTransform viewport;

        [Header("Snapping")]
        [Tooltip("Fraction of the viewport width the drag must cover before the carousel advances on release.")]
        [SerializeField, Range(0.1f, 0.9f)] private float snapThreshold = 0.3f;

        [Tooltip("How long the snap animation takes to settle on the target item.")]
        [SerializeField] private float snapSmoothTime = 0.12f;

        [Tooltip("Horizontal velocity (px/s) at release required to advance without meeting the threshold distance.")]
        [SerializeField] private float flickVelocityThreshold = 800f;

        [Header("Behaviour")]
        [Tooltip("Pixel gap between item slots.")]
        [SerializeField] private float spacing;

        [Tooltip("Resistance multiplier when dragging past the first or last item. 0 = immovable, 1 = no resistance.")]
        [SerializeField, Range(0f, 1f)] private float edgeResistance = 0.3f;

        [Tooltip("If true, navigating past the last item wraps to the first and vice versa.")]
        [SerializeField] private bool loop;

        [Tooltip("Which item to center when the carousel first starts.")]
        [SerializeField] private InitialItem initialItem = InitialItem.First;
        
        [SerializeField] private IndexChangedEvent onIndexChanged = new IndexChangedEvent();

        private RectTransform _rect;
        private int _currentIndex;
        private float _targetOffset;
        private float _snapVelocity;
        private float _currentOffset;
        private bool _dragging;
        private float _dragStartLocalX;
        private float _offsetAtDragStart;
        private float _prevOffset;
        private float _dragVelocity;

        /// <summary>The viewport that contains the carousel items and handles clipping.</summary>
        public RectTransform Viewport
        {
            get => viewport;
            set => viewport = value;
        }

        /// <summary>If true, navigating past the last item wraps to the first and vice versa.</summary>
        public bool Loop
        {
            get => loop;
            set => loop = value;
        }

        /// <summary>Pixel gap between item slots.</summary>
        public float Spacing
        {
            get => spacing;
            set { spacing = value; ApplyLayout(); }
        }

        /// <summary>Invoked whenever the centered item changes.</summary>
        public IndexChangedEvent OnIndexChanged => onIndexChanged;

        /// <summary>The zero-based index of the currently centered item.</summary>
        public int CurrentIndex => _currentIndex;

        /// <summary>Total number of items (direct children of <see cref="Viewport"/>).</summary>
        public int ItemCount => viewport != null ? viewport.childCount : 0;

        /// <summary>Returns true if the carousel has at least one item.</summary>
        public bool HasItems => ItemCount > 0;

        /// <summary>Returns true if the current item is the first one.</summary>
        public bool IsFirst => _currentIndex == 0;

        /// <summary>Returns true if the current item is the last one.</summary>
        public bool IsLast => _currentIndex == ItemCount - 1;

        /// <summary>
        /// Current position as a normalized value between 0 and 1,
        /// where 0 is the first item and 1 is the last.
        /// </summary>
        public float NormalizedPosition => ItemCount > 1 ? (float)_currentIndex / (ItemCount - 1) : 0f;

        public override bool IsActive() => base.IsActive() && viewport != null;

        /// <summary>
        /// Navigates to the specified item index.
        /// </summary>
        /// <param name="index">Target index; clamped to valid range, or wrapped if <see cref="Loop"/> is enabled.</param>
        /// <param name="immediate">If true, jumps without animation.</param>
        public void GoTo(int index, bool immediate = false)
        {
            if (ItemCount == 0)
                return;

            int resolved = loop ? WrapIndex(index) : Mathf.Clamp(index, 0, ItemCount - 1);
            bool changed = resolved != _currentIndex;
            _currentIndex = resolved;

            _targetOffset = GetTargetOffset(_currentIndex);

            if (immediate)
            {
                _snapVelocity  = 0f;
                _currentOffset = _targetOffset;
                ApplyOffset(_currentOffset);
            }

            if (changed)
                onIndexChanged.Invoke(_currentIndex);
        }

        /// <summary>Advances to the next item. Wraps to the first if <see cref="Loop"/> is enabled.</summary>
        public void Next() => GoTo(_currentIndex + 1);

        /// <summary>Returns to the previous item. Wraps to the last if <see cref="Loop"/> is enabled.</summary>
        public void Previous() => GoTo(_currentIndex - 1);
        

        protected new virtual void Start()
        {
            Canvas.ForceUpdateCanvases();
            ApplyLayout();
            GoTo(ResolveInitialIndex(), immediate: true);
        }

        protected virtual void LateUpdate()
        {
            if (viewport == null)
                return;

            if (_dragging)
            {
                float dt = Time.unscaledDeltaTime;
                _dragVelocity = dt > 0f ? (_currentOffset - _prevOffset) / dt : 0f;
                _prevOffset   = _currentOffset;
                return;
            }

            _currentOffset = Mathf.SmoothDamp(
                _currentOffset, _targetOffset, ref _snapVelocity,
                snapSmoothTime, Mathf.Infinity, Time.unscaledDeltaTime);

            ApplyOffset(_currentOffset);
        }

        protected override void OnRectTransformDimensionsChange()
        {
            base.OnRectTransformDimensionsChange();
            if (!IsActive()) return;
            ApplyLayout();
            _targetOffset  = GetTargetOffset(_currentIndex);
            _currentOffset = _targetOffset;
            ApplyOffset(_currentOffset);
        }

        public void OnInitializePotentialDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            _snapVelocity = 0f;
            _dragVelocity = 0f;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left || !IsActive())
                return;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                ViewRect, eventData.position, eventData.pressEventCamera, out Vector2 localPoint);

            _dragStartLocalX   = localPoint.x;
            _offsetAtDragStart = _currentOffset;
            _prevOffset        = _currentOffset;
            _dragVelocity      = 0f;
            _dragging          = true;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!_dragging || eventData.button != PointerEventData.InputButton.Left || !IsActive())
                return;

            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    ViewRect, eventData.position, eventData.pressEventCamera, out Vector2 localPoint))
                return;

            float rawDelta = localPoint.x - _dragStartLocalX;

            bool atStart = _currentIndex == 0             && rawDelta > 0f;
            bool atEnd   = _currentIndex == ItemCount - 1 && rawDelta < 0f;
            bool resist  = ItemCount <= 1 || (!loop && (atStart || atEnd));

            _currentOffset = _offsetAtDragStart + (resist ? rawDelta * edgeResistance : rawDelta);
            ApplyOffset(_currentOffset);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            _dragging = false;

            if (ItemCount <= 1)
            {
                GoTo(0);
                return;
            }

            float dragDelta = _currentOffset - _offsetAtDragStart;
            float threshold = ViewRect.rect.width * snapThreshold;
            int targetIndex = _currentIndex;

            if (Mathf.Abs(_dragVelocity) >= flickVelocityThreshold)
                targetIndex += _dragVelocity > 0f ? -1 : 1;
            else if (Mathf.Abs(dragDelta) >= threshold)
                targetIndex += dragDelta > 0f ? -1 : 1;

            GoTo(targetIndex);
        }

        private RectTransform ViewRect
        {
            get
            {
                if (viewport != null) return viewport;
                if (_rect == null) _rect = GetComponent<RectTransform>();
                return _rect;
            }
        }

        private float SlotWidth => ViewRect.rect.width + spacing;

        /// <summary>
        /// Sets each item slot origin to index * slotWidth.
        /// Items anchor against the viewport rect so offsets resolve correctly.
        /// </summary>
        private void ApplyLayout()
        {
            if (viewport == null) return;

            for (int i = 0; i < viewport.childCount; i++)
            {
                RectTransform item = viewport.GetChild(i) as RectTransform;
                if (item == null) continue;

                Vector2 pos = item.anchoredPosition;
                pos.x = i * SlotWidth;
                item.anchoredPosition = pos;
            }
        }

        /// <summary>Shifts each item from its slot origin by the given offset.</summary>
        private void ApplyOffset(float offset)
        {
            if (viewport == null) return;

            for (int i = 0; i < viewport.childCount; i++)
            {
                RectTransform item = viewport.GetChild(i) as RectTransform;
                if (item == null) continue;

                Vector2 pos = item.anchoredPosition;
                pos.x = i * SlotWidth + offset;
                item.anchoredPosition = pos;
            }
        }

        /// <summary>Returns the offset required to center the item at <paramref name="index"/>.</summary>
        private float GetTargetOffset(int index) => -(index * SlotWidth);

        private int WrapIndex(int index) => ((index % ItemCount) + ItemCount) % ItemCount;

        private int ResolveInitialIndex()
        {
            switch (initialItem)
            {
                case InitialItem.Last:   return ItemCount - 1;
                case InitialItem.Middle: return ItemCount >= 3 ? ItemCount / 2 : 0;
                default:                 return 0;
            }
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            if (viewport == null || ItemCount == 0) return;
            ApplyLayout();
            _targetOffset  = GetTargetOffset(Mathf.Clamp(_currentIndex, 0, ItemCount - 1));
            _currentOffset = _targetOffset;
            ApplyOffset(_currentOffset);
        }
#endif
    }
}