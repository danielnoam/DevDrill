using System.Collections.Generic;
using UnityEngine;

public static class ProgressManager
{
    private const string Prefix = "progress_";
    private const char Separator = ',';

    public static HashSet<string> GetAnsweredIds(string courseId)
    {
        var raw = PlayerPrefs.GetString(Prefix + courseId, "");
        var result = new HashSet<string>();

        if (string.IsNullOrEmpty(raw)) return result;

        foreach (var id in raw.Split(Separator))
        {
            var trimmed = id.Trim();
            if (trimmed.Length > 0)
                result.Add(trimmed);
        }

        return result;
    }

    public static void SaveAnswered(string courseId, string questionId)
    {
        var answered = GetAnsweredIds(courseId);
        if (!answered.Add(questionId)) return;
        
        PlayerPrefs.SetString(Prefix + courseId, string.Join(Separator.ToString(), answered));
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
        foreach (var course in DataLoader.LoadCourses())
            PlayerPrefs.DeleteKey(Prefix + course.id);
        PlayerPrefs.Save();
    }
}