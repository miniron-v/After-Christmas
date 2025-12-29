using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class ManualUI : MonoBehaviour
{
    [Header("Root UI")]
    [SerializeField] private GameObject manualUI;

    [Header("Timing")]
    [SerializeField] private float minimumActiveTime = 2f;

    [Header("Key Icons UI")]
    [SerializeField] private Transform keyIconRoot;
    [SerializeField] private GameObject keyIconPrefab;

    // runtime state
    private List<ManualKeyVisual> targetKeys;
    private Dictionary<KeyCode, bool> keyPressedMap;
    private Dictionary<KeyCode, Image> keyImageMap;

    private bool actionCompleted;
    private float startTime;
    private Coroutine routine;

    void Start()
    {
        manualUI.SetActive(false);
    }

    void Update()
    {
        if (!manualUI.activeSelf || actionCompleted || targetKeys == null)
            return;

        foreach (var kv in targetKeys)
        {
            // 이미 눌린 키는 스킵
            if (keyPressedMap[kv.key])
                continue;

            if (Input.GetKeyDown(kv.key))
            {
                keyPressedMap[kv.key] = true;

                // sprite 활성화
                if (keyImageMap.TryGetValue(kv.key, out Image img))
                {
                    img.sprite = kv.activeSprite;
                }

                Debug.Log($"[{kv.key}] 입력 → 활성화");

                CheckAllKeysPressed();
                break;
            }
        }
    }

    // 조작법 알림 시작
    public void ShowManual(ManualActionSO action)
    {
        if (action == null)
        {
            Debug.LogWarning("[ManualUI] action is null");
            return;
        }
        if (routine != null)
            StopCoroutine(routine);

        targetKeys = action.keyVisuals;
        keyPressedMap = new Dictionary<KeyCode, bool>();
        keyImageMap = new Dictionary<KeyCode, Image>();

        actionCompleted = false;
        startTime = Time.realtimeSinceStartup;

        SetupUI(action);
        manualUI.SetActive(true);

        Debug.Log($"[ManualUI] 조작법 알림 표시 ({action.actionName})");

        routine = StartCoroutine(WaitAndHide());
    }

    // 모든 조작키를 눌렀는지 확인
    private void CheckAllKeysPressed()
    {
        foreach (bool pressed in keyPressedMap.Values)
        {
            if (!pressed)
                return;
        }

        Debug.Log("[ManualUI] 모든 키 입력 완료");
        actionCompleted = true;
    }

    // 조작법 알림 종료 대기 (키입력 + 3초 대기)
    private IEnumerator WaitAndHide()
    {
        yield return new WaitUntil(() => actionCompleted);

        float elapsed = Time.realtimeSinceStartup - startTime;
        if (elapsed < minimumActiveTime)
        {
            yield return new WaitForSecondsRealtime(minimumActiveTime - elapsed);
        }

        yield return new WaitForSecondsRealtime(1f);

        manualUI.SetActive(false);
        Debug.Log("[ManualUI] 조작법 알림 종료");

        routine = null;
    }

    // UI 아이콘
    private void SetupUI(ManualActionSO action)
    {
        // 초기화
        foreach (Transform child in keyIconRoot)
            Destroy(child.gameObject);

        // 세팅
        foreach (var kv in action.keyVisuals)
        {
            GameObject icon = Instantiate(keyIconPrefab, keyIconRoot);
            Image img = icon.GetComponent<Image>();

            img.sprite = kv.normalSprite;

            keyPressedMap[kv.key] = false;
            keyImageMap[kv.key] = img;
        }

        Debug.Log($"{action.actionName} UI 세팅 완료");
    }
}
