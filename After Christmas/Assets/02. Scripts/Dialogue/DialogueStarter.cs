using UnityEngine;

public class DialogueStarter: MonoBehaviour
{
    public DialogueData dialogueData;

    public GameObject speechBubble;

    private bool isPlayerInRange = false;

    public string playerTag = "Player";

    private void Awake()
    {
        if (speechBubble != null)
        {
            speechBubble.SetActive(false);
        }
    }

    void Update()
    {
        if (isPlayerInRange && Input.GetKeyDown(KeyCode.E))
        {
            if (dialogueData != null)
            {
                if (speechBubble != null)
                {
                    speechBubble.SetActive(false);
                }

                DialogueManager.Instance.StartDialogue(dialogueData);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            Debug.Log("Dialogue Trigger Enter");
            isPlayerInRange = true;

            if (speechBubble != null)
            {
                speechBubble.SetActive(true);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            Debug.Log("Dialogue Trigger Exit");
            isPlayerInRange = false;


            if (speechBubble != null)
            {
                speechBubble.SetActive(false);
            }
        }
    }
}
