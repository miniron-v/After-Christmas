using UnityEngine;
using UnityEngine.UI;

public class FixedImageRatio : MonoBehaviour
{
    private Image uiImage;
    private void Awake()
    {
        uiImage = GetComponent<Image>();
    }
    private void LateUpdate()
    {
        if (uiImage.sprite != null)
        {
            // 스프라이트 가로세로 비율 계산
            float spriteWidth = uiImage.sprite.rect.width;
            float spriteHeight = uiImage.sprite.rect.height;
            float aspectRatio = spriteWidth / spriteHeight;

            // 가로 기준으로 높이 변경 ( 비율 고정 )
            Vector2 newSize = new Vector2(uiImage.rectTransform.sizeDelta.x, uiImage.rectTransform.sizeDelta.x / aspectRatio);
            uiImage.rectTransform.sizeDelta = newSize;
        }
    }
}
