using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PrimeTween;

public class SubjectElement : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private float expandDuration = 0.2f;
    [SerializeField] private float expendHeightMultiplier = 0.35f;

    private RectTransform _rectTransform;
    private float _collapsedHeight;
    private float _expandedHeight;
    private bool _expanded;

    public event Action<SubjectElement> OnExpanded;
    public float ExpandDuration => expandDuration;

    private void Awake()
    {
        _rectTransform = (RectTransform)transform;
        _collapsedHeight = _rectTransform.sizeDelta.y;
        button?.onClick.AddListener(ToggleExpand);
    }

    public void Setup(TagData tag)
    {
        if (titleText) titleText.text = tag.label;
        
        
        if (descriptionText)
        {
            descriptionText.text = tag.summary;

            if (tag.links.Length > 0)
            {
                string linksString = "\n\nLinks:";
                
                foreach (var link in tag.links)
                {
                    linksString += $"\n{link}";
                }
                
                descriptionText.text += linksString;
            }
        }

        _expandedHeight = _collapsedHeight + descriptionText.preferredHeight * expendHeightMultiplier;
    }

    public void Collapse()
    {
        if (!_expanded) return;
        _expanded = false;
        AnimateTo(_collapsedHeight);
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
}