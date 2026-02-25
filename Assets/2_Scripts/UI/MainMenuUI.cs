using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DNExtensions.Utilities.AutoGet;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private Button startButton;
    [SerializeField] private Transform coursesContainer;
    [SerializeField] private CourseButton courseButtonPrefab;

    [SerializeField, AutoGetScene, HideInInspector] private QuizManager quizManager;
    
    private readonly List<(CourseButton button, CourseData course, int total)> _courseButtons = new();

    private void Awake()
    {
        startButton.onClick.AddListener(() => quizManager.StartQuiz());
        SpawnCourseButtons();
        
        QuizManager.OnQuizQuit += RefreshProgress;
    }
    

    private void OnDestroy()
    {
        QuizManager.OnQuizQuit -= RefreshProgress;
    }
    
    private void OnQuizCompleted(int correct, int total) => RefreshProgress();
    
    private void RefreshProgress()
    {
        foreach (var (button, course, total) in _courseButtons)
        {
            button.UpdateProgress(ProgressManager.GetAnsweredCount(course.id), total);
        }
    }

    private void SpawnCourseButtons()
    {
        var courses = QuestionLoader.LoadCourses();

        foreach (var course in courses)
        {
            var courseButton = Instantiate(courseButtonPrefab, coursesContainer);
            var answered = ProgressManager.GetAnsweredCount(course.id);
            var total = QuestionLoader.LoadQuestionsForCourse(course).Count;
            courseButton.Setup(course, answered, total, quizManager);
            _courseButtons.Add((courseButton, course, total));
        }
    }
}