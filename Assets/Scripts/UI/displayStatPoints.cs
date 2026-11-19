using UnityEngine;
using System;
using TMPro;

public class displayStatPoints : MonoBehaviour
{
    public static displayStatPoints Instance;
    public TextMeshProUGUI statPointsText;
    [TextArea(4, 5)]
    public string importantDesignReminder = "DO NOT CHANGE THE ORDERING OF THE INTERNAL STAT UI, the stat's internal order must be strictly in this order: \n1. Button \n2. Text";
    GameObject[] stats; // using array because size doesnt change for stats
    [SerializeField]
    player player;
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


        stats = new GameObject[transform.childCount]; // initialize array with count

        for (int i = 0; i < stats.Length; i++)
        {
            stats[i] = transform.GetChild(i).gameObject;
        }
    }

    public void updateStatUI()
    {
        player = LevelingSystem.Instance.player;
        //update player stats 
        player.updatePlayerStats();

        statPointsText.text = $"Available Stat Points: {LevelingSystem.Instance.availableStatPoints}";
        foreach (GameObject stat in stats)
        {
            if (Enum.TryParse(typeof(Stats), stat.name, out var enumStat)) // so since the gameobject names are strings, but also match, we can parse it to match 
                                                                            //the stat enum.
            {
                
                // so now we get the enum result, enumStat
                stat.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = $"{enumStat}: {LevelingSystem.Instance.playerStats[(Stats)enumStat]}";
                
            }

        }
    }
}
