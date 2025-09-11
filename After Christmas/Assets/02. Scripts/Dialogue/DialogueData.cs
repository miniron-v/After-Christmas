using System;
using UnityEngine;

[CreateAssetMenu(fileName = "New Dialogue Data", menuName = "Dialogue System/Dialogue Data")]
public class DialogueData : ScriptableObject
{
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
