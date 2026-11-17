using System.Collections;
using TMPro;
using UnityEngine;

public class dialogueBox : MonoBehaviour
{
    public GameObject panel;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI lineText;
    public GameObject talkPrompt;
    public float letterDelay = 0.03f;

    public bool isTyping { get; private set; }

    string fullLine;

    private void Awake()
    {
        panel.SetActive(false);
        talkPrompt.SetActive(false);
    }

    public void showLine(string speakerName, string line)
    {
        panel.SetActive(true);
        nameText.text = speakerName;
        fullLine = line;

        StopAllCoroutines();
        StartCoroutine(typeLine());
    }

    public void finishLine()
    {
        StopAllCoroutines();
        lineText.text = fullLine;
        isTyping = false;
    }

    public void hide()
    {
        StopAllCoroutines();
        isTyping = false;
        panel.SetActive(false);
    }

    public void showPrompt(bool show)
    {
        talkPrompt.SetActive(show);
    }

    IEnumerator typeLine()
    {
        isTyping = true;
        lineText.text = "";

        foreach (char letter in fullLine)
        {
            lineText.text += letter;
            yield return new WaitForSeconds(letterDelay);
        }

        isTyping = false;
    }
}
