using System;
using UnityEngine;

[CreateAssetMenu(fileName = "New Dialogue Data", menuName = "Dialogue System/Dialogue Data")]
public class DialogueData : ScriptableObject
{
    [Header("대화 메타 정보")]
    [Tooltip("이 대화의 고유 ID (선행 조건 확인용). 0은 일반 대화.")]
    public int dialogueID;

    [Header("▶ 실행 조건 및 분기")]
    public ConditionData prerequisiteCondition;

    // 조건 불만족 시 실행할 대화 데이터
    public DialogueData failSafeDialogue;

    [Header("▶ 대화 내용")]
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
