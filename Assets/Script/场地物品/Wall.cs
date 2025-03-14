using System;
using UnityEngine;

public class Wall : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        transform.parent = null;
        transform.position = FindAnyObjectByType<PlayerControl>().transform.position;
        transform.rotation = Quaternion.identity;
    }

}
