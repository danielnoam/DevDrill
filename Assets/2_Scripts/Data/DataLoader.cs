using UnityEngine;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif

public static class DataLoader
{
    private static List<QuestionData> _questionCache;
    private static List<CourseData> _courseCache;
    private static Dictionary<string, TagData> _tagCache;
    private static Dictionary<string, int> _courseQuestionCountCache;

    
    
#if UNITY_EDITOR
    [InitializeOnLoadMethod]
    private static void SubscribeToPlayModeChanges()
    {
        EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredEditMode)
            ClearCache();
    }
#endif

    
    private static Dictionary<string, int> GetCourseQuestionCounts()
    {
        if (_courseQuestionCountCache != null) return _courseQuestionCountCache;

        _courseQuestionCountCache = new Dictionary<string, int>();
        foreach (var course in LoadCourses())
            _courseQuestionCountCache[course.id] = LoadQuestionsForCourse(course).Count;

        return _courseQuestionCountCache;
    }

    private static List<QuestionData> GetAllQuestions()
    {
        if (_questionCache != null) return _questionCache;

        _questionCache = new List<QuestionData>();
        var files = Resources.LoadAll<TextAsset>("Questions");

        foreach (var file in files)
        {
            var db = JsonUtility.FromJson<QuestionDatabase>(file.text);
            _questionCache.AddRange(db.questions);
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        ValidateTags(_questionCache);
#endif

        return _questionCache;
    }

    private static void ValidateTags(List<QuestionData> questions)
    {
        var knownTags = LoadTags();

        foreach (var q in questions)
        {
            if (q.tags == null) continue;
            foreach (var tag in q.tags)
            {
                if (!knownTags.ContainsKey(tag))
                    Debug.LogWarning($"[DataLoader] Unknown tag '{tag}' on question '{q.id}' — add it to tags.json");
            }
        }
    }
    
    private static void ClearCache()
    {
        _questionCache = null;
        _courseCache = null;
        _tagCache = null;
        _courseQuestionCountCache = null;
    }

    #region Public

    public static List<QuestionData> LoadQuestions(string[] tags = null, string[] excludeTags = null, bool matchAll = false, string[] difficulties = null)
    {
        var all = GetAllQuestions();

        if (tags is { Length: > 0 })
            all = matchAll
                ? all.Where(q => tags.All(t => q.tags.Contains(t))).ToList()
                : all.Where(q => q.tags.Any(tags.Contains)).ToList();

        if (excludeTags is { Length: > 0 })
            all = all.Where(q => !q.tags.Any(excludeTags.Contains)).ToList();

        if (difficulties is { Length: > 0 })
            all = all.Where(q => difficulties.Contains(q.difficulty)).ToList();

        return all;
    }

    public static List<QuestionData> LoadQuestionsForCourse(CourseData course)
    {
        return LoadQuestions(course.tags, course.excludeTags, course.matchAll, course.difficulties);
    }
    
    public static List<CourseData> LoadCourses()
    {
        if (_courseCache != null) return _courseCache;

        _courseCache = new List<CourseData>();
        var files = Resources.LoadAll<TextAsset>("Courses");

        foreach (var file in files)
        {
            var db = JsonUtility.FromJson<CourseDatabase>(file.text);
            _courseCache.AddRange(db.courses);
        }

        return _courseCache;
    }
    
    public static Dictionary<string, TagData> LoadTags()
    {
        if (_tagCache != null) return _tagCache;

        _tagCache = new Dictionary<string, TagData>();
        var files = Resources.LoadAll<TextAsset>("Tags");

        foreach (var file in files)
        {
            var db = JsonUtility.FromJson<TagDatabase>(file.text);
            foreach (var tag in db.tags)
                _tagCache[tag.id] = tag;
        }

        return _tagCache;
    }
    
    public static int QuestionCount => GetAllQuestions().Count;

    public static int GetCourseQuestionCount(string courseId)
    {
        var counts = GetCourseQuestionCounts();
        return counts.GetValueOrDefault(courseId, 0);
    }
    
    public static TagData GetTag(string id)
    {
        return LoadTags().GetValueOrDefault(id);
    }

    public static int TagCount => LoadTags().Count;
    
    public static int CourseCount => LoadCourses().Count;

    #endregion
}