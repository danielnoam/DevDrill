using System.Collections.Generic;
using DNExtensions.Systems.MenuSystem;
using DNExtensions.Utilities;
using UnityEngine;
using DNExtensions.Utilities.AutoGet;
using UnityEngine.UI;
using Screen = DNExtensions.Systems.MenuSystem.Screen;


public class CoursesUI : MonoBehaviour
{
    [SerializeField] private Screen mainMenuScreen;
    [SerializeField] private Button backButton;
    [SerializeField] private Transform coursesContainer;
    [SerializeField] private CourseElement courseElementPrefab;
    [SerializeField, AutoGetScene] private MenuManager menuManager;
    [SerializeField, AutoGetScene, HideInInspector] private QuizManager quizManager;
    
    private readonly List<(CourseElement button, CourseData course, int total)> _courseButtons = new();

    private void Awake()
    {
        backButton?.onClick.AddListener(() => menuManager.ShowScreen(mainMenuScreen));
        SpawnCourseButtons();
        QuizManager.OnQuizQuit += RefreshProgress;
    }
    

    private void OnDestroy()
    {
        QuizManager.OnQuizQuit -= RefreshProgress;
    }
    
    
    private void RefreshProgress()
    {
        foreach (var (button, course, total) in _courseButtons)
        {
            button.UpdateProgress(ProgressManager.GetAnsweredCount(course.id), total);
        }
    }

    private void SpawnCourseButtons()
    {
        if (!coursesContainer || !courseElementPrefab) return;
        
        var courses = DataLoader.LoadCourses();
        coursesContainer.gameObject.DestroyChildren();

        foreach (var course in courses)
        {
            var courseButton = Instantiate(courseElementPrefab, coursesContainer);
            var answered = ProgressManager.GetAnsweredCount(course.id);
            var total = DataLoader.LoadQuestionsForCourse(course).Count;
            courseButton.Setup(course, answered, total, quizManager);
            _courseButtons.Add((courseButton, course, total));
        }
    }
}