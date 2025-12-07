using Unity.VisualScripting;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StageManager : MonoBehaviour
{
    // 스테이지 클리어 시 진행할 대화
    private DialogueTrigger dialogueTrigger;
    [SerializeField] private int requiredInteractionCount;
    private int currentInteractionCount = 0;

    private void Awake()
    {
        dialogueTrigger = GetComponent<DialogueTrigger>();
    }

    private void OnEnable()
    {
        SpecialInteractObj.interactWithItem += IncreaseCount;
    }

    private void OnDisable()
    {
        SpecialInteractObj.interactWithItem -= IncreaseCount;
    }

    private void IncreaseCount()
    {
        currentInteractionCount++;
        if (isClearable())
        {
            StageClearCinematicSequence();
        }
    }

    private bool isClearable()
    {
        return requiredInteractionCount == currentInteractionCount;
    }

    // 스테이지가 클리어된다면 시네마틱 진행(대화 혹은 시네마틱)
    private void StageClearCinematicSequence()
    {
        if (TryGetComponent(out CinematicController cinematic))
        {
            cinematic.StartCutscene(() =>
            {
                if (dialogueTrigger != null)
                    dialogueTrigger.StartDialogueSequence(StageClear);
                else
                    StageClear();
            });
        }
        else
        {
            if (dialogueTrigger != null)
                dialogueTrigger.StartDialogueSequence(StageClear);
            else
                StageClear();
        }
    }


    private void StageClear()
    {
        // 진짜 스테이지 클리어 로직 넣기
    }

    void Update()
    {
        // 임시 씬이동 테스트(O,P == 1,2번 씬)
        if (Input.GetKeyDown(KeyCode.O))
        {
            SceneManager.LoadScene("itemHandlerScene");
        }
        if (Input.GetKeyDown(KeyCode.P))
        {
            SceneManager.LoadScene("itemHandlerScene2");
        }
    }
}
