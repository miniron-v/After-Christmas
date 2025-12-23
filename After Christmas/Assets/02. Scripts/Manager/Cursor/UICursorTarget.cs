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

        CursorShapeManager.Instance.RequestCursor(this, type);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        CursorShapeManager.Instance.ReleaseCursor(this);
    }

    private void OnDisable()
    {
        CursorShapeManager.Instance.ReleaseCursor(this);
    }
}
