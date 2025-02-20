using UnityEngine;
using UnityEngine.EventSystems;
public class JoystickZoneEvent : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public RectTransform rectTransform;
    private Vector2 originalPosition;


    void Start()
    {
        // 保存初始位置
        originalPosition = rectTransform.anchoredPosition;
    }

    // 按下时触发
    public void OnPointerDown(PointerEventData eventData)
    {
        // 将 UI 元素的位置更新为鼠标按下的位置
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform.parent as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out localPoint
        );
        rectTransform.anchoredPosition = localPoint;
    }

    // 松开时触发
    public void OnPointerUp(PointerEventData eventData)
    {
        // 还原 UI 元素为初始位置
        rectTransform.anchoredPosition = originalPosition;
    }

}

