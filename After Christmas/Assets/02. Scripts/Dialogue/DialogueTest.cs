using UnityEngine;

public class DialogueTest : MonoBehaviour
{
    public DialogueData testDialogue;
    public CharacterData testCharacter;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            if (testDialogue != null && DialogueManager.Instance != null)
            {
                DialogueManager.Instance.StartDialogue(testDialogue);
            }
            else
            {
                Debug.LogWarning("warning : 컴포넌트 세팅 다시하기");
            }
        }
    }
}
