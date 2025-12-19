using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UICursorTarget : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler
{
    [SerializeField] private CursorType type;
    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (button != null && !button.interactable)
            return;

        CursorManager.Instance.RequestCursor(this, type);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        CursorManager.Instance.ReleaseCursor(this);
    }

    private void OnDisable()
    {
        CursorManager.Instance.ReleaseCursor(this);
    }
}
