using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    public bool isCollected = false;
    public bool isVisible = true;
    MeshRenderer meshRenderer;
    

    private void Start() {
        meshRenderer = GetComponent<MeshRenderer>();
    }

    private void Update() {

        if (!isVisible)
        {
            meshRenderer.enabled = false;
            return;
        }

        if (isCollected)
        {
            meshRenderer.enabled = false;
        }
        else
        {
            meshRenderer.enabled = true;
        }
    }
}