using System;

[Serializable]
public class QuestionData
{
    public string type;
    public string[] tags;
    public string question;
    public string[] options;
    public int[] correct;
    public string explanation;
}

[Serializable]
public class QuestionDatabase
{
    public QuestionData[] questions;
}