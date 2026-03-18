using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PrimeTween;
using UnityEngine.InputSystem;

public class SubjectElement : MonoBehaviour
{
    [Header("Animation")]
    [SerializeField] private float expandDuration = 0.2f;
    [SerializeField] private float expandPadding = 0.15f;
    
    [Header("Links")]
    [SerializeField] private SOFontStyle linkFontStyle;
    [SerializeField] private Texture2D linkCursor;
    [SerializeField] private Vector2 linkCursorHotspot;
    
    [Header("References")]
    [SerializeField] private Button button;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI descriptionText;

    private RectTransform _rectTransform;
    private float _collapsedHeight;
    private float _expandedHeight;
    private bool _expanded;
    private bool _isHoveringLink;
    private TagLink[] _links;

    public event Action<SubjectElement> OnExpanded;
    public float ExpandDuration => expandDuration;

    private void Awake()
    {
        _rectTransform = (RectTransform)transform;
        _collapsedHeight = _rectTransform.sizeDelta.y;
        button?.onClick.AddListener(ToggleExpand);
    }

    private void SetDefaultCursor() 
    {
        _isHoveringLink = false;
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }

    private void Update()
    {
        if (!_expanded) return;

        int linkIndex = TMP_TextUtilities.FindIntersectingLink(descriptionText, Mouse.current.position.ReadValue(), null);
        bool hoveringLink = linkIndex != -1;

        if (hoveringLink != _isHoveringLink)
        {
            _isHoveringLink = hoveringLink;
            if (_isHoveringLink)
            {
                Cursor.SetCursor(linkCursor, linkCursorHotspot, CursorMode.Auto);
            }
            else
            {
                SetDefaultCursor();
            }
        }

        if (Mouse.current.leftButton.wasReleasedThisFrame && hoveringLink)
        {
            string linkId = descriptionText.textInfo.linkInfo[linkIndex].GetLinkID();
            if (int.TryParse(linkId, out int i) && i >= 0 && i < _links.Length)
            {
                Application.OpenURL(_links[i].url);
            }
        }
    }

    private void ToggleExpand()
    {
        _expanded = !_expanded;
        AnimateTo(_expanded ? _expandedHeight : _collapsedHeight);
        if (_expanded) OnExpanded?.Invoke(this);
    }

    private void AnimateTo(float targetHeight)
    {
        Tween.Custom(this, _rectTransform.sizeDelta.y, targetHeight, expandDuration, (target, value) => target.SetHeight(value));
    }

    private void SetHeight(float height)
    {
        _rectTransform.sizeDelta = new Vector2(_rectTransform.sizeDelta.x, height);
    }
    
    public void Collapse()
    {
        if (!_expanded) return;
        _expanded = false;
        AnimateTo(_collapsedHeight);
    }
    
    public void Setup(TagData tag)
    {
        if (titleText) titleText.text = tag.label;

        if (descriptionText)
        {
            _links = tag.links;
            descriptionText.text = tag.summary;

            if (tag.links is { Length: > 0 })
            {
                string linksString = string.Empty;

                for (int i = 0; i < tag.links.Length; i++)
                    linksString += $"\n<link={i}>{linkFontStyle.ApplyStyle(tag.links[i].label)}</link>";

                descriptionText.text += "\n\nLinks:" + linksString;
            }

            descriptionText.ForceMeshUpdate();
            float textHeight = descriptionText.textBounds.size.y;
            _expandedHeight = _collapsedHeight + (textHeight * expandPadding);
        }
    }
}