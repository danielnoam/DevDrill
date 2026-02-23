using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using DNExtensions.Systems.MenuSystem;
using DNExtensions.Utilities.AutoGet;

public class QuizUI : MonoBehaviour
{
    [SerializeField, AutoGetScene] private QuizManager quizManager;
    [SerializeField, AutoGetSelf] private ScreenNavigation screenNavigation;
    
    [Header("Question Panel")]
    [SerializeField] private GameObject questionPanel;
    [SerializeField] private TextMeshProUGUI questionText;
    [SerializeField] private Transform optionsContainer;
    [SerializeField] private GameObject optionButtonPrefab;
    [SerializeField] private Button submitButton;
    
    [Header("Feedback Panel")]
    [SerializeField] private GameObject feedbackPanel;
    [SerializeField] private TextMeshProUGUI feedbackText;
    [SerializeField] private Button nextButton;

    private readonly List<Toggle> _toggles = new();

    private void Awake()
    {
        questionPanel.SetActive(true);
        feedbackPanel.SetActive(false);
    }

    private void OnEnable()
    {
        quizManager.OnQuestionLoaded += DisplayQuestion;
        quizManager.OnAnswerSubmitted += ShowFeedback;
        quizManager.OnQuizFinished += ShowFinished;
    }

    private void OnDisable()
    {
        quizManager.OnQuestionLoaded -= DisplayQuestion;
        quizManager.OnAnswerSubmitted -= ShowFeedback;
        quizManager.OnQuizFinished -= ShowFinished;
    }

    private void Start()
    {
        submitButton.onClick.AddListener(OnSubmitClicked);
        nextButton.onClick.AddListener(quizManager.NextQuestion);
    }

    private void DisplayQuestion(QuestionData data)
    {
        feedbackPanel.SetActive(false);
        questionPanel.SetActive(true);

        foreach (Transform child in optionsContainer)
        {
            Destroy(child.gameObject);
        }
        _toggles.Clear();

        bool isMultiSelect = data.type == "MultiSelect";
        
        string typeHint = isMultiSelect ? "(Select all that apply)" : "(Select one)";
        questionText.text = $"{data.question}\n<size=70%>{typeHint}</size>";

        foreach (string option in data.options)
        {
            var obj = Instantiate(optionButtonPrefab, optionsContainer);
            var toggle = obj.GetComponent<Toggle>();
            obj.GetComponentInChildren<TextMeshProUGUI>().text = option;

            if (!isMultiSelect)
            {
                toggle.onValueChanged.AddListener(val =>
                {
                    if (val)
                    {
                        DeselectOthers(toggle);
                    }
                    else
                    {
                        toggle.SetIsOnWithoutNotify(true);
                    }
                });
            }

            _toggles.Add(toggle);
        }
        
        screenNavigation?.SetUpSelectables();
    }

    private void DeselectOthers(Toggle selected)
    {
        foreach (var t in _toggles)
        {
            if (t != selected) t.SetIsOnWithoutNotify(false);
        }
    }

    private void OnSubmitClicked()
    {
        var selected = _toggles
            .Select((t, i) => (t, i))
            .Where(x => x.t.isOn)
            .Select(x => x.i)
            .ToArray();

        if (selected.Length == 0) return;

        questionPanel.SetActive(false);
        quizManager.Submit(selected);
    }

    private void ShowFeedback(bool correct, string explanation)
    {
        questionPanel.SetActive(false);
        feedbackPanel.SetActive(true);
        feedbackText.text = correct
            ? $"Correct!\n{explanation}"
            : $"Wrong!\n{explanation}";
    }

    private void ShowFinished()
    {
        questionPanel.SetActive(false);
        feedbackPanel.SetActive(true);
        feedbackText.text = "Quiz Complete!";
        nextButton.gameObject.SetActive(false);
    }
}