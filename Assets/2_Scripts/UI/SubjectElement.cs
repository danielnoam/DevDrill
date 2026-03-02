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
    [SerializeField] private float expendHeightMultiplier = 0.35f;
    
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
    private Texture2D _defaultCursor;

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
            string url = descriptionText.textInfo.linkInfo[linkIndex].GetLinkID();
            Application.OpenURL(url);
        }
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
    
    public void Setup(TagData tag)
    {
        if (titleText) titleText.text = tag.label;

        if (descriptionText)
        {
            descriptionText.text = tag.summary;

            if (tag.links.Length > 0)
            {
                string linksTitleString = "\n\nLinks:";
                string linksString = string.Empty;

                foreach (var link in tag.links)
                {
                    linksString += $"\n<link={link}>{link}</link>";
                }

                descriptionText.text += linksTitleString + linkFontStyle.ApplyStyle(linksString);
            }
        }

        _expandedHeight = _collapsedHeight + descriptionText.preferredHeight * expendHeightMultiplier;
    }
}