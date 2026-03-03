#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace DNExtensions.Utilities
{
    /// <summary>
    /// Adds a Carousel View entry to the GameObject > UI (Canvas) menu, creating a ready-to-use
    /// hierarchy with a viewport that acts as both the clipping mask and item container.
    /// </summary>
    internal static class CarouselViewCreator
    {
        private const string MenuPath = "GameObject/UI (Canvas)/Carousel View";
        private const int MenuPriority = 2062;

        [MenuItem(MenuPath, false, MenuPriority)]
        private static void Create(MenuCommand menuCommand)
        {
            GameObject parent = menuCommand.context as GameObject;
            EnsureCanvasExists(ref parent);

            GameObject root = CreateUIObject("Carousel View", parent);
            SetAnchorsAndSize(root.GetComponent<RectTransform>(),
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            CarouselView carousel = root.AddComponent<CarouselView>();
            carousel.Spacing = 10f;

            GameObject viewportGo = CreateUIObject("Viewport", root);
            RectTransform viewportRect = viewportGo.GetComponent<RectTransform>();
            SetAnchorsAndSize(viewportRect, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            Image viewportImage     = viewportGo.AddComponent<Image>();
            viewportImage.sprite    = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UIMask.psd");
            viewportImage.type      = Image.Type.Sliced;
            viewportImage.color     = Color.white;

            Mask viewportMask            = viewportGo.AddComponent<Mask>();
            viewportMask.showMaskGraphic = false;

            SerializedObject so = new SerializedObject(carousel);
            so.FindProperty("viewport").objectReferenceValue = viewportRect;
            so.ApplyModifiedPropertiesWithoutUndo();

            Undo.RegisterCreatedObjectUndo(root, "Create Carousel View");
            Selection.activeGameObject = root;
        }

        private static GameObject CreateUIObject(string name, GameObject parent)
        {
            GameObject go = new GameObject(name);
            go.AddComponent<RectTransform>();
            GameObjectUtility.SetParentAndAlign(go, parent);
            return go;
        }

        private static void SetAnchorsAndSize(RectTransform rect,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, Vector2 sizeDelta)
        {
            rect.anchorMin        = anchorMin;
            rect.anchorMax        = anchorMax;
            rect.anchoredPosition = anchoredPos;
            rect.sizeDelta        = sizeDelta;
        }

        /// <summary>
        /// Ensures a Canvas and EventSystem exist in the scene, creating them if needed.
        /// Matches Unity own UI element creation behavior.
        /// </summary>
        private static void EnsureCanvasExists(ref GameObject parent)
        {
            if (parent != null && parent.GetComponentInParent<Canvas>() != null)
                return;

            Canvas canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                GameObject canvasGo = new GameObject("Canvas");
                canvas = canvasGo.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasGo.AddComponent<CanvasScaler>();
                canvasGo.AddComponent<GraphicRaycaster>();
                Undo.RegisterCreatedObjectUndo(canvasGo, "Create Canvas");
            }

            if (Object.FindFirstObjectByType<EventSystem>() == null)
            {
                GameObject eventSystemGo = new GameObject("EventSystem");
                eventSystemGo.AddComponent<EventSystem>();
                eventSystemGo.AddComponent<StandaloneInputModule>();
                Undo.RegisterCreatedObjectUndo(eventSystemGo, "Create EventSystem");
            }

            parent = canvas.gameObject;
        }
    }
}
#endif