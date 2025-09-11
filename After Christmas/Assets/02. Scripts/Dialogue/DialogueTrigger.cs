using System.Collections.Generic;
using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    public List<DialogueData> dialogueSequence;

    private bool isPlayerInRange = false;
    private int currentDialogueIndex = 0;

    public string playerTag = "Player";

    void Update()
    {
        if (isPlayerInRange && Input.GetKeyDown(KeyCode.E))
        {
            if (currentDialogueIndex < dialogueSequence.Count)
            {
                DialogueManager.Instance.StartDialogue(dialogueSequence[currentDialogueIndex]);
                currentDialogueIndex++;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            Debug.Log("Dialogue Trigger Enter");
            isPlayerInRange = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            Debug.Log("Dialogue Trigger Exit");
            isPlayerInRange = false;
        }
    }
}
