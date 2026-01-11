using UnityEngine;

public class StageManager : MonoBehaviour
{
    [Header("Clear Settings")]
    [SerializeField] private int requiredInteractionCount;
    [SerializeField] private string nextSceneName; // 다음으로 이동할 씬 이름(기획상 현실세계)
    public TransitionSettings clearTransitionSettings; // 스테이지 전용 전환 설정, 인스펙터에서 설정 가능

    // 스테이지 클리어 시 진행할 대화
    private DialogueTrigger dialogueTrigger;
    
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
        // 타겟 씬으로 이동
        TempLoadingManager.Instance.TransitionTo(nextSceneName, clearTransitionSettings);
    }

    [SerializeField] private KeyCode clearKey = KeyCode.Tab;

    private void Update()
    {
        if(Input.GetKeyDown(clearKey))
        {
            Debug.Log("클리어 디버그 작동");
            IncreaseCount();
        }
    }
}
