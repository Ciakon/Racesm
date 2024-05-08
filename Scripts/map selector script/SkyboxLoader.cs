using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkyboxLoader : MonoBehaviour
{
    public Material skyboxDay;
    public Material skyboxNight;
    string skyboxToLoad;
    void Awake()
    {
        skyboxToLoad = GameObject.Find("SkyboxManager").GetComponent<TimeOfDay>().skyboxMaterial;
        if (skyboxToLoad == "Day")
        {
            RenderSettings.skybox = skyboxDay;
        }
        else
        {
            RenderSettings.skybox = skyboxNight;
        }
    }
}