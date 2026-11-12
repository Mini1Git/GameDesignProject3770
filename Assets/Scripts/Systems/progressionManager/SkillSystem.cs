using JetBrains.Annotations;
using System.Collections.Generic;
using UnityEngine;

public class SkillSystem : MonoBehaviour
{
    public static SkillSystem Instance;
    public int skillPoints;

    [SerializeField]
    [DictionaryDisplay(keyLabel = "Skill", valueLabel = "Unlocked")]
    public Dictionary<skill_SO, bool> skills = new Dictionary<skill_SO, bool>(); // WOOOOO SERIAZIABLE DICTIONARIES
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

       
    }

    public void gainSkillPoint() {
        skillPoints++;
    }
    private void Start()
    {
        
        
        
    }
    public bool skillUnlock(skill_SO skill)// this function basically is a skill req check, it goes through all the skills, and check if the current player
                                            // stats are higher than the stat reqs, if ALL the stat reqs for the skill is smaller or equal to player stats,
                                            // then unlock the skill.
    {
        foreach (KeyValuePair<Stats, int> requiredStat in skill.requiredStats)
        {
            if (LevelingSystem.Instance.getPlayerStat(requiredStat.Key) >= requiredStat.Value)
            {
                //Debug.Log("MET THE REQS FOR: " + requiredStat.Key);
            }
            else {
                //Debug.Log("DID NOT MEET THE REQS FOR: " + requiredStat.Key);
                return false;
            }
        }
        return true; // means it met all reqs
    }
}
