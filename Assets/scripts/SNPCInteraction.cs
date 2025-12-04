using UnityEngine;
using TMPro;

public class SNPCInteraction : MonoBehaviour
{
    public float interactDistance = 3f;
    public Transform player;

    public TextMeshProUGUI interactText;
    public TextMeshProUGUI dialogueText;

    private bool isPlayerNearby = false;
    private bool dialogueActive = false;

    private void Update()
    {
        float dist = Vector3.Distance(player.position, transform.position);

        // Player enters interaction range
        if (dist < interactDistance && !dialogueActive)
        {
            if (!isPlayerNearby)
            {
                interactText.gameObject.SetActive(true);
                isPlayerNearby = true;
            }

            if (Input.GetKeyDown(KeyCode.G))
            {
                StartDialogue();
            }
        }
        else
        {
            if (isPlayerNearby && !dialogueActive)
            {
                interactText.gameObject.SetActive(false);
                isPlayerNearby = false;
            }
        }
    }

    void StartDialogue()
    {
        dialogueActive = true;
        interactText.gameObject.SetActive(false);

        StartCoroutine(DialogueSequence());
    }

    private System.Collections.IEnumerator DialogueSequence()
    {
        dialogueText.gameObject.SetActive(true);

        // Player line
        dialogueText.text = "Hey,is there anything helpful here?";
        yield return new WaitForSeconds(2f);

        // NPC line
        dialogueText.text = "Look on my right!";
        yield return new WaitForSeconds(2f);

        // End dialogue
        dialogueText.gameObject.SetActive(false);
        dialogueActive = false;
    }
}
