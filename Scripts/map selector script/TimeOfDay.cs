using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TimeOfDay : MonoBehaviour
{
    public GameObject DayLight;
    public GameObject NightLight;
    public string skyboxMaterial = "Night";
    public Material DaySkybox;
    public Material NightSkybox;
    public GameObject[] Lights;

    public static TimeOfDay Instance;
    public TextMeshProUGUI TellTime;
       void DisableAllLights()
    {
        foreach (GameObject light in Lights)
        {
            light.SetActive(false);
        }
    }

    public void ChangeToDayTime()
    {
        NightLight.SetActive(false);
        RenderSettings.skybox = DaySkybox;
        DayLight.SetActive(true);
        skyboxMaterial = "Day";
        TellTime.text = "Selected time of day: \n" + skyboxMaterial;
    }
    public void ChangeToNightTime()
    {
        DayLight.SetActive(false);
        RenderSettings.skybox = NightSkybox;
        NightLight.SetActive(true);
        skyboxMaterial = "Night";
        TellTime.text = "Selected time of day: \n" + skyboxMaterial;
    }
    private void Awake() 
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);
        DisableAllLights();
        ChangeToNightTime();
    }
}
