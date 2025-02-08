using UnityEngine;
using UnityEngine.EventSystems;
//右摇杆触发事件
public class ShootEvent : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    private PlayerControl player;
    private void Awake()
    {
        player = FindFirstObjectByType<PlayerControl>();
    }


    public void OnPointerDown(PointerEventData eventData)
    {
        player.canFire = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        player.canFire = false;
    }
}
