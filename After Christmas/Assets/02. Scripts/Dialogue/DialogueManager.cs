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

    private Dictionary<CharacterData, Image> characterImageMap;

    private Color activeColor = new Color(1f, 1f, 1f, 1f);
    private Color inactiveColor = new Color(0.5f, 0.5f, 0.5f, 1f);

    public Action onDialogueEnd;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        dialogueCanvas.SetActive(false);
        dialogueQueue = new Queue<DialogueData.DialogueLine>();
        characterImageMap = new Dictionary<CharacterData, Image>();
    }

    void Update()
    {
        if (isDialogueActive && Input.GetKeyDown(KeyCode.Space))
        {
            DisplayNextSentence();
        }
    }

    // Dialogue 시작
    public void StartDialogue(DialogueData data, Action onEnd = null)
    {
        if (isDialogueActive) return;
        onDialogueEnd = onEnd;
        isDialogueActive = true;
        dialogueCanvas.SetActive(true);
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
    }

    private Image GetAvailableImageSlot()
    {
        return characterImageSlots.FirstOrDefault(slot => !characterImageMap.ContainsValue(slot));
    }

    // 다음 문장 보여주기
    public void DisplayNextSentence()
    {
        if (isTyping)
        {
            if (typingCoroutine != null)
            {
                StopCoroutine(typingCoroutine);
            }
            dialogueText.text = currentSentence;
            isTyping = false;
            return;
        }
        if (dialogueQueue.Count == 0)
        {
            EndDialogue();
            return;
        }

        DialogueData.DialogueLine line = dialogueQueue.Dequeue();
        currentSentence = line.sentence;

        // 대화 중인 캐릭터만 강조
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

        if (!playerData)
        {
            nameText.text = line.speaker.characterName;
        }
        typingCoroutine = StartCoroutine(TypeSentence(line.sentence));
    
    }

    // 타이핑 효과 코루틴
    IEnumerator TypeSentence(string sentence)
    {
        isTyping = true;
        dialogueText.text = "";
        foreach (char letter in sentence.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }
        isTyping = false;
        typingCoroutine = null;
    }

    // dialogue 종료
    void EndDialogue()
    {
        dialogueCanvas.SetActive(false);
        isDialogueActive = false;

        foreach (var image in characterImageSlots)
        {
            image.sprite = null;
            image.color = Color.white;
        }
        onDialogueEnd?.Invoke();
        characterImageMap.Clear();
    }

    public bool IsDialogueActive()
    {
        return isDialogueActive;
    }
}