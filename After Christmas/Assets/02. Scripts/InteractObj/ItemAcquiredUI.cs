using DG.Tweening;
using System.Collections;
using TMPro;
using UnityEngine;

public class ItemAcquiredUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private float duration;

    public void Awake()
    {
        text.gameObject.SetActive(false);
    }

    public IEnumerator NoticeItemAcquired(string itemName)
    {
        text.text = $"'{itemName} 획득'";

        text.color = new Color(1, 1, 1, 0);
        text.gameObject.SetActive(true);

        Sequence sequence = DOTween.Sequence();
        sequence.Append(text.DOFade(1f, duration)).SetEase(Ease.OutCubic)
            .Append(text.DOFade(0f, duration)).SetEase(Ease.InCubic);

        yield return sequence.WaitForCompletion();
    }
}
