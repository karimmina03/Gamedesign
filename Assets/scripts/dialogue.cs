using TMPro;
using UnityEngine;

public class DialogueUI : MonoBehaviour
{
    public GameObject dialogueBox;
    public TMP_Text dialogueText;

    public void ShowDialogue(string text)
    {
        dialogueText.text = text;
        dialogueBox.SetActive(true);
    }

    public void HideDialogue()
    {
        dialogueBox.SetActive(false);
    }
}
