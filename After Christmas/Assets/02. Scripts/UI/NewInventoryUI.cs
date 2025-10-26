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
    private int currentSceneIndex = -1;
    private bool isMapTab = true;

    #region Open / Close

    public void Open(PlayerItemHandler h)
    {
        handler = h;
        gameObject.SetActive(true);

        BuildPatientList();

        string sceneName = SceneManager.GetActiveScene().name;
        currentSceneIndex = ItemInfoManager.Instance.GetSceneIndex(sceneName);

        if (currentSceneIndex >= 0)
            ShowPatientInfo(currentSceneIndex);
        else
            ShowPatientList();

        patientInventoryRoot.gameObject.SetActive(false);

        // 기본적으로 Map 탭 선택 + 버튼 색 변경
        OnClickMapTab();
    }

    public void Close()
    {
        gameObject.SetActive(false);
        handler = null;
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

        foreach (Transform child in inventoryScrollList)
            Destroy(child.gameObject);

        if (isMapTab)
        {
            bool firstSet = false; // 첫 번째 방문 맵 선택 표시

            foreach (var mapName in MapVisitManager.Instance.GetVisitedMaps())
            {
                Map mapObj = MapVisitManager.Instance.GetMapObject(mapName);
                if (mapObj == null) continue;

                MapDataSO mapSO = mapObj.mapDataSO;
                if (mapSO == null) continue;

                AddInventorySlot(mapSO.mapID, mapSO.mapIcon, mapSO.description, true);

                //첫 번째 슬롯 선택 표시
                if (!firstSet)
                {
                    selectedImage.sprite = mapSO.mapIcon;
                    selectedName.text = mapSO.mapID;
                    selectedDesc.text = mapSO.description;
                    firstSet = true;
                }
            }
        }
        else
        {
            int sceneIndex = currentSceneIndex;
            if (sceneIndex < 0) return;

            foreach (var id in handler.GetAllItemIDs(sceneIndex))
            {
                if (!handler.isHavingItem(id)) continue;
                var icon = ItemInfoManager.Instance.GetItemIcon(id);
                var desc = ItemInfoManager.Instance.GetItemDescription(id);
                AddInventorySlot(id, icon, desc, false);
            }
        }
    }

    private void AddInventorySlot(string id, Sprite icon, string description, bool isMap)
    {
        if (string.IsNullOrEmpty(id)) return;

        GameObject slotObj = Instantiate(inventorySlotPrefab, inventoryScrollList);
        Button btn = slotObj.GetComponent<Button>();
        Image img = slotObj.GetComponentInChildren<Image>();
        TMP_Text txt = slotObj.GetComponentInChildren<TMP_Text>();

        if (txt != null) txt.text = id;
        if (img != null) img.sprite = icon;

        if (btn != null)
        {
            btn.onClick.AddListener(() =>
            {
                selectedImage.sprite = icon;
                selectedName.text = id;
                selectedDesc.text = description;

                selectedButton.onClick.RemoveAllListeners();
                selectedButton.onClick.AddListener(() =>
                {
                    if (isMap)
                    {
                        Map mapObj = MapVisitManager.Instance.GetMapObject(id);
                        if (mapObj != null && handler != null)
                        {
                            SpawnTransform spawn = new SpawnTransform(
                                mapObj.playerSpawnPoint.position,
                                mapObj.cameraSpawnPoint.position,
                                mapObj.mapName
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
            });
        }
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
