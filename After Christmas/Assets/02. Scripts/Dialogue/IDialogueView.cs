using UnityEngine;

public interface IDialogueView
{
    // 텍스트 타이핑 관련 프로퍼티
    bool IsTyping { get; }
    bool IsComplete { get; }

    // UI 시작 & 종료
    void Show();
    void Hide();

    // 대화 문장 제외 UI 설정 ( 이름 / 이미지 / 대화 다음 아이콘 보이는 여부 ) 
    void SetSpeakerName(string speakerName);
    void SetPortrait(Sprite portrait);
    void SetNextVisible(bool visible);
    
    // 대화 문장을 타이핑 없이 업데이트
    void SetDialogueText(string text);
    // 대화 문장을 타이핑하면서 업데이트
    void StartTyping(string text);
    // 타이핑하는 문장을 즉시 완료
    void FinishTyping();

    // UI Clear
    void Clear();

}