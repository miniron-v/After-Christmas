using TMPro;
using UnityEngine;

public class PostItView : MonoBehaviour
{
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private CanvasGroup canvasGroup;

    private PostItData boundData;
    private bool isEditing;

    private void Awake()
    {
        Hide();
    }

    public void Show(PostItData data, bool editable)
    {
        boundData = data;
        isEditing = editable;

        inputField.SetTextWithoutNotify(data.text);
        inputField.interactable = editable;

        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = editable;
        canvasGroup.interactable = editable;

        PostItRegistry.Register(this);   // ⭐ 등록
    }

    public void Hide()
    {
        if (boundData != null && isEditing)
        {
            boundData.text = inputField.text;
        }

        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;

        isEditing = false;
        boundData = null;

        PostItRegistry.Unregister(this); // ⭐ 해제
    }

    private void OnDestroy()
    {
        PostItRegistry.Unregister(this);
    }

}
