using JetBrains.Annotations;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
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

        // Optional: Keep this object alive when switching scenes
        DontDestroyOnLoad(gameObject);
    }

    public void gainSkillPoint() {
        skillPoints++;
    }
    private void Start()
    {
        
        skills = skills.ToDictionary(skill => skill.Key, skill => skillUnlock(skill.Key)); // this updates all skills, and checks reqs using LINQ lib.
        
    }
    public bool skillUnlock(skill_SO skill)
    {
        foreach (KeyValuePair<Stats, int> requiredStat in skill.requiredStats)
        {
            if (LevelingSystem.Instance.getPlayerStat(requiredStat.Key) >= requiredStat.Value)
            {
                Debug.Log("MET THE REQS FOR: " + requiredStat.Key);
            }
            else {
                Debug.Log("DID NOT MEET THE REQS FOR: " + requiredStat.Key);
                return false;
            }
        }
        return true; // means it met all reqs
    }
}
