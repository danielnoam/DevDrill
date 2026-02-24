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
    [SerializeField, AutoGetSelf] private CanvasGroup canvasGroup;
    
    
    [Header("Style")]
    [SerializeField] private TMP_FontAsset monoFont;
    
    
    [Header("Top Bar")]
    [SerializeField] private GameObject topBar;
    [SerializeField] private TextMeshProUGUI questionsProgressText;
    [SerializeField] private Button exitButton;
    [SerializeField] private Button settingsButton;
    
    [Header("Botton Bar")]
    [SerializeField] private GameObject bottomBar;
    [SerializeField] private Button skipButton;
    [SerializeField] private Button helpButton;
    
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
        submitButton.onClick.AddListener(OnSubmitClicked);
        exitButton.onClick.AddListener(quizManager.QuitQuiz);
        nextButton.onClick.AddListener(quizManager.NextQuestion);
        skipButton.onClick.AddListener(quizManager.SkipQuestion);

        canvasGroup.alpha = 0;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    private void OnEnable()
    {
        QuizManager.OnQuestionLoaded += DisplayQuestion;
        QuizManager.OnAnswerSubmitted += ShowFeedback;
        QuizManager.OnQuizFinished += QuizFinished;
        QuizManager.OnQuizStarted += QuizStarted;
    }

    private void OnDisable()
    {
        QuizManager.OnQuestionLoaded -= DisplayQuestion;
        QuizManager.OnAnswerSubmitted -= ShowFeedback;
        QuizManager.OnQuizFinished -= QuizFinished;
        QuizManager.OnQuizStarted -= QuizStarted;
    }
    
    
    private void QuizStarted()
    {
        questionPanel.SetActive(true);
        topBar.SetActive(true);
        feedbackPanel.SetActive(false);
        canvasGroup.alpha = 1;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }

    private void DisplayQuestion(QuestionData data, int answered, int total)
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
        questionText.text = $"{ParseCodeTags(data.question)}\n<size=70%><i>{typeHint}</i></size>";
        
        questionsProgressText.text = $"Question {answered}/{total}";

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
    
    private string ParseCodeTags(string text)
    {
        string fontTag = monoFont ? $"<font=\"{monoFont}\">" : "<font=\"monospace\">";
    
        while (text.Contains("`"))
        {
            int open = text.IndexOf('`');
            int close = text.IndexOf('`', open + 1);
            if (close == -1) break;

            string code = text.Substring(open + 1, close - open - 1);
            string styled = $"{fontTag}<color=#A8FF60><size=85%>{code}</size></color></font>";
            text = text.Remove(open, close - open + 1).Insert(open, styled);
        }
        return text;
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

    private void QuizFinished(int correct, int total)
    {
        questionPanel.SetActive(false);
        bottomBar.SetActive(false);
        feedbackPanel.SetActive(true);
        feedbackText.text = $"Quiz Complete!\n{correct}/{total}";
        nextButton.gameObject.SetActive(false);
    }
}