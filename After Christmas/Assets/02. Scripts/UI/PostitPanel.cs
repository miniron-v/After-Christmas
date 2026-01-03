using UnityEngine;
using UnityEngine.EventSystems;
using System;

public class PostitPanel : MonoBehaviour, IPointerClickHandler
{
    private Action onBackdropClick;

    public bool IsActive => gameObject.activeSelf;

    public void Show(Action onClick)
    {
        onBackdropClick = onClick;
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        onBackdropClick = null;
        gameObject.SetActive(false);
    }

    public void TriggerBackdropClick()
    {
        if (!IsActive) return;

        onBackdropClick?.Invoke();
        Hide();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        TriggerBackdropClick();
    }

    private void Update()
    {
        if (IsActive && Input.GetKeyDown(KeyCode.Escape))
        {
            TriggerBackdropClick();
        }
    }
}