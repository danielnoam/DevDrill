
using System;
using UnityEngine;
using System.Collections.Generic;
using DNExtensions.Utilities.Button;
using Random = UnityEngine.Random;

public class QuizManager : MonoBehaviour
{
    [SerializeField] private QuestionLoader loader;

    private List<QuestionData> _pool;
    private List<QuestionData> _remaining;

    public event Action<QuestionData> OnQuestionLoaded;
    public event Action<bool, string> OnAnswerSubmitted;
    public event Action OnQuizFinished;
    
    public QuestionData CurrentQuestion { get; private set; }

    private void Start()
    {
        _pool = loader.LoadQuestions();
        StartQuiz();
    }

    [Button]
    public void StartQuiz()
    {
        _remaining = new List<QuestionData>(_pool);
        NextQuestion();
    }

    [Button]
    public void NextQuestion()
    {
        if (_remaining.Count == 0)
        {
            OnQuizFinished?.Invoke();
            return;
        }
        
        int index = Random.Range(0, _remaining.Count);
        CurrentQuestion = _remaining[index];
        _remaining.RemoveAt(index);
        OnQuestionLoaded?.Invoke(CurrentQuestion);
    }

    public void Submit(int[] selectedIndices)
    {
        var selected = new HashSet<int>(selectedIndices);
        var correct = new HashSet<int>(CurrentQuestion.correct);
        bool isCorrect = selected.SetEquals(correct);
        OnAnswerSubmitted?.Invoke(isCorrect, CurrentQuestion.explanation);
    }
}