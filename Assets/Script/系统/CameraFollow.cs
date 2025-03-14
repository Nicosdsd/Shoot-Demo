using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("目标对象")]
    public Transform target;

    [Header("摄像机参数")]
    public float followSpeed = 5f;
    public Vector3 followOffset = new Vector3(0, 5, -5);

    [Header("摄像机旋转")]
    public Transform rotationTarget; // 可选的旋转参考目标
    public float rotationSpeed = 5f;
    public float lookAtHeight = 2f; // 摄像机在Y轴的偏移

    void LateUpdate()
    {
        if (target == null)
        {
            Debug.LogWarning("摄像机没有目标！");
            return;
        }

        // 摄像机跟随位置
        Vector3 targetPosition = target.position + followOffset;
        transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);

        // 摄像机旋转
        Vector3 lookAtPoint = rotationTarget != null ? rotationTarget.position : target.position;
        lookAtPoint.y += lookAtHeight; // 稍微抬高视角

        Quaternion targetRotation = Quaternion.LookRotation(lookAtPoint - transform.position, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }
}