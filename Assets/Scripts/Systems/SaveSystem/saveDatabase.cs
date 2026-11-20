using UnityEngine;

// a list of every item and skill so a saved name can be turned back into the real asset
[CreateAssetMenu(fileName = "saveDatabase", menuName = "Save/Save Database")]
public class saveDatabase : ScriptableObject
{
    public itemData[] items;
    public skill_SO[] skills;

    public itemData findItem(string itemName)
    {
        foreach (itemData item in items)
        {
            if (item != null && item.name == itemName) return item;
        }
        return null;
    }

    public skill_SO findSkill(string skillName)
    {
        foreach (skill_SO skill in skills)
        {
            if (skill != null && skill.name == skillName) return skill;
        }
        return null;
    }

#if UNITY_EDITOR
    // right click this asset and press find all after making a new item or skill
    [ContextMenu("Find All")]
    void findAll()
    {
        string[] itemIds = UnityEditor.AssetDatabase.FindAssets("t:itemData");
        items = new itemData[itemIds.Length];
        for (int i = 0; i < itemIds.Length; i++)
        {
            string path = UnityEditor.AssetDatabase.GUIDToAssetPath(itemIds[i]);
            items[i] = UnityEditor.AssetDatabase.LoadAssetAtPath<itemData>(path);
        }

        string[] skillIds = UnityEditor.AssetDatabase.FindAssets("t:skill_SO");
        skills = new skill_SO[skillIds.Length];
        for (int i = 0; i < skillIds.Length; i++)
        {
            string path = UnityEditor.AssetDatabase.GUIDToAssetPath(skillIds[i]);
            skills[i] = UnityEditor.AssetDatabase.LoadAssetAtPath<skill_SO>(path);
        }

        UnityEditor.EditorUtility.SetDirty(this);
    }
#endif
}
