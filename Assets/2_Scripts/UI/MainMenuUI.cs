using UnityEngine;
using UnityEngine.UI;
using DNExtensions.Utilities.AutoGet;

public class MainMenu : MonoBehaviour
{
    
    
    [SerializeField] private Button startButton;
    [SerializeField, AutoGetScene, HideInInspector] private QuizManager quizManager;
    
    private void Awake()
    {
        startButton.onClick.AddListener(quizManager.StartQuiz);
    }
    
}