using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Utkaka.ScaleNineSlicer.UI;

public class CourseElement : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private bool showDifficulty = true;
    [SerializeField] private Color progressCompleteColor = Color.lightGreen;
    
    [Header("References")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private SlicedImage progressBarFill;
    [SerializeField] private SOFontStyle difictutyStyle;
    [SerializeField] private Button button;

    private Color _progressBaseColor;
    
    private void Awake()
    {
        _progressBaseColor = progressBarFill.color;
    }
    public void Setup(CourseData course, int answered, int total, QuizManager quizManager)
    {
        button?.onClick.AddListener(() => quizManager.StartQuiz(course));
        if (titleText) titleText.text = course.name;
        if (descriptionText)
        {
            var text = course.description;
            var difificulty = string.Empty;
            
            if (course.difficulties.Length > 0 && showDifficulty)
            {
                difificulty += $"\n\nDifficulties:";
                
                foreach (var difficulty in course.difficulties)
                {
                    difificulty += $"\n{difficulty}";
                }
                
                
            }
            descriptionText.text = text + difictutyStyle.ApplyStyle(difificulty);
        }
        
        UpdateProgress(answered, total);
    }
    
    public void UpdateProgress(int answered, int total)
    {
        if (!progressBarFill) return;
        progressBarFill.fillAmount = total > 0 ? (float)answered / total : 0f;

        if (answered >= total && total > 0)
        {
            progressBarFill.color = progressCompleteColor;
        }
        else
        {
            progressBarFill.color = _progressBaseColor;
        }
    }
}