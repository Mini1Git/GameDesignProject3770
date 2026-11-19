using System.Collections.Generic;
using UnityEngine;

public class SkillSystem : MonoBehaviour
{
    public static SkillSystem Instance;
    public int availableSkillPoints;

    public skill_SO[] skillsList;
    [SerializeField]
    [DictionaryDisplay(keyLabel = "Skill", valueLabel = "Unlocked")]
    public Dictionary<skill_SO, bool> skillsDictionary = new Dictionary<skill_SO, bool>(); // WOOOOO SERIAZIABLE DICTIONARIES
    // TO DO, check if need to use list for the UI, then convert to dictionary? if so how. 
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
        availableSkillPoints++;
    }
    private void Start()
    {
        
        
        
    }
    public bool skillUnlock(skill_SO skill){// this function basically is a skill req check, it goes through all the skills, and check if the current player
                                            // stats are higher than the stat reqs, if ALL the stat reqs for the skill is smaller or equal to player stats,
                                            // then unlock the skill.

        if (skillsDictionary.TryGetValue(skill, out var unlock))
        {
            if (unlock)
            {
                return true; // if we alr have it unlocked, then we skip minus skillpoint.
            }
        }
        
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
        Debug.Log(availableSkillPoints);
        availableSkillPoints--;
        return true; // means it met all reqs
    }
}
