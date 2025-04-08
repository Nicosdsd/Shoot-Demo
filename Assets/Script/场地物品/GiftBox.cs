using System;
using UnityEngine;

public class GiftBox : MonoBehaviour
{

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            ExpManager expManager = FindAnyObjectByType<ExpManager>();
            expManager.LevelUp();
        }
    }
}
