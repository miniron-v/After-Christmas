using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    public enum Speaker
    {
        Player,
        NPC
    }

    [System.Serializable]
    public class DialogueLine
    {
        public Speaker speaker;
        [TextArea(2, 5)]
        public string text;
    }

    public DialogueLine[] dialogueLines;
    public TextMeshProUGUI dialogueText;

    public Image playerImage;   // Player Image
    public Image npcImage;      // NPC Image

    private int currentIndex = 0;

    void Start()
    {
        ShowLine();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            NextLine();
        }
    }

    void ShowLine()
    {
        if (currentIndex >= dialogueLines.Length) return;

        DialogueLine line = dialogueLines[currentIndex];
        dialogueText.text = line.text;

        // 캐릭터 강조 (투명도 조절)
        if (line.speaker == Speaker.Player)
        {
            SetAlpha(playerImage, 1f);
            SetAlpha(npcImage, 0.5f);
        }
        else if (line.speaker == Speaker.NPC)
        {
            SetAlpha(playerImage, 0.5f);
            SetAlpha(npcImage, 1f);
        }
    }

    void NextLine()
    {
        currentIndex++;
        if (currentIndex < dialogueLines.Length)
        {
            ShowLine();
        }
        else
        {
            dialogueText.text = "";
        }
    }

    void SetAlpha(Image img, float alpha)
    {
        Color c = img.color;
        c.a = alpha;
        img.color = c;
    }
}
