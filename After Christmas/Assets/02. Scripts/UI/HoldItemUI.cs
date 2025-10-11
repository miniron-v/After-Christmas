using UnityEngine;
using UnityEngine.UI;

public class HoldItemUI : MonoBehaviour
{
    public static HoldItemUI Instance { get; private set; }
    [SerializeField] private GameObject holdItemObjParents; // HOLDITEMOBJPARENTS
    [SerializeField] private Image holdItemIcon;            // HOLDITEMICON (Image)

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    // 아이템을 잡았을 때 아이콘 갱신 & UI 켜기
    public void Show(Sprite icon)
    {
        if (holdItemIcon != null)
            holdItemIcon.sprite = icon;

        if (holdItemObjParents != null)
            holdItemObjParents.SetActive(true);
    }

    // 아이템을 내려놓았을 때 UI 끄기
    public void Hide()
    {
        Debug.Log("실행");
        if (holdItemObjParents != null)
            holdItemObjParents.SetActive(false);
    }
}
