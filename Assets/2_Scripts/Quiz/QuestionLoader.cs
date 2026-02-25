using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public static class QuestionLoader
{
    public static List<QuestionData> LoadQuestions(string[] tags = null, string[] excludeTags = null, bool matchAll = false)
    {
        var all = new List<QuestionData>();
        var files = Resources.LoadAll<TextAsset>("Questions");
    
        foreach (var file in files)
        {
            var db = JsonUtility.FromJson<QuestionDatabase>(file.text);
            all.AddRange(db.questions);
        }

        if (tags is { Length: > 0 })
            all = matchAll
                ? all.Where(q => tags.All(t => q.tags.Contains(t))).ToList()
                : all.Where(q => q.tags.Any(tags.Contains)).ToList();

        if (excludeTags is { Length: > 0 })
            all = all.Where(q => !q.tags.Any(excludeTags.Contains)).ToList();

        return all;
    }

    public static List<CourseData> LoadCourses()
    {
        var files = Resources.LoadAll<TextAsset>("Courses");
        var all = new List<CourseData>();

        foreach (var file in files)
        {
            var db = JsonUtility.FromJson<CourseDatabase>(file.text);
            all.AddRange(db.courses);
        }

        return all;
    }

    public static List<QuestionData> LoadQuestionsForCourse(CourseData course)
    {
        return LoadQuestions(course.tags, course.excludeTags, course.matchAll);
    }
}