using System;
using UnityEngine;
using System.Collections.Generic;
using DNExtensions.Utilities;
using DNExtensions.Utilities.Button;
using Random = UnityEngine.Random;

public class QuizManager : MonoBehaviour
{
    public static event Action OnQuizStarted;
    public static event Action<QuestionData, int, int> OnQuestionLoaded;
    public static event Action<bool, string> OnAnswerSubmitted;
    public static event Action<int, int> OnQuizCompleted;
    public static event Action OnQuizQuit;

    private Quiz _activeQuiz;
    

    public void StartQuiz(string[] tags = null, string[] excludeTags = null)
    {
        var questions = DataLoader.LoadQuestions(tags, excludeTags);
        StartQuizWithPool(questions, null);
    }

    public void StartQuiz(CourseData course)
    {
        var questions = DataLoader.LoadQuestionsForCourse(course);
        var answered = ProgressManager.GetAnsweredIds(course.id);
        questions.RemoveAll(q => answered.Contains(q.id));

        if (questions.Count == 0)
        {
            ProgressManager.ClearProgress(course.id);
            questions = DataLoader.LoadQuestionsForCourse(course);
        }

        StartQuizWithPool(questions, course);
    }

    private void StartQuizWithPool(List<QuestionData> pool, CourseData course)
    {
        pool.Shuffle();
        
        _activeQuiz = new Quiz
        {
            remaining = new List<QuestionData>(pool),
            course = course,
            totalQuestions = pool.Count,
            questionsAnswered = 0,
            correctAnswers = 0
        };
        
        OnQuizStarted?.Invoke();
        NextQuestion();
    }

    [Button]
    public void NextQuestion()
    {
        if (_activeQuiz.remaining.Count == 0)
        {
            OnQuizCompleted?.Invoke(_activeQuiz.correctAnswers, _activeQuiz.totalQuestions);
            return;
        }

        int index = Random.Range(0, _activeQuiz.remaining.Count);
        _activeQuiz.currentQuestion = _activeQuiz.remaining[index];
        _activeQuiz.currentQuestion.ShuffleOptions();
        _activeQuiz.remaining.RemoveAt(index);
        _activeQuiz.questionsAnswered++;
        
        OnQuestionLoaded?.Invoke(_activeQuiz.currentQuestion, _activeQuiz.questionsAnswered, _activeQuiz.totalQuestions);
    }

    public void SkipQuestion()
    {
        NextQuestion();
    }

    public void Submit(int[] selectedIndices)
    {
        var selected = new HashSet<int>(selectedIndices);
        var correct = new HashSet<int>(_activeQuiz.currentQuestion.correct);
        bool isCorrect = selected.SetEquals(correct);
        
        if (isCorrect)
        {
            _activeQuiz.correctAnswers++;
            if (_activeQuiz.IsCourse) ProgressManager.SaveAnswered(_activeQuiz.course.id, _activeQuiz.currentQuestion.id);
        }
        
        OnAnswerSubmitted?.Invoke(isCorrect, _activeQuiz.currentQuestion.explanation);
    }

    public void QuitQuiz()
    {
        _activeQuiz = null;
        OnQuizQuit?.Invoke();
    }
}