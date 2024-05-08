using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Mathematics;
using UnityEngine.UI;
using TMPro;

public class MapSelectorScript : MonoBehaviour
{
    public GameObject[] Maps;
    quaternion currentRotation;
    public GameObject CameraRotator;
    public string scene = "ForestRacetrack";
    public TextMeshProUGUI TellMap;

    void Start()
    {
        DisableAllMaps();
        ChangeToTrack2();
    }
    private void FixedUpdate() 
    {
        CameraRotator.transform.eulerAngles += Vector3.up * 0.7f;
        TellMap.text = "Selected map: \n" + scene;
    }

    void DisableAllMaps()
    {
        foreach (GameObject map in Maps)
        {
            map.SetActive(false);
        }
    }

    void SelectMap(int index)
    {
        DisableAllMaps();
        Maps[index].SetActive(true);
    }

    public void ChangeToTrack2()
    {
        scene = "ForestRacetrack";
        SelectMap(0);
    }

    public void ChangeToTrack3()
    {
        scene = "CityRacetrack";
        SelectMap(1);
    }

    public void SelectMapAndLoadScene()
    {
        SceneManager.LoadScene(scene);
    }
}
