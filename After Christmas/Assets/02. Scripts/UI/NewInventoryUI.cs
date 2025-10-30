using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

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

    private PlayerItemHandler handler;
    public int currentSceneIndex = -1;
    private bool isMapTab = true;

    #region Open / Close

    public void Open(PlayerItemHandler h)
    {
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

        // Map 탭 선택 + 버튼 색 변경
        OnClickMapTab();

        // 선택 버튼 상태 갱신
        UpdateSelectedButtonInteractable();
    }


    public void Close()
    {
        gameObject.SetActive(false);
        handler = null;
        UIModalGate.Release(this);
        PlayerStateManager.Instance?.SetState(PlayerState.Play);
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
            selectedImage.sprite = icon;
            selectedName.text = id;
            selectedDesc.text = description;

            selectedButton.onClick.RemoveAllListeners();
            selectedButton.interactable = false;

            if (canInteract && handler != null)
            {
                selectedButton.interactable = true;
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
                            handler.Teleport(spawn);
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
}
