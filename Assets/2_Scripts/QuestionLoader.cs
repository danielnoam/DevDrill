
using UnityEngine;
using System.Collections.Generic;
using System.Linq;


public class QuestionLoader : MonoBehaviour
{
    [SerializeField] private TextAsset jsonFile;

    public List<QuestionData> LoadQuestions()
    {
        var db = JsonUtility.FromJson<QuestionDatabase>(jsonFile.text);
        return db.questions.ToList();
    }
}