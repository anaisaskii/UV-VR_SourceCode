using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartGame : MonoBehaviour
{
    private SphereCollider sphereCollider;

    void Start()
    {
        // Get button collider
        sphereCollider = GetComponent<SphereCollider>();
    }

    //If the player hits the button, start the game
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            SceneManager.LoadScene("MRTScene");
        }
    }
}
