using System;
using System.Collections.Generic;

// everything that gets written into save.json
[Serializable]
public class saveData
{
    // items are saved by their asset name, empty slots are ""
    public string[] inventorySlots;
    public string[] quickSlots;
    public string[] spellSlots;
    public int activeQuickSlot;
    public int activeSpellSlot;

    public int level;
    public int xp;
    public int xpToNextLevel;
    public int statPoints;
    public List<statSave> stats = new List<statSave>();

    public int skillPoints;
    public List<string> unlockedSkills = new List<string>();
}

[Serializable]
public class statSave
{
    public string stat;
    public int value;
}
