using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;
using System.Collections;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    // 대화창 UI 요소
    public GameObject dialogueCanvas;
    public TMP_Text nameText;
    public TMP_Text dialogueText;
    public float typingSpeed = 0.05f;

    // 대화창 이미지 UI 요소
    public Image playerImage;
    public Image npcImage;
    public CharacterData playerData;

    private Queue<DialogueData.DialogueLine> dialogueQueue;
    private Coroutine typingCoroutine;
    private bool isTyping = false;
    private bool isDialogueActive = false;

    // 이미지 오버레이 색상
    private Color activeColor = new Color(1f, 1f, 1f, 1f);
    private Color inactiveColor = new Color(0.5f, 0.5f, 0.5f, 1f);

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        // 시작할 때, 대화창 비활성화
        dialogueCanvas.SetActive(false);
        dialogueQueue = new Queue<DialogueData.DialogueLine>();

        // 플레이어 이미지를 미리 할당
        if (playerData != null)
        {
            playerImage.sprite = playerData.characterImage;
        }
    }

    void Update()
    {
        if (isDialogueActive && Input.GetKeyDown(KeyCode.Space))
        {
            DisplayNextSentence();
        }
    }

    // 대화 시작
    public void StartDialogue(DialogueData data, CharacterData npcData)
    {
        // 대화 재시작 방지
        if (isDialogueActive) return;

        // 대화 중 표시
        isDialogueActive = true;

        // 캔버스 활성화
        dialogueCanvas.SetActive(true);

        // 이전 대화 내용 초기화
        dialogueQueue.Clear();

        // 대화 내용을 큐에 넣기
        foreach (DialogueData.DialogueLine line in data.dialogueLines)
        {
            dialogueQueue.Enqueue(line);
        }
        
        // NPC 이미지 초기화
        if (npcData != null)
        {
            npcImage.sprite = npcData.characterImage;
        }

        // 첫번째 대화 시작
        DisplayNextSentence();
    }

    // 다음 문장 표시
    public void DisplayNextSentence()
    {
        // 현재 텍스트가 타이핑 중이라면, 대화 텍스트 즉시 완성
        if (isTyping)
        {
            // 큐가 비었는지 확인 (연타로 인한 버그 방지)
            if (dialogueQueue.Count > 0)
            {
                StopCoroutine(typingCoroutine);
                dialogueText.text = dialogueQueue.Peek().sentence;
            }
            isTyping = false;
            return;
        }

        if (dialogueQueue.Count == 0)
        {
            EndDialogue();
            return;
        }

        // 다음 대화를 큐에서 꺼내기
        DialogueData.DialogueLine line = dialogueQueue.Dequeue();

        // line.characterData가 null인지 확인하는 방어 코드
        if (line.characterData == null)
        {
            Debug.LogError("CharacterData is null on a dialogue line! Check your DialogueData asset.");
            EndDialogue();
            return;
        }

        if (line.characterData == playerData)
        {
            playerImage.sprite = line.characterData.characterImage;
            playerImage.color = activeColor;
            npcImage.color = inactiveColor;
        }
        else // NPC인 경우
        {
            playerImage.color = inactiveColor;
            npcImage.sprite = line.characterData.characterImage;
            npcImage.color = activeColor;
        }


        nameText.text = line.characterData.characterName;
        typingCoroutine = StartCoroutine(TypeSentence(line.sentence));
    }


    // 타이핑 효과 코루틴
    IEnumerator TypeSentence(string sentence)
    {
        isTyping = true;
        dialogueText.text = ""; // 텍스트 초기화
        foreach (char letter in sentence.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }
        isTyping = false;
    }

    // 대화 종료
    void EndDialogue()
    {
        // 캔버스 비활성화
        dialogueCanvas.SetActive(false);
        isDialogueActive = false;
    }

    // 대화가 활성화 확인 함수
    public bool IsDialogueActive()
    {
        return isDialogueActive;
    }
}
