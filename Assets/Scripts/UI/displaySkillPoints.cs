using TMPro;
using UnityEngine;

public class displaySkillPoints : MonoBehaviour
{
    public static displaySkillPoints Instance;
    TextMeshProUGUI skillpointText;
    public void Awake()
    {
        // Check if an instance already exists
        if (Instance != null && Instance != this)
        {
            // Destroy this duplicate object immediately to enforce a single instance
            Destroy(gameObject);
            return;
        }

        // Set the active instance to this object
        Instance = this;
        skillpointText = GetComponent<TextMeshProUGUI>();
    }
    

    public void updateSkillUI()
    {
        skillpointText.text = $"Available Skill Points: {SkillSystem.Instance.availableSkillPoints.ToString()}";
    }

}
