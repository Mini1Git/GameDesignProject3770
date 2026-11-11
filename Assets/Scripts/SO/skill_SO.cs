using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "skill_SO", menuName = "Skills/skill_SO")]
public class skill_SO : ScriptableObject
{
    
    [SerializeField]
    public Dictionary<Stats, int> requiredStats = new Dictionary<Stats, int>();
}
