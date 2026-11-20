using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

// saves the player's progress to save.json before changing scenes and loads it in the next scene
public static class saveSystem
{
    static string getSavePath()
    {
        return Path.Combine(Application.persistentDataPath, "save.json");
    }

    // unity runs this once when the game starts
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void onGameStart()
    {
        // every new run starts fresh
        deleteSave();

        // load the save every time a scene is loaded
        SceneManager.sceneLoaded -= onSceneLoaded;
        SceneManager.sceneLoaded += onSceneLoaded;
    }

    static void onSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        load();
    }

    public static void deleteSave()
    {
        if (File.Exists(getSavePath())) File.Delete(getSavePath());
    }

    public static void save()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject == null || LevelingSystem.Instance == null || SkillSystem.Instance == null) return;

        saveData data = new saveData();

        inventory playerInventory = playerObject.GetComponent<inventory>();
        data.inventorySlots = getItemNames(playerInventory.inventorySlots);
        data.quickSlots = getItemNames(playerInventory.quickSlots);
        data.spellSlots = getItemNames(playerInventory.spellSlots);
        data.activeQuickSlot = playerInventory.activeQuickSlot;
        data.activeSpellSlot = playerInventory.activeSpellSlot;

        LevelingSystem leveling = LevelingSystem.Instance;
        data.level = leveling.currentLevel;
        data.xp = leveling.currentXP;
        data.xpToNextLevel = leveling.xpToNextLVL;
        data.statPoints = leveling.availableStatPoints;
        foreach (KeyValuePair<Stats, int> stat in leveling.playerStats)
        {
            statSave savedStat = new statSave();
            savedStat.stat = stat.Key.ToString();
            savedStat.value = stat.Value;
            data.stats.Add(savedStat);
        }

        data.skillPoints = SkillSystem.Instance.availableSkillPoints;
        foreach (KeyValuePair<skill_SO, bool> skill in SkillSystem.Instance.skillsDictionary)
        {
            if (skill.Value) data.unlockedSkills.Add(skill.Key.name);
        }

        File.WriteAllText(getSavePath(), JsonUtility.ToJson(data, true));
        Debug.Log("saved game to " + getSavePath());
    }

    public static void load()
    {
        if (!File.Exists(getSavePath())) return;

        // scenes like the main menu have no player, so there is nothing to load into
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject == null || LevelingSystem.Instance == null || SkillSystem.Instance == null) return;

        saveData data = JsonUtility.FromJson<saveData>(File.ReadAllText(getSavePath()));
        saveDatabase database = Resources.Load<saveDatabase>("saveDatabase");

        inventory playerInventory = playerObject.GetComponent<inventory>();
        loadItems(data.inventorySlots, playerInventory.inventorySlots, database);
        loadItems(data.quickSlots, playerInventory.quickSlots, database);
        loadItems(data.spellSlots, playerInventory.spellSlots, database);
        playerInventory.activeQuickSlot = data.activeQuickSlot;
        playerInventory.activeSpellSlot = data.activeSpellSlot;

        LevelingSystem leveling = LevelingSystem.Instance;
        leveling.currentLevel = data.level;
        leveling.currentXP = data.xp;
        leveling.xpToNextLVL = data.xpToNextLevel;
        leveling.availableStatPoints = data.statPoints;
        foreach (statSave savedStat in data.stats)
        {
            if (System.Enum.TryParse(savedStat.stat, out Stats stat))
            {
                leveling.playerStats[stat] = savedStat.value;
            }
        }

        SkillSystem.Instance.availableSkillPoints = data.skillPoints;
        foreach (string skillName in data.unlockedSkills)
        {
            skill_SO skill = database.findSkill(skillName);
            if (skill != null) SkillSystem.Instance.skillsDictionary[skill] = true;
        }

        // so the skill buttons show the loaded skills as unlocked
        foreach (skillButton button in Object.FindObjectsByType<skillButton>(FindObjectsInactive.Include))
        {
            button.applyColoursInEditor();
        }

        // use the loaded stats and skills, then start the level with full health, mana and stamina
        player playerScript = playerObject.GetComponent<player>();
        playerScript.updatePlayerStats();
        playerScript.activateSkills();
        playerScript.health = playerScript.maxHealth;
        playerScript.mana = playerScript.maxMana;
        playerScript.stamina = playerScript.maxStamina;
    }

    static string[] getItemNames(itemData[] slots)
    {
        string[] names = new string[slots.Length];
        for (int i = 0; i < slots.Length; i++)
        {
            names[i] = slots[i] == null ? "" : slots[i].name;
        }
        return names;
    }

    static void loadItems(string[] names, itemData[] slots, saveDatabase database)
    {
        for (int i = 0; i < slots.Length && i < names.Length; i++)
        {
            if (names[i] == "")
            {
                slots[i] = null;
            }
            else
            {
                slots[i] = database.findItem(names[i]);
                if (slots[i] == null) Debug.LogWarning("could not find item " + names[i] + ", right click the save database and press find all");
            }
        }
    }
}
