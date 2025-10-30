using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    public GameObject dialogueCanvas;
    public TMP_Text nameText;
    public TMP_Text dialogueText;
    public string currentSentence;
    public float typingSpeed = 0.05f;

    public List<Image> characterImageSlots;
    public CharacterData playerData;

    private Queue<DialogueData.DialogueLine> dialogueQueue;
    private Coroutine typingCoroutine;
    private bool isTyping = false;
    private bool isDialogueActive = false;
    private DialogueData currentDialogueData;

    private Dictionary<CharacterData, Image> characterImageMap;

    private Color activeColor = new Color(1f, 1f, 1f, 1f);
    private Color inactiveColor = new Color(0.5f, 0.5f, 0.5f, 1f);

    public Action onDialogueEnd;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(transform.root.gameObject);
        }
        else
        {
            Destroy(transform.root.gameObject);
        }

        dialogueCanvas.SetActive(false);
        dialogueQueue = new Queue<DialogueData.DialogueLine>();
        characterImageMap = new Dictionary<CharacterData, Image>();
    }

    void Update()
    {
        if (isDialogueActive && Input.GetKeyDown(KeyCode.Space))
        {
            if (isTyping)
            {
                DisplayNextSentence();
            }
        }
    }

    // Dialogue 시작
    public IEnumerator StartDialogue(DialogueData data, bool isCutscene = false)
    {
        if (isDialogueActive) yield break;

        if (data.prerequisiteCondition != null)
        {
            // ItemInfoManager의 CheckCondition 메서드를 사용
            bool isMet = data.prerequisiteCondition.CheckCondition(ItemInfoManager.Instance);

            if (!isMet)
            {
                // 조건 불만족
                if (data.failSafeDialogue != null)
                {
                    Debug.Log($"Dialogue ID {data.dialogueID} 조건 불만족. Fail-safe Dialogue로 분기.");
                    StartDialogue(data.failSafeDialogue);
                    yield break;
                }
                Debug.Log($"Dialogue ID {data.dialogueID} 조건 불만족. 대화 실행 취소.");
                yield break;
            }
        }

        currentDialogueData = data;
        isDialogueActive = true;
        dialogueCanvas.SetActive(true);

        // 대화 시작 시 플레이어 상태 DIALOGUE로 전환
        PlayerStateManager.Instance?.SetState(PlayerState.Dialogue);

        dialogueQueue.Clear();
        characterImageMap.Clear();

        // 이미지 슬롯 초기화
        foreach (var slot in characterImageSlots)
        {
            slot.gameObject.SetActive(false);
        }

        int slotIndex = 0;
        foreach (CharacterData character in data.participants)
        {
            if (slotIndex >= characterImageSlots.Count) break;

            Image currentSlot = characterImageSlots[slotIndex];
            if (character != null && character.characterImage != null)
            {
                currentSlot.gameObject.SetActive(true);
                characterImageMap[character] = currentSlot;
                currentSlot.sprite = character.characterImage;
                currentSlot.color = inactiveColor;
                slotIndex++;
            }
        }

        // 대화할 상대가 없다면, player Image를 표시하지 않음
        if (data.participants.Length > 0)
        {
            if (playerData != null && playerData.characterImage != null && !characterImageMap.ContainsKey(playerData))
            {
                if (slotIndex < characterImageSlots.Count)
                {
                    Image nextSlot = characterImageSlots[slotIndex];
                    nextSlot.gameObject.SetActive(true);
                    characterImageMap[playerData] = nextSlot;
                    nextSlot.sprite = playerData.characterImage;
                    nextSlot.color = inactiveColor;
                }
            }
        }

        // 대화 시작 문장 꺼내기
        foreach (DialogueData.DialogueLine line in data.dialogueLines)
        {
            dialogueQueue.Enqueue(line);
        }

        DisplayNextSentence();

        while (dialogueQueue.Count > 0)
        {
            DialogueData.DialogueLine line = dialogueQueue.Dequeue();

            yield return StartCoroutine(RunSentenceFlow(line));
        }

        EndDialogue();
    }

    private IEnumerator RunSentenceFlow(DialogueData.DialogueLine line)
    {
        currentSentence = line.sentence;

        foreach (var kvp in characterImageMap)
        {
            if (kvp.Key == line.speaker)
            {
                kvp.Value.color = activeColor;
            }
            else
            {
                kvp.Value.color = inactiveColor;
            }
        }

        string speakerName = line.speaker?.characterName;
        if (line.speaker == null || string.IsNullOrEmpty(speakerName))
        {
            nameText.gameObject.SetActive(false);
        }
        else
        {
            nameText.gameObject.SetActive(true);
            nameText.text = speakerName;
        }

        typingCoroutine = StartCoroutine(TypeSentence(line.sentence));
        yield return typingCoroutine;

        bool inputReceived = false;
        while (!inputReceived)
        {
            if (!isTyping && Input.GetKeyDown(KeyCode.Space))
            {
                inputReceived = true;
            }
            yield return null;
        }
    }

    // 다음 문장 실행
    public void DisplayNextSentence()
    {
        if (isTyping)
        {
            Coroutine tempCoroutine = typingCoroutine;
            typingCoroutine = null;

            isTyping = false;

            if (typingCoroutine != null)
            {
                StopCoroutine(tempCoroutine);
            }
            dialogueText.text = currentSentence;
        }
    }

    // 타이핑 효과 코루틴
    IEnumerator TypeSentence(string sentence)
    {
        isTyping = true;
        dialogueText.text = "";
        foreach (char letter in sentence.ToCharArray())
        {
            if (!isTyping) break;

            dialogueText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }
        isTyping = false;
    }

    // dialogue 종료
    public void EndDialogue()
    {
        dialogueCanvas.SetActive(false);
        isDialogueActive = false;

        foreach (var image in characterImageSlots)
        {
            image.sprite = null;
            image.color = Color.white;
        }

        characterImageMap.Clear();

        // 대화 종료 시 플레이어 상태 PLAY로 전환
        PlayerStateManager.Instance?.SetState(PlayerState.Play);
    }

    public bool IsDialogueActive()
    {
        return isDialogueActive;
    }
}