using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using DNExtensions.Systems.MenuSystem;
using DNExtensions.Utilities.AutoGet;

public class MainMenu : MonoBehaviour
{
    
    [SerializeField, AutoGetScene] private QuizManager quizManager;
    [SerializeField, AutoGetSelf] private ScreenNavigation screenNavigation;
    [SerializeField, AutoGetSelf] private CanvasGroup canvasGroup;
    [SerializeField] private Button startButton;
    
    
    
    private void Awake()
    {
        startButton.onClick.AddListener(quizManager.StartQuiz);
    }

    private void OnEnable()
    {
        QuizManager.OnQuizStarted += QuizStarted;
        QuizManager.OnQuizFinished += QuizEnded;
    }

    private void OnDisable()
    {
        QuizManager.OnQuizStarted -= QuizStarted;
        QuizManager.OnQuizFinished -= QuizEnded;
    }

    private void QuizEnded(int arg1, int arg2)
    {

    }

    private void QuizStarted()
    {
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
    }
}