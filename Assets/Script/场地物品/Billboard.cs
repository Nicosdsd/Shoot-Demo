using UnityEngine;

public class Billboard : MonoBehaviour {
    Transform m_Camera;

    void Start() {
// 获取场景里的主摄像机
        m_Camera = Camera.main.transform;
    }

    void LateUpdate() {
        if (m_Camera == null) {
            return;
        }
// 使UI元素朝向摄像机
        transform.rotation = Quaternion.LookRotation(transform.position - m_Camera.position);
    }
}