using System.Collections.Generic;
using DNExtensions.Systems.MenuSystem;
using UnityEngine;
using UnityEngine.UI;
using DNExtensions.Utilities.AutoGet;
using PrimeTween;
using Screen = DNExtensions.Systems.MenuSystem.Screen;

public class LibraryUI : MonoBehaviour
{
    [SerializeField] private Screen mainMenuScreen;
    [SerializeField] private Button backButton;
    [SerializeField] private Transform subjectsContainer;
    [SerializeField] private SubjectElement subjectElementPrefab;
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private float scrollDuration = 0.3f;
    [SerializeField, AutoGetScene] private MenuManager menuManager;

    private SubjectElement _expandedElement;

    private void Awake()
    {
        backButton.onClick.AddListener(() => menuManager.ShowScreen(mainMenuScreen));
        SpawnSubjectElements();
    }

    private void SpawnSubjectElements()
    {
        var tags = DataLoader.LoadTags();

        foreach (var tag in tags.Values)
        {
            var element = Instantiate(subjectElementPrefab, subjectsContainer);
            element.Setup(tag);
            element.OnExpanded += OnElementExpanded;
        }
    }

    private void OnElementExpanded(SubjectElement expanded)
    {
        if (_expandedElement && _expandedElement != expanded)
        {
            _expandedElement.Collapse();
        }

        _expandedElement = expanded;

        Tween.Delay(expanded.ExpandDuration, () => ScrollToCenter(expanded));
    }

    private void ScrollToCenter(SubjectElement element)
    {
        if (!scrollRect) return;
        
        var content = scrollRect.content;
        var viewport = scrollRect.viewport;

        float contentHeight = content.rect.height;
        float viewportHeight = viewport.rect.height;

        if (contentHeight <= viewportHeight) return;
        
        var elementRect = (RectTransform)element.transform;
        Vector2 elementLocalPos = content.InverseTransformPoint(elementRect.position);
        float elementCenterY = -elementLocalPos.y;

        float targetY = elementCenterY - viewportHeight * 0.5f;
        float normalised = Mathf.Clamp01(targetY / (contentHeight - viewportHeight));

        Tween.Custom(this, scrollRect.verticalNormalizedPosition, 1f - normalised, scrollDuration, (target, value) => target.scrollRect.verticalNormalizedPosition = value);
    }
}