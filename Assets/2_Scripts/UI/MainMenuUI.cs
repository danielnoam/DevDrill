using System;
using DNExtensions.Systems.MenuSystem;
using DNExtensions.Utilities.AutoGet;
using UnityEngine;
using UnityEngine.UI;
using Screen = DNExtensions.Systems.MenuSystem.Screen;

public class MainMenuUI : MonoBehaviour
{

    [SerializeField] private Screen coursesScreen;
    [SerializeField] private Button coursesButton;
    [SerializeField] private Screen libraryScreen;
    [SerializeField] private Button libraryButton;
    [SerializeField, AutoGetScene] private MenuManager menuManager;


    private void Awake()
    {
        coursesButton?.onClick.AddListener(() => menuManager.ShowScreen(coursesScreen));
        libraryButton?.onClick.AddListener(() => menuManager.ShowScreen(libraryScreen));
    }
}