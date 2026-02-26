using System.Text.RegularExpressions;
using DNExtensions.Utilities;
using TMPro;
using UnityEngine;

[CreateAssetMenu(fileName = "CodeBlockStyle", menuName = "Scriptable Objects/CodeBlockStyle", order = 1)]
public class SOCodeBlockStyle : ScriptableObject
{
    [SerializeField] private TMP_FontAsset font;
    [SerializeField] private  Color fontColor = Color.lightGreen;
    [SerializeField] private  int fontSizeMultiplier = 85;
    
    
    
    public string ParseCodeTags(string text)
    {
        string fontName = font? font.name : "LiberationSans SDF";

        return Regex.Replace(text, @"`([^`]+)`", m => 
            m.Groups[1].Value.Rich()
                .Font(fontName)
                .Color(fontColor.ToHex())
                .Size($"{fontSizeMultiplier}%")
                .ToString());
    }
}