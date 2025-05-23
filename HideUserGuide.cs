using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HideUserGuide : MonoBehaviour
{
    private ShapeEdgeRaycast shapeedgeraycast;

    private void Start()
    {
        GameObject shapespawner = GameObject.Find("Shapes");
        shapeedgeraycast = shapespawner.GetComponent<ShapeEdgeRaycast>();
    }

    // If the player hits the button show the user guides
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            shapeedgeraycast.HideUserGuide = false;
        }
    }
}
