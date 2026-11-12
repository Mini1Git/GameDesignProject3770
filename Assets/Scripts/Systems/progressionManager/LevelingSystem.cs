using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


public enum Stats
{
    Strength,
    Vitality,
    Agility,
    Magic, 
    Endurance
}

public class LevelingSystem : MonoBehaviour
{
    
    public static LevelingSystem Instance;
    [Header("Level settings")]
    public int currentLevel = 1; // this is the current level of the player
    public int currentXP;
    public int xpToNextLVL;

    // player inputs here
    InputAction openLevelMenuInput;
    PlayerInput playerInput;
    bool openLeveling = false;

    [Header("Stat points")]
    public int availableStatPoints = 0;
    [Header("Player Stats")]
    //this dictionary stores the stats that a player can upgrade, and it's corresponding level.
    [SerializeField]
    [DictionaryDisplay(keyLabel = "Stat", valueLabel = "Level")]
    public Dictionary<Stats, int> playerStats = new Dictionary<Stats, int>() {
        {Stats.Strength, 1 },
        {Stats.Magic, 1 },
        {Stats.Agility, 1 },
        {Stats.Vitality, 1 },
        {Stats.Endurance, 1 }
    };

    GameObject skillMenu;
    public int getPlayerStat(Stats stat) { // this method is a getter for the current player's specific stat.
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

        


        //playerInput initialization, always need to do this.
        playerInput = new PlayerInput();
    }
    private void Start()
    {
        
        gainXP(320000); //lvl 99
        skillMenu = GameObject.FindGameObjectWithTag("skillUI"); // this gets and initially sets the skilmenu to inactive.
        skillMenu.SetActive(false);

    }
    private void OnEnable()
    {
        playerInput.Enable();
        openLevelMenuInput = playerInput.PlayerGame.toggleInventory; // ik it says toggleInventory but still same. "tab"
        
        openLevelMenuInput.performed += toggleLevelSystem; 
    }
    public void toggleLevelSystem(InputAction.CallbackContext ctx)
    {
        skillMenu.SetActive(!openLeveling);
        openLeveling = !openLeveling; // flips each time u tab, in sync with inventory.
    }

    private void OnDisable()
    {
        playerInput.Disable();
    }
    public void levelUp()
    {
        currentLevel++;
        SkillSystem.Instance.skillPoints++;
    }

    public void increaseVitality()
    {
        if (availableStatPoints == 0)
        {
            return;
        }
        playerStats[Stats.Vitality] += 1;
        availableStatPoints--;
    }
    public void increaseEndurance()
    {
        if (availableStatPoints == 0)
        {
            return;
        }
        playerStats[Stats.Endurance] += 1;
        availableStatPoints--;
    }
    public void increaseStrength()
    {
        if (availableStatPoints == 0)
        {
            return;
        }
        playerStats[Stats.Strength] += 1;
        availableStatPoints--;
    }
    public void increaseAgility()
    {
        if (availableStatPoints == 0)
        {
            return;
        }
        playerStats[Stats.Agility] += 1;
        availableStatPoints--;
    }
    public void increaseMagic()
    {
        if (availableStatPoints == 0)
        {
            return;
        }
        playerStats[Stats.Magic] += 1;
        availableStatPoints--;
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
