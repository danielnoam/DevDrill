using System;
using System.Linq;
using DNExtensions.Utilities;

[Serializable]
public class QuestionData
{
    public string id;
    public string type;
    public string difficulty;
    public string[] tags;
    public string question;
    public string[] options;
    public int[] correct;
    public string explanation;
    
    public void ShuffleOptions()
    {
        var indexed = options
            .Select((opt, i) => (opt, i))
            .ToList();
    
        indexed.Shuffle();
    
        options = indexed.Select(x => x.opt).ToArray();
        correct = correct.Select(c => indexed.FindIndex(x => x.i == c)).ToArray();
    }
}

[Serializable]
public class QuestionDatabase
{
    public QuestionData[] questions;
}