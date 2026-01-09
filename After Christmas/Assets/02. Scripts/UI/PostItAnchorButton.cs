using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.PlayerLoop;

public class PostItAnchorButton : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerClickHandler
{
    [SerializeField] private PostItView postItView;
    [SerializeField] private PostItData data;
    [SerializeField] private GameObject destroyObj;
    private PostitPanel postitPanel;
    private Canvas canvas;

    private bool isPostItOpen = false;

    private void Awake()
    {
        InSideInit();
    }
    private void InSideInit()
    {
        if (data == null)
            data = new PostItData();
        canvas = postItView.GetComponent<Canvas>();
        canvas.overrideSorting = true;
        canvas.sortingOrder = 5;
        postItView.Hide();
        PostItRegistry.RegisterButton(gameObject);
    }
    public void OutSideInit(PostitPanel panel)
    {
        postitPanel = panel;
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
        if (MinimapManager.Instance.CurrentState != MinimapState.PostItMode) return;

        if (eventData.button == PointerEventData.InputButton.Left)
        {
            if (isPostItOpen) 
            {
                ClosePostIt();
            }
            else 
            {
                if (postitPanel != null && postitPanel.IsActive)
                {
                    postitPanel.TriggerBackdropClick();
                }
                
                OpenPostIt();
            }
        }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            ClosePostIt();
            Destroy(destroyObj);
        }
    }


    private void OpenPostIt()
    {
        if (isPostItOpen) return;
        postItView.Show(data, editable: true);
        
        isPostItOpen = true;

        // 주입받은 패널을 활성화하고 본인의 닫기 함수를 넘김
        if (postitPanel != null)
        {
            postitPanel.Show(() => ClosePostIt());
        }
    }

    public void ClosePostIt()
    {
        if (!isPostItOpen) return;
        postItView.Hide();
        isPostItOpen = false;

        if (postitPanel != null) postitPanel.Hide();
    }

    private void OnDestroy()
    {
        PostItRegistry.UnregisterButton(gameObject);
    }
}
