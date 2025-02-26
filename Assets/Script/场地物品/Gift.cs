using System;
using UnityEngine;

public class Gift : MonoBehaviour
{
    public GameObject parachute;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
   

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.name == "Plane")
        {
            parachute.SetActive(false);
        }
    }
}
