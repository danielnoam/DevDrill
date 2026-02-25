using System.Collections.Generic;

public class Quiz
{
    public List<QuestionData> remaining;
    public QuestionData currentQuestion;
    public CourseData course;
    public int totalQuestions;
    public int questionsAnswered;
    public int correctAnswers;
    
    public bool IsCourse => course != null;
}