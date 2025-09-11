using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

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
    public void StartDialogue(DialogueData data)
    {
        if (isDialogueActive) return;
        isDialogueActive = true;
        dialogueCanvas.SetActive(true);
        dialogueQueue.Clear();
        characterImageMap.Clear();

        // 이미지 할당하기
        foreach (var slot in characterImageSlots)
        {
            slot.sprite = null;
            slot.color = Color.clear;
            slot.transform.SetAsFirstSibling();
        }

        for (int i = 0; i < data.participants.Length && i < characterImageSlots.Count; i++)
        {
            CharacterData character = data.participants[i];
            if (character != null)
            {
                characterImageMap[character] = characterImageSlots[i];
                characterImageSlots[i].sprite = character.characterImage;
                characterImageSlots[i].color = inactiveColor;
                characterImageSlots[i].transform.SetAsFirstSibling();
            }
        }

        if (playerData != null && !characterImageMap.ContainsKey(playerData))
        {
            Image nextSlot = GetAvailableImageSlot();
            if (nextSlot != null)
            {
                characterImageMap[playerData] = nextSlot;
                nextSlot.sprite = playerData.characterImage;
                nextSlot.color = inactiveColor;
                nextSlot.transform.SetAsFirstSibling();
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

        if (line.speaker == null)
        {
            Debug.LogError("CharacterData is null on a dialogue line! Check your DialogueData asset.");
            EndDialogue();
            return;
        }

        // 대화 중인 캐릭터만 강조
        foreach (var kvp in characterImageMap)
        {
            kvp.Value.color = (kvp.Key == line.speaker) ? activeColor : inactiveColor;
        }

        if (characterImageMap.ContainsKey(line.speaker))
        {
            // 말하는 캐릭터의 이미지를 맨 위로 올립니다.
            characterImageMap[line.speaker].transform.SetAsLastSibling();
        }

        nameText.text = line.speaker.characterName;
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
        characterImageMap.Clear();
    }

    public bool IsDialogueActive()
    {
        return isDialogueActive;
    }
}