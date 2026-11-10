using System;
using System.Collections.Generic;
using UnityEngine;

public enum Stats
{
    Strength,
    Vitality,
    Agility,
    Magic
}

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
    [SerializeField]
    [DictionaryDisplay(keyLabel = "Stat", valueLabel = "Level")]
    public Dictionary<Stats, int> playerStats = new Dictionary<Stats, int>() {
        {Stats.Strength, 1 },
        {Stats.Magic, 1 },
        {Stats.Agility, 1 },
        {Stats.Vitality, 1 },
    };

    public int getPlayerStat(Stats stat) { // this method is a getter for the current player's stats.
        playerStats.TryGetValue(stat, out var value);
        return value;
    }

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
    private void Start()
    {
        
        gainXP(320000); //lvl 99
    }

    public void levelUp()
    {
        currentLevel++;
        SkillSystem.Instance.skillPoints++;
    }

    public void gainXP(int xpGained)
    {
        currentXP += xpGained;
        if (currentXP >= xpToNextLVL) {
            int countLevelUps = (int) currentXP/xpToNextLVL;
            
            for (int i = 0; i < countLevelUps; i++)
            {
                
                xpToNextLVL = (int)Mathf.Pow((float)currentLevel, 2f); // increase as you level up
                if (currentXP >= (float)xpToNextLVL) {
                    levelUp();
                    currentXP = currentXP - xpToNextLVL;
                }
            }
            
            
            //curve xp to next lvl up

            
        }
    }
    
}
