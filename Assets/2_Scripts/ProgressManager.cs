using System.Collections.Generic;
using UnityEngine;

public static class ProgressManager
{
    private const string Prefix = "progress_";

    public static HashSet<string> GetAnsweredIds(string courseId)
    {
        var raw = PlayerPrefs.GetString(Prefix + courseId, "");
        var result = new HashSet<string>();
        
        if (string.IsNullOrEmpty(raw)) return result;
        
        foreach (var id in raw.Split(','))
            result.Add(id);
        
        return result;
    }

    public static void SaveAnswered(string courseId, string questionId)
    {
        var answered = GetAnsweredIds(courseId);
        answered.Add(questionId);
        PlayerPrefs.SetString(Prefix + courseId, string.Join(",", answered));
        PlayerPrefs.Save();
    }

    public static int GetAnsweredCount(string courseId)
    {
        return GetAnsweredIds(courseId).Count;
    }

    public static void ClearProgress(string courseId)
    {
        PlayerPrefs.DeleteKey(Prefix + courseId);
        PlayerPrefs.Save();
    }

    public static void ClearAllProgress()
    {
        var courses = QuestionLoader.LoadCourses();
        foreach (var course in courses)
            PlayerPrefs.DeleteKey(Prefix + course.id);
        PlayerPrefs.Save();
    }
}