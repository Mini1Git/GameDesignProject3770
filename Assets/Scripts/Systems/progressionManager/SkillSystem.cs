using UnityEngine;

public class SkillSystem : MonoBehaviour
{
    public static SkillSystem Instance;
    public int skillPoints;
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

        // Optional: Keep this object alive when switching scenes
        DontDestroyOnLoad(gameObject);
    }

    public void gainSkillPoint() {
        skillPoints++;
    }
}
