using System;

[Serializable]
public class TagData
{
    public string id;
    public string label;
    public string summary;
    public string[] links;
}

[Serializable]
public class TagDatabase
{
    public TagData[] tags;
}