using System;

[Serializable]
public class CourseData
{
    public string id;
    public string name;
    public string description;
    public string[] tags;
    public string[] excludeTags;
    public bool matchAll;

}

[Serializable]
public class CourseDatabase
{
    public CourseData[] courses;
}