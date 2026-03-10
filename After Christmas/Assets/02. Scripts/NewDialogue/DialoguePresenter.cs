using UnityEngine;
using static DialogueData;

public class DialoguePresenter
{
    private readonly IDialogueView view;
    private DialogueModel model;

    public DialoguePresenter(IDialogueView view)
    {
        this.view = view;
    }

    public void StartDialogue(DialogueData dialogueData)
    {
        model = new DialogueModel(dialogueData);

        if (!model.HasLine)
        {
            EndDialogue();
            return;
        }

        view.Clear();
        view.Show();
        ShowCurrentLine();
    }

    public void OnNext()
    {
        if (model == null)
            return;

        if (view.IsTyping)
        {
            view.FinishTyping();
            return;
        }

        if (!view.IsComplete)
            return;

        if (model.HasNext)
        {
            model.MoveNext();
            ShowCurrentLine();
        }
        else
        {
            EndDialogue();
        }
    }

    private void ShowCurrentLine()
    {
        DialogueLine line = model.CurrentLine;

        string speakerName = line.speaker != null ? line.speaker.characterName : string.Empty;
        Sprite portrait = line.speaker != null ? line.speaker.characterImage : null;

        view.SetSpeakerName(speakerName);
        view.SetPortrait(portrait);
        view.StartTyping(line.sentence);
    }

    private void EndDialogue()
    {
        view.Clear();
        view.Hide();
        model = null;
    }
}