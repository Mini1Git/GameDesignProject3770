using TMPro;
using UnityEngine;

public class displayLevel : MonoBehaviour
{

    public static displayLevel Instance;
    private TextMeshProUGUI levelDisplayText;


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            // Destroy this duplicate object immediately to enforce a single instance
            Destroy(gameObject);
            return;
        }

        // Set the active instance to this object
        Instance = this;
    }


    private void Start()
    {
        levelDisplayText = gameObject.GetComponentInChildren<TextMeshProUGUI>();
    }

    public void updateLevelUI()
    {
        levelDisplayText.text = $"{LevelingSystem.Instance.currentLevel}";
    }

}
