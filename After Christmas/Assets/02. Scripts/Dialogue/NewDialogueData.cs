using System;
using UnityEngine;

[CreateAssetMenu(menuName = "New Dialogue/Dialogue Data")]
public class NewDialogueData : ScriptableObject
{
    // 대화 UI 형태 선택
    public DialogueViewType viewType;

    [Header("View Options")]
    // public DialogueSmallOption smallOption;
    public DialogueFullScreenView fullScreenView;
   

    // 조건 불만족 시 실행할 대화 데이터
    public DialogueData failSafeDialogue;

    [Header("대화 인원")]
    // 대화에 나오는 전체 인원
    public CharacterData[] participants;

    [Serializable]
    public class DialogueLine
    {
        public CharacterData speaker;
        [TextArea(3, 10)]
        public string sentence;
    }

    public DialogueLine[] dialogueLines;
}
