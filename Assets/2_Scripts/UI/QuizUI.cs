using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using DNExtensions.Systems.MenuSystem;
using DNExtensions.Utilities;
using DNExtensions.Utilities.AutoGet;
using Utkaka.ScaleNineSlicer.UI;

public class QuizUI : MonoBehaviour
{
    [Header("Top Bar")]
    [SerializeField] private GameObject topBar;
    [SerializeField] private TextMeshProUGUI questionsProgressText;
    [SerializeField] private SlicedImage progressBarFill;
    [SerializeField] private Button exitButton;
    [SerializeField] private Button skipButton;
    
    [Header("Botton Bar")]
    [SerializeField] private GameObject bottomBar;
    [SerializeField] private Button continueButton;
    
    [Header("Feedback Panel")]
    [SerializeField] private GameObject feedbackPanel;
    [SerializeField] private TextMeshProUGUI feedbackText;
    
    [Header("Question Panel")]
    [SerializeField] private GameObject questionPanel;
    [SerializeField] private TextMeshProUGUI questionText;
    [SerializeField] private Transform optionsContainer;
    [SerializeField] private GameObject optionButtonPrefab;
    
    [Header("Code Text")]
    [SerializeField] private TMP_FontAsset monospaceFont;
    [SerializeField] private Color codeHighlightColor;
    [SerializeField, Range(0,100)] private int codeFontSizeMultiplier = 85;
    
    [SerializeField, AutoGetScene, HideInInspector] private QuizManager quizManager;
    [SerializeField, AutoGetSelf, HideInInspector] private ScreenNavigation screenNavigation;

    private readonly List<Toggle> _toggles = new();
    private bool _waitingForFeedback;
    private bool _quizCompleted;
    
    
    private void Awake()
    {
        continueButton.onClick.AddListener(OnContinueClicked);
        exitButton.onClick.AddListener(quizManager.QuitQuiz);
        skipButton.onClick.AddListener(quizManager.SkipQuestion);
        
        QuizManager.OnQuestionLoaded += DisplayQuestion;
        QuizManager.OnAnswerSubmitted += ShowFeedback;
        QuizManager.OnQuizCompleted += QuizCompleted;
        QuizManager.OnQuizStarted += QuizStarted;
    }

    private void OnDestroy()
    {
        QuizManager.OnQuestionLoaded -= DisplayQuestion;
        QuizManager.OnAnswerSubmitted -= ShowFeedback;
        QuizManager.OnQuizCompleted -= QuizCompleted;
        QuizManager.OnQuizStarted -= QuizStarted;
    }
    
    
    private void QuizStarted()
    {
        _quizCompleted = false;
        questionPanel.SetActive(true);
        topBar.SetActive(true);
        feedbackPanel.SetActive(false);
        continueButton.GetComponentInChildren<TextMeshProUGUI>().text = "Submit";
    }
    
    private void QuizCompleted(int correct, int total)
    {
        _quizCompleted = true;
        questionPanel.SetActive(false);
        feedbackPanel.SetActive(true);
        topBar.SetActive(false);
        feedbackText.text = $"Quiz Complete!\n{correct}/{total}";
        continueButton.interactable = true;
        continueButton.GetComponentInChildren<TextMeshProUGUI>().text = "Return";
    }
    
    private void ShowFeedback(bool correct, string explanation)
    {
        continueButton.GetComponentInChildren<TextMeshProUGUI>().text = "Continue";
        _waitingForFeedback = true;
        questionPanel.SetActive(false);
        feedbackPanel.SetActive(true);

        var titleText = correct ? $"Correct!" : $"Wrong!";
        titleText = titleText.Rich().Color(correct ? "Green" : "Red");
        
        feedbackText.text = $"{titleText}\n{explanation}";
    }

    private void DisplayQuestion(QuestionData data, int answered, int total)
    {
        foreach (Transform child in optionsContainer)
        {
            Destroy(child.gameObject);
        }
        _toggles.Clear();
        
        feedbackPanel.SetActive(false);
        questionPanel.SetActive(true);
        UpdateContinueButton();

        bool isMultiSelect = data.type == "MultiSelect";
        string typeHint = isMultiSelect ? "(Select all that apply)" : "(Select one)";
        
        questionText.text = $"{ParseCodeTags(data.question)}\n<size=70%><i>{typeHint}</i></size>";
        questionsProgressText.text = $"Question {answered}/{total}";
        progressBarFill.fillAmount = (float)answered / total;

        foreach (string option in data.options)
        {
            var obj = Instantiate(optionButtonPrefab, optionsContainer);
            var toggle = obj.GetComponent<Toggle>();
            obj.GetComponentInChildren<TextMeshProUGUI>().text = option;

            toggle.onValueChanged.AddListener(val =>
            {
                UpdateContinueButton();
            } );
            
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
                        toggle.isOn = true;
                    }
                });
            }

            _toggles.Add(toggle);
        }
        
        screenNavigation?.SetUpSelectables();
    }
    
    private string ParseCodeTags(string text)
    {
        string fontName = monospaceFont ? monospaceFont.name : "LiberationSans SDF";

        return Regex.Replace(text, @"`([^`]+)`", m => 
            m.Groups[1].Value.Rich()
                .Font(fontName)
                .Color(codeHighlightColor.ToHex())
                .Size($"{codeFontSizeMultiplier}%")
                .ToString());
    }

    private void DeselectOthers(Toggle selected)
    {
        foreach (var t in _toggles)
        {
            if (t != selected) t.SetIsOnWithoutNotify(false);
        }
    }

    private void UpdateContinueButton()
    {
        foreach (var toggle in _toggles)
        {
            if (toggle && toggle.isOn)
            {
                continueButton.interactable = true;
                return;
            }
        }

        continueButton.interactable = false;
    }

    private void OnContinueClicked()
    {
        if (_quizCompleted)
        {
            quizManager.QuitQuiz();
            return;
        }
        
        if (_waitingForFeedback)
        {
            _waitingForFeedback = false;
            continueButton.GetComponentInChildren<TextMeshProUGUI>().text = "Submit";
            quizManager.NextQuestion();
            return;
        }

        var selected = _toggles
            .Select((t, i) => (t, i))
            .Where(x => x.t.isOn)
            .Select(x => x.i)
            .ToArray();

        if (selected.Length != 0)
        {
            _waitingForFeedback = true;
            questionPanel.SetActive(false);
            quizManager.Submit(selected);   
        }
    }




}