using UnityEngine;

public class LevelingSystem : MonoBehaviour
{
    
    public static LevelingSystem Instance;
    [Header("Level settings")]
    public int currentLevel = 1; // this is the current level of the player
    public int currentXP;
    public int xpToNextLVL;
    [Header("Stat points")]
    public int statPoints = 0;
    [Header("Player Stats")]
    public int strength = 1;
    public int vitality = 1;
    public int agility = 1;
    public int magic = 1;
    public int endurance = 1;

    

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

    public void levelUp()
    {
        currentLevel++;
    }
    
}
