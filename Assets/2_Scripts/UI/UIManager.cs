using System;
using DNExtensions.Systems.MenuSystem;
using DNExtensions.Utilities.AutoGet;
using UnityEngine;
using Screen = DNExtensions.Systems.MenuSystem.Screen;



public class UIManager : MonoBehaviour
{
    [SerializeField] private MenuManager menuManager;
    [SerializeField] private Screen quizScreen;
    [SerializeField] private Screen mainMenuScreen;
    
    [SerializeField, AutoGetScene, HideInInspector] private QuizManager quizManager;

    private void OnEnable()
    {
        QuizManager.OnQuizStarted += QuizStarted;
        QuizManager.OnQuizQuit += QuizQuit;
    }
    
    private void OnDisable()
    {
        QuizManager.OnQuizStarted -= QuizStarted;
        QuizManager.OnQuizQuit -= QuizQuit;
    }
    
    private void QuizStarted()
    {
        menuManager?.ShowScreen(quizScreen);
    }
    
    private void QuizQuit()
    {
        menuManager?.ShowScreen(mainMenuScreen);
    }
}