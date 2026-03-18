using System;

[Serializable]
public class TagLink
{
    public string label;
    public string url;
}

[Serializable]
public class TagData
{
    public string id;
    public string label;
    public string summary;
    public TagLink[] links;
}

[Serializable]
public class TagDatabase
{
    public TagData[] tags;
}