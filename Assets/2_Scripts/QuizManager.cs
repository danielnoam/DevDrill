
using System;
using UnityEngine;
using System.Collections.Generic;
using DNExtensions.Utilities;
using DNExtensions.Utilities.Button;
using Random = UnityEngine.Random;

public class QuizManager : MonoBehaviour
{
    public static QuizManager Instance { get; private set;}
    public static event Action OnQuizStarted;
    public static event Action<QuestionData, int, int> OnQuestionLoaded;
    public static event Action<bool, string> OnAnswerSubmitted;
    public static event Action<int, int> OnQuizFinished;
    
    
    [SerializeField] private QuestionLoader loader;

    
    private List<QuestionData> _pool;
    private List<QuestionData> _remaining;
    private QuestionData _currentQuestion;
    private int _totalQuestions;
    private int _questionsAnswered;
    private int _correctAnswers;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
    }

    private void Start()
    {
        StartQuiz();
    }

    [Button]
    public void StartQuiz()
    {
        _pool = loader.LoadQuestions();
        _pool.Shuffle();
        _totalQuestions = _pool.Count;
        _remaining = new List<QuestionData>(_pool);
        OnQuizStarted?.Invoke();
        NextQuestion();
    }

    [Button]
    public void NextQuestion()
    {
        if (_remaining.Count == 0)
        {
            OnQuizFinished?.Invoke(_correctAnswers, _totalQuestions);
            return;
        }
        
        int index = Random.Range(0, _remaining.Count);
        _currentQuestion = _remaining[index];
        _remaining.RemoveAt(index);
        _questionsAnswered++;
        OnQuestionLoaded?.Invoke(_currentQuestion, _questionsAnswered, _totalQuestions);
    }
    
    public void SkipQuestion()
    {
        NextQuestion();
    }
    
    public void Submit(int[] selectedIndices)
    {
        var selected = new HashSet<int>(selectedIndices);
        var correct = new HashSet<int>(_currentQuestion.correct);
        bool isCorrect = selected.SetEquals(correct);
        if (isCorrect) _correctAnswers++;
        OnAnswerSubmitted?.Invoke(isCorrect, _currentQuestion.explanation);
    }
    
    public void QuitQuiz()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
        Application.Quit();
    }


}