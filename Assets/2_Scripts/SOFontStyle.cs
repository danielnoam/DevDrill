using DNExtensions.Utilities;
using TMPro;
using UnityEngine;

[CreateAssetMenu(fileName = "New FontStyle", menuName = "Scriptable Objects/Font Style", order = 1)]
public class SOFontStyle : ScriptableObject
{
    [SerializeField] private TMP_FontAsset font;
    [SerializeField] private  Color fontColor = Color.lightGreen;
    [SerializeField] private  int fontSizeMultiplier = 85;
    

    public string ApplyStyle(string text)
    {
        var rich = text.Rich().Color(fontColor.ToHex()).Size($"{fontSizeMultiplier}%");
        return font ? rich.Font(font.name).ToString() : rich.ToString();
    }
}