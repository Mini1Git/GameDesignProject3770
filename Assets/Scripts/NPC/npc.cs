using UnityEngine;
using UnityEngine.InputSystem;

public class npc : MonoBehaviour
{
    public string npcName = "Villager";
    [TextArea] public string[] lines;
    public dialogueBox dialogueBoxPrefab;

    dialogueBox dialogue;
    player nearbyPlayer;
    bool talking;
    int currentLine;

    void Start()
    {
        dialogue = FindAnyObjectByType<dialogueBox>();
        if (dialogue == null)
        {
            dialogue = Instantiate(dialogueBoxPrefab);
        }
    }

    void Update()
    {
        if (nearbyPlayer == null || pauseMenu.GameIsPaused) return;

        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (!talking)
            {
                startTalking();
            }
            else if (dialogue.isTyping)
            {
                dialogue.finishLine();
            }
            else
            {
                nextLine();
            }
        }
    }

    void startTalking()
    {
        if (!nearbyPlayer.canAct || lines.Length == 0) return;

        talking = true;
        currentLine = 0;
        nearbyPlayer.canAct = false;
        dialogue.showPrompt(false);
        dialogue.showLine(npcName, lines[currentLine]);
    }

    void nextLine()
    {
        currentLine++;

        if (currentLine >= lines.Length)
        {
            stopTalking();
            dialogue.showPrompt(true);
            return;
        }

        dialogue.showLine(npcName, lines[currentLine]);
    }

    void stopTalking()
    {
        talking = false;
        nearbyPlayer.canAct = true;
        dialogue.hide();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        player playerScript = collision.GetComponent<player>();
        if (playerScript == null) return;

        nearbyPlayer = playerScript;
        dialogue.showPrompt(true);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        player playerScript = collision.GetComponent<player>();
        if (playerScript == null || playerScript != nearbyPlayer) return;

        if (talking) stopTalking();
        dialogue.showPrompt(false);
        nearbyPlayer = null;
    }
}
