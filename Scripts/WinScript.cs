using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.VisualScripting;
using System.Linq;
public class WinScript : MonoBehaviour
{
    public TextMeshProUGUI MainText;
    public TextMeshProUGUI MainText2;
    public List<string> leaderboard;
    public string[] time;
    public GameObject[] Garages;
    
    private GameObject currentGarage;

    void Start()
    {
        foreach (GameObject carGarage in Garages)
        {
            carGarage.SetActive(false);
        }
        leaderboard = GameObject.Find("GameManager").GetComponent<GameManager>().playersFinished;
        time = GameObject.Find("GameManager").GetComponent<GameManager>().playerTimesStr;
        if (leaderboard[0].Contains("player")){
            
        }
        if (leaderboard[0].Contains("Racecar"))
        {
            currentGarage = Garages[6];
            Garages[6].SetActive(true);
        }
        else if (leaderboard[0].Contains("Hotrod"))
        {
            currentGarage = Garages[2];
            Garages[2].SetActive(true);
        }
        else if (leaderboard[0].Contains("Icecream"))
        {
            currentGarage = Garages[5];
            Garages[5].SetActive(true);
        }
        else if (leaderboard[0].Contains("Musclecar"))
        {
            currentGarage = Garages[1];
            Garages[1].SetActive(true);
        }
        else if (leaderboard[0].Contains("Pickup"))
        {
            currentGarage = Garages[3];
            Garages[3].SetActive(true);
        }
        else if (leaderboard[0].Contains("SchoolBus"))
        {
            currentGarage = Garages[7];
            Garages[7].SetActive(true);
        }
        else if (leaderboard[0].Contains("Sportscar"))
        {
            currentGarage = Garages[4];
            Garages[4].SetActive(true);
        }
        else if (leaderboard[0].Contains("Streetracist"))
        {
            currentGarage = Garages[0];
            Garages[0].SetActive(true);
        }
        
        
        
    }


    private void FixedUpdate() {
        MainText.text = "Leaderboard: ";
        MainText2.text = "Time: ";
        for (var i = 0; i < leaderboard.Count; i++){
            MainText.text += "\n" + (i+1) + "." + leaderboard[i];
            if (time[i] == "1000000"){
                MainText2.text += "\n(DNF)";  
                continue;   
            }
            MainText2.text += "\n(" + time[i] + ")"; 
        }
        currentGarage.transform.eulerAngles += Vector3.up * 0.7f;
        //currentRotation = CurrentCar.transform.rotation;
    }



}

