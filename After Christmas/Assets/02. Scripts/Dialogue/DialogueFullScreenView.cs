using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueFullScreenView : MonoBehaviour, IDialogueView
{
    [Header("UI")]
    [SerializeField] private GameObject root;
    [SerializeField] private TMP_Text speakerText;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private Image portraitImage;
    [SerializeField] private Image nextIcon;

    [Header("Typing")]
    [SerializeField] private float typingDelay = 0.03f;

    private Coroutine typingCoroutine;
    private string currentText = string.Empty;
    private bool isTyping;
    private bool isComplete;

    public bool IsTyping => isTyping;
    public bool IsComplete => isComplete;

    // UI 시작 & 끝
    #region
    public void Show()
    {
        root.SetActive(true);
    }

    public void Hide()
    {
        root.SetActive(false);
    }
    #endregion

    // UI 초기화
    public void Clear()
    {
        StopTyping();
        ClearText();
        ClearPortrait();
        ClearState();
        SetNextVisible(false);
    }
    
    // 대화 UI 업데이트
    public void SetSpeakerName(string speakerName)
    {
        speakerText.text = speakerName;
    }

    public void SetPortrait(Sprite portrait)
    {
        portraitImage.sprite = portrait;
        portraitImage.gameObject.SetActive(portrait != null);
    }

    public void SetNextVisible(bool visible)
    {
        nextIcon.gameObject.SetActive(visible);
    }

    public void SetDialogueText(string text)
    {
        dialogueText.text = text;
    }

    public void StartTyping(string text)
    {
        StopTyping();
        typingCoroutine = StartCoroutine(TypeText(text));
    }

    public void FinishTyping()
    {
        if (!isTyping)
            return;

        StopTyping();
        SetDialogueText(currentText);
        SetCompleteState();
    }

    private IEnumerator TypeText(string text)
    {
        BeginTyping(text);

        foreach (char c in text)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(typingDelay);
        }

        typingCoroutine = null;
        SetCompleteState();
    }

    private void BeginTyping(string text)
    {
        currentText = text;
        dialogueText.text = string.Empty;
        SetTypingState();
        SetNextVisible(false);
    }

    private void StopTyping()
    {
        if (typingCoroutine == null)
            return;

        StopCoroutine(typingCoroutine);
        typingCoroutine = null;
    }

    private void SetTypingState()
    {
        isTyping = true;
        isComplete = false;
    }

    private void SetCompleteState()
    {
        isTyping = false;
        isComplete = true;
        SetNextVisible(true);
    }

    private void ClearState()
    {
        currentText = string.Empty;
        isTyping = false;
        isComplete = false;
    }

    private void ClearText()
    {
        speakerText.text = string.Empty;
        dialogueText.text = string.Empty;
    }

    private void ClearPortrait()
    {
        portraitImage.sprite = null;
        portraitImage.gameObject.SetActive(false);
    }
}