using static DialogueData;

public class DialogueModel
{
    private readonly DialogueData dialogueData;
    private int currentIndex;

    public DialogueModel(DialogueData dialogueData)
    {
        this.dialogueData = dialogueData;
        currentIndex = 0;
    }

    public bool HasLine =>
        dialogueData != null &&
        dialogueData.dialogueLines != null &&
        dialogueData.dialogueLines.Length > 0;

    public bool HasNext =>
        dialogueData != null &&
        currentIndex + 1 < dialogueData.dialogueLines.Length;

    public DialogueLine CurrentLine =>
        dialogueData.dialogueLines[currentIndex];

    public void MoveNext()
    {
        if (HasNext)
            currentIndex++;
    }

    public void Reset()
    {
        currentIndex = 0;
    }
}