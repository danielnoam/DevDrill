using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Utkaka.ScaleNineSlicer.UI;

public class CourseElement : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private SlicedImage progressBarFill;
    [SerializeField] private Button button;
    [SerializeField] private Color progressCompleteColor = Color.lightGreen;

    private Color _progressBaseColor;
    
    private void Awake()
    {
        _progressBaseColor = progressBarFill.color;
    }
    public void Setup(CourseData course, int answered, int total, QuizManager quizManager)
    {
        button?.onClick.AddListener(() => quizManager.StartQuiz(course));
        if (titleText) titleText.text = course.name;
        if (descriptionText) descriptionText.text = course.description;
        
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