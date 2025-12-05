using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class NewInventoryUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Transform patientListRoot;      // PATIENTLIST
    [SerializeField] private Transform patientInfoRoot;      // PATIENTINFO
    [SerializeField] private Transform patientInventoryRoot; // PATIENTINVENTORY
    [SerializeField] private GameObject patientButtonPrefab;

    [Header("Patient Info UI")]
    [SerializeField] private Image patientIcon;
    [SerializeField] private TMP_Text patientName;

    [Header("Inventory UI")]
    [SerializeField] private Button mapTabButton;
    [SerializeField] private Button itemTabButton;
    [SerializeField] private Transform inventoryScrollList;   // Slot들을 담는 부모
    [SerializeField] private GameObject inventorySlotPrefab;
    [SerializeField] private Image selectedImage;
    [SerializeField] private TMP_Text selectedName;
    [SerializeField] private TMP_Text selectedDesc;
    [SerializeField] private Button selectedButton;

    [Header("Animation Settings")]
    [SerializeField] private RectTransform rootPanel;   // UI 전체 패널
    [SerializeField] private float slideDuration = 0.4f;
    [SerializeField] private Ease slideEase = Ease.OutBack;    // 위로 올라올 때 튕기는 듯한 느낌
    [SerializeField] private CanvasGroup tabletScreenCanvasGroup;
    [SerializeField] private RectTransform tabletScreenRect;
    [SerializeField] private float screenRevealDuration = 0.4f;
    [SerializeField] private Ease screenEase = Ease.OutCubic;

    private GameObject currentSelectedSlot = null;  // 현재 선택된 슬롯


    private bool isAnimating = false;


    private PlayerItemHandler handler;
    public int currentSceneIndex = -1;
    private bool isMapTab = true;

    #region Open / Close

    public void Open(PlayerItemHandler h)
    {
        if (isAnimating) return;

        isAnimating = true;
        UIModalGate.Acquire(this, Close);
        handler = h;
        gameObject.SetActive(true);

        PlayerStateManager.Instance?.SetState(PlayerState.UIOpen);
        BuildPatientList();

        string sceneName = SceneManager.GetActiveScene().name;
        currentSceneIndex = ItemInfoManager.Instance.GetSceneIndex(sceneName);

        if (currentSceneIndex >= 0)
            ShowPatientInfo(currentSceneIndex);
        else
            ShowPatientList();

        patientInventoryRoot.gameObject.SetActive(false);

        OnClickMapTab();
        UpdateSelectedButtonInteractable();

        PlayOpenAnimation();
    }



    public void Close()
    {
        if (isAnimating)
            return;

        isAnimating = true;

        // DOTween 시퀀스
        Sequence seq = DOTween.Sequence();

        // 1️⃣ 화면 먼저 접히며 사라짐 (중앙에서 양쪽으로)
        seq.Append(tabletScreenRect.DOScaleX(0f, screenRevealDuration * 0.8f).SetEase(screenEase));
        seq.Join(tabletScreenCanvasGroup.DOFade(0f, screenRevealDuration).SetEase(Ease.OutQuad));

        // 2️⃣ 화면이 완전히 닫히면 태블릿 본체 아래로 슬라이드
        seq.Append(rootPanel.DOAnchorPosY(-Screen.height, slideDuration).SetEase(slideEase));

        // 3️⃣ 완료 후 UI 비활성화 및 상태 초기화
        seq.OnComplete(() =>
        {
            gameObject.SetActive(false);
            tabletScreenRect.localScale = Vector3.one;           // 초기화
            tabletScreenCanvasGroup.alpha = 1f;                 // 초기화
            handler = null;
            UIModalGate.Release(this);
            PlayerStateManager.Instance?.SetState(PlayerState.Play);
            isAnimating = false;
        });
    }


    #endregion

    #region Patient List

    private void BuildPatientList()
    {
        if (patientListRoot == null || patientButtonPrefab == null)
        {
            Debug.LogError("PatientListRoot 또는 PatientButtonPrefab이 할당되지 않았습니다!");
            return;
        }

        // 기존 버튼 삭제
        foreach (Transform child in patientListRoot)
            Destroy(child.gameObject);

        int sceneCount = ItemInfoManager.Instance.GetSceneCount();

        for (int i = 0; i < sceneCount; i++)
        {
            if (!ItemInfoManager.Instance.HasVisitedScene(i))
                continue;

            GameObject btnObj = Instantiate(patientButtonPrefab, patientListRoot);
            Button btn = btnObj.GetComponent<Button>();
            TMP_Text txt = btnObj.GetComponentInChildren<TMP_Text>();

            if (txt != null)
                txt.text = $"환자 {i + 1}";

            int index = i;
            if (btn != null)
                btn.onClick.AddListener(() => ShowPatientInfo(index));
        }
    }

    #endregion

    #region Patient Info

    private void ShowPatientInfo(int sceneIndex)
    {
        currentSceneIndex = sceneIndex;

        patientListRoot.gameObject.SetActive(false);
        patientInfoRoot.gameObject.SetActive(true);
        patientInventoryRoot.gameObject.SetActive(false);

        patientName.text = $"환자 {sceneIndex + 1}";

        if (patientIcon != null)
        {
            patientIcon.GetComponent<Button>().onClick.RemoveAllListeners();
            patientIcon.GetComponent<Button>().onClick.AddListener(() => OpenInventory());
        }
    }

    private void ShowPatientList()
    {
        patientListRoot.gameObject.SetActive(true);
        patientInfoRoot.gameObject.SetActive(false);
        patientInventoryRoot.gameObject.SetActive(false);
    }

    #endregion

    #region Patient Inventory (Map / Item Tab)

    private void OpenInventory()
    {
        if (patientInventoryRoot == null) return;

        patientInventoryRoot.gameObject.SetActive(true);
        patientListRoot.gameObject.SetActive(false);
        patientInfoRoot.gameObject.SetActive(false);

        isMapTab = true;
        BuildInventory();

        //버튼 색 갱신
        UpdateTabButtonColors();
    }

    private void BuildInventory()
    {
        if (inventoryScrollList == null || inventorySlotPrefab == null)
            return;

        // 기존 슬롯 삭제
        foreach (Transform child in inventoryScrollList)
            Destroy(child.gameObject);

        int sceneIndex = currentSceneIndex;
        if (sceneIndex < 0) return;

        bool first = true;

        if (isMapTab)
        {
            var mapIDs = MapInfoManager.Instance.GetVisitedMapIDs(sceneIndex);

            foreach (var id in mapIDs)
            {
                var record = MapInfoManager.Instance.GetMapRecord(id, sceneIndex);
                if (record == null) continue;

                bool canInteract = sceneIndex == ItemInfoManager.Instance.GetSceneIndex(SceneManager.GetActiveScene().name);

                // 첫 슬롯이면 자동 선택
                bool autoSelect = first;

                AddInventorySlot(record.mapID, record.mapIcon, record.description, true, canInteract, autoSelect);

                first = false;
            }

            if (first) // Map이 하나도 없으면 NULL 처리
            {
                selectedImage.sprite = null;
                selectedName.text = "";
                selectedDesc.text = "";
            }
        }
        else
        {
            var itemIDs = ItemInfoManager.Instance.GetAllItemIDs(sceneIndex);

            foreach (var id in itemIDs)
            {
                var icon = ItemInfoManager.Instance.GetItemIcon(id);
                var desc = ItemInfoManager.Instance.GetItemDescription(id);

                bool canInteract = sceneIndex == ItemInfoManager.Instance.GetSceneIndex(SceneManager.GetActiveScene().name);

                // 첫 슬롯이면 자동 선택
                bool autoSelect = first;

                AddInventorySlot(id, icon, desc, false, canInteract, autoSelect);

                first = false;
            }

            if (first) // Item이 하나도 없으면 NULL 처리
            {
                selectedImage.sprite = null;
                selectedName.text = "";
                selectedDesc.text = "";
            }
        }

        // 선택 버튼 상태 갱신
        UpdateSelectedButtonInteractable();
    }

    // AddInventorySlot 수정 (autoSelect 플래그 추가)
    private void AddInventorySlot(string id, Sprite icon, string description, bool isMap, bool canInteract, bool autoSelect)
    {
        if (string.IsNullOrEmpty(id)) return;

        GameObject slotObj = Instantiate(inventorySlotPrefab, inventoryScrollList);
        Button btn = slotObj.GetComponent<Button>();
        Image img = slotObj.GetComponentInChildren<Image>();
        TMP_Text txt = slotObj.GetComponentInChildren<TMP_Text>();

        if (txt != null) txt.text = id;
        if (img != null) img.sprite = icon;

        void SelectSlot()
        {
            if (slotObj == null) return; // slotObj 안전 체크
            if (currentSelectedSlot != null)
            {
                Transform prevSelected = currentSelectedSlot.transform.Find("Selected");
                if (prevSelected != null)
                    prevSelected.gameObject.SetActive(false);
            }

            Transform selectedObj = slotObj.transform.Find("Selected");
            if (selectedObj != null)
                selectedObj.gameObject.SetActive(true);

            currentSelectedSlot = slotObj;

            if (selectedImage != null) selectedImage.sprite = icon;
            if (selectedName != null) selectedName.text = id;
            if (selectedDesc != null) selectedDesc.text = description;

            if (selectedButton != null)
            {
                selectedButton.onClick.RemoveAllListeners();
                selectedButton.interactable = canInteract && handler != null;

                if (canInteract && handler != null)
                {
                    selectedButton.onClick.AddListener(() =>
                    {
                        if (isMap)
                        {
                            MapRecord mapObj = MapInfoManager.Instance.GetMapRecord(id, currentSceneIndex);
                            if (mapObj != null)
                            {
                                SpawnTransform spawn = new SpawnTransform(
                                    mapObj.playerSpawnPoint,
                                    mapObj.cameraSpawnPoint,
                                    mapObj.mapID
                                );
                                handler.Teleport(spawn,id);
                            }
                        }
                        else
                        {
                            handler.HoldItem(id);
                        }

                        Close();
                    });
                }
            }
        }


        // 버튼 클릭 리스너
        if (btn != null)
            btn.onClick.AddListener(SelectSlot);

        // 첫 슬롯이면 자동 선택
        if (autoSelect)
            SelectSlot();
    }



    public void OnClickMapTab()
    {
        isMapTab = true;
        BuildInventory();
        UpdateTabButtonColors();
    }

    public void OnClickItemTab()
    {
        isMapTab = false;
        BuildInventory();
        UpdateTabButtonColors();
    }

    private void UpdateSelectedButtonInteractable()
    {
        if (selectedButton == null) return;

        // 현재 선택된 이름 기준으로 맵/아이템 판단
        string id = selectedName.text;
        if (string.IsNullOrEmpty(id))
        {
            selectedButton.interactable = false;
            return;
        }

        bool canInteract = false;

        if (isMapTab)
        {
            // Map일 경우 현재 씬과 선택한 환자 씬 비교
            canInteract = currentSceneIndex == ItemInfoManager.Instance.GetSceneIndex(SceneManager.GetActiveScene().name);
        }
        else
        {
            // Item일 경우도 동일하게 처리
            canInteract = currentSceneIndex == ItemInfoManager.Instance.GetSceneIndex(SceneManager.GetActiveScene().name);
        }

        selectedButton.interactable = canInteract && handler != null;
    }


    // Map / Item 버튼 색상 업데이트
    private void UpdateTabButtonColors()
    {
        Color selectedColor = new Color(0f, 0f, 0.5f);
        Color normalColor = Color.white;

        if (mapTabButton != null)
            mapTabButton.image.color = isMapTab ? selectedColor : normalColor;
        if (itemTabButton != null)
            itemTabButton.image.color = isMapTab ? normalColor : selectedColor;
    }

    #endregion

    #region 뒤로가기 관련 헬퍼

    public void CloseAllUI()
    {
        gameObject.SetActive(false);
    }

    public void ShowPatientListUI()
    {
        patientListRoot.gameObject.SetActive(true);
        patientInfoRoot.gameObject.SetActive(false);
        patientInventoryRoot.gameObject.SetActive(false);
    }

    public void ShowPatientInfoUI()
    {
        patientListRoot.gameObject.SetActive(false);
        patientInfoRoot.gameObject.SetActive(true);
        patientInventoryRoot.gameObject.SetActive(false);
    }

    #endregion

    #region 애니메이션 관련 함수

    private void PlayOpenAnimation()
    {
        Vector2 startPos = new Vector2(rootPanel.anchoredPosition.x, -Screen.height * 0.5f);
        rootPanel.anchoredPosition = startPos;
        tabletScreenCanvasGroup.alpha = 0f;

        Sequence seq = DOTween.Sequence();
        seq.Append(rootPanel.DOAnchorPosY(0f, slideDuration).SetEase(slideEase));
        seq.AppendCallback(() =>
        {
            PlayInnerReveal();
        });
    }



    private void PlayInnerReveal()
    {
        if (tabletScreenRect == null || tabletScreenCanvasGroup == null)
            return;

        // 초기 상태: 화면이 중앙에서 가늘게 존재 (X축 0)
        tabletScreenRect.localScale = new Vector3(0f, 1f, 1f);

        // 중앙에서 좌우로 확장되며 알파가 서서히 올라감
        Sequence seq = DOTween.Sequence();
        seq.Append(tabletScreenRect.DOScaleX(1f, screenRevealDuration * 0.8f)
            .SetEase(screenEase));

        seq.Join(tabletScreenCanvasGroup.DOFade(1f, screenRevealDuration)
            .SetEase(Ease.OutQuad));

        seq.OnComplete(() =>
        {
            // 완전히 켜진 상태 유지
            tabletScreenRect.localScale = Vector3.one;
            tabletScreenCanvasGroup.alpha = 1f;
            isAnimating = false;
        });
    }

    #endregion
}
