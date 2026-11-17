using NUnit.Framework.Constraints;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class skillButton : MonoBehaviour
{
    private skillButton_Settings buttonColours;
    private Button thisButton;
    public skill_SO associatedSkill;

    public void Start()
    {
        thisButton = GetComponent<Button>();
        thisButton.onClick.AddListener(skillCheck);
        buttonColours = GetComponentInParent<skillButton_Settings>();
    }

    [ContextMenu("Preview Unlocked Colours")]
    public void applyColoursInEditor() // this modifies the colours when changing in inspector. But please use skillButton_Settings when changing since all buttons should look the same.
    {

        

        if (thisButton == null)
            thisButton = GetComponent<Button>();



        if (buttonColours == null)
            buttonColours = GetComponentInParent<skillButton_Settings>();

        Navigation nav = thisButton.navigation; // so that you can't focus select.

        nav.mode = Navigation.Mode.None;

        thisButton.navigation = nav;

        if (thisButton != null && buttonColours != null)
        {
            ColorBlock cb = thisButton.colors;
            // Only check the dictionary if the game is running and SkillSystem exists
            if (Application.isPlaying && SkillSystem.Instance != null && SkillSystem.Instance.skillsDictionary != null && associatedSkill != null)
            {
                if (SkillSystem.Instance.skillsDictionary.ContainsKey(associatedSkill))
                {
                    if (SkillSystem.Instance.skillsDictionary[associatedSkill])
                    {
                        cb.normalColor = buttonColours.unlockColour;
                    }
                    else
                    {
                        cb.normalColor = buttonColours.normalColour;
                    }
                }
                
            }
            else
            {
                cb.normalColor = buttonColours.normalColour;
            }
            
            cb.pressedColor = buttonColours.pressedColour;
            cb.highlightedColor = buttonColours.highlightedColour;
            thisButton.colors = cb;
        }
    }

    public void skillCheck()
    {

        if (associatedSkill == null)
        {
            Debug.LogWarning($"The skill prefab has not been added to {gameObject.name}!");
            return;
        }
        if (SkillSystem.Instance.availableSkillPoints <= 0)
        {
            // cannot get skill
            Debug.Log("Not enough skill points");
            return;
        }

        
        

        bool skillUnlocked = SkillSystem.Instance.skillUnlock(associatedSkill);
        player player = GameObject.FindGameObjectWithTag("Player").GetComponent<player>();

        if (skillUnlocked)
        {
            
            
            Debug.Log("UNLOCKED SKILL: " + associatedSkill);
            SkillSystem.Instance.skillsDictionary[associatedSkill] = skillUnlocked;


        }
        else
        {
            SkillSystem.Instance.skillsDictionary[associatedSkill] = skillUnlocked;
        }


        // update ui/colour and activate skills 
        applyColoursInEditor();
        displaySkillPoints.Instance.updateSkillUI();
        player.activateSkills();



    }
}
