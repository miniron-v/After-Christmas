using UnityEngine;
using UnityEngine.EventSystems;

public class PostItAnchorButton : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerClickHandler
{
    [SerializeField] private PostItView postItView;
    [SerializeField] private PostItData data;

    private bool isPostItOpen = false;

    private void Awake()
    {
        if (data == null)
            data = new PostItData();

        postItView.Hide();
    }

    // 미니맵 모드 – Hover (Read Only)
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (MinimapManager.Instance.CurrentState == MinimapState.MinimapView)
        {
            postItView.Show(data, editable: false);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (MinimapManager.Instance.CurrentState == MinimapState.MinimapView)
        {
            postItView.Hide();
        }
    }

    // 포스트잇 모드 – Click
    public void OnPointerClick(PointerEventData eventData)
    {
        if (MinimapManager.Instance.CurrentState != MinimapState.PostItMode)
            return;

        // 좌클릭
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            if (!isPostItOpen)
            {
                postItView.Show(data, editable: true);
                isPostItOpen = true;
            }
            else
            {
                postItView.Hide();   // ⭐ 여기서 저장됨
                isPostItOpen = false;
            }
        }
        // 우클릭
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            Destroy(gameObject);
        }
    }
}
