using System;
using UnityEngine;

public class GiftBox : MonoBehaviour
{

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            BuffManager buffManager = FindAnyObjectByType<BuffManager>();
            if (buffManager != null) // 空值检查
            {
                buffManager.gameObject.SetActive(true);
            }
        }
    }
}
