using DNExtensions.Systems.MenuSystem;
using DNExtensions.Utilities.AutoGet;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private Button startButton;
    [SerializeField] private Transform coursesContainer;
    [SerializeField] private Button courseButtonPrefab;
    [SerializeField, AutoGetSelf, HideInInspector] private ScreenNavigation screenNavigation;
    
    private void Awake()
    {
        startButton.onClick.AddListener(() => QuizManager.Instance?.StartQuiz());
        CreateCourseButtons();
    }

    private void CreateCourseButtons()
    {
        var courses = QuestionLoader.LoadCourses();
        
        foreach (var course in courses)
        {
            var button = Instantiate(courseButtonPrefab, coursesContainer);
            button.GetComponentInChildren<TextMeshProUGUI>().text = course.name;
            button.GetComponent<Button>().onClick.AddListener(() => QuizManager.Instance?.StartQuiz(course));
        }
        
        screenNavigation?.SetUpSelectables();
    }
}