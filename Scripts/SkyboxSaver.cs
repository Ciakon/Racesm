using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkyboxSaver : MonoBehaviour
{
    public string skyboxMaterial;
    void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }
}
