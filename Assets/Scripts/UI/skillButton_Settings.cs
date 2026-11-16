using UnityEngine;
using UnityEngine.UI;

public class skillButton_Settings : MonoBehaviour
{
    public Color normalColour;
    public Color unlockColour;
    public Color pressedColour;
    public Color highlightedColour;

    private void OnValidate()//this updates the buttons in when modifying the colours during edit time. Use this rather than individual. 
    {
        foreach (var btn in GetComponentsInChildren<skillButton>())
        {
            btn.applyColoursInEditor();
        }
    }
}
