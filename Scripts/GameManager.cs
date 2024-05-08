using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
// using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.SceneManagement;


public class GameManager : MonoBehaviour
{
    public int lapAmount;
    public GameObject[] checkpoints;
    public GameObject[] players;
    [HideInInspector] public List<string> playersFinished;
    [HideInInspector] public float[] playerTimes;
    [HideInInspector] public string[] playerTimesStr;
    int[] playerLaps;
    public TextMeshProUGUI lapCounter;
    public TextMeshProUGUI timeCounter;
    public TextMeshProUGUI startTime;
    public TextMeshProUGUI DNFTime;
    public TextMeshProUGUI speedometer;
    public GameObject nextCarbtn;
    float DNFTimer = 0;
    private bool hasCountedDown = false;

    private Rigidbody carRB;
    void Start()
    {
        AudioSource backgroundMusic = GameObject.Find("Car Data").GetComponent<AudioSource>();
        backgroundMusic.Stop();
        
        // reset laps
        playerLaps = new int[players.Count()];
        playerTimes = new float[players.Count()];
        playerTimesStr = new string[players.Count()];

        for (int i = 0; i < playerLaps.Count(); i++)
        {
            playerLaps[i] = 1;
        }

        for (int i = 0; i < players.Count(); i++)
        {
            playerTimes[i] = 0.00000000000f;
            playerTimesStr[i] = "1000000";
        }

        DontDestroyOnLoad(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        if (!hasCountedDown)
        {
            StartGame();
            
        }

        


        for (int i = 0; i < players.Count(); i++)
        {
            GameObject player = players[i];

            try
            {
                if (playersFinished.Contains(player.name) || playersFinished.Contains(player.name + " (player)"))
                    continue;
            }
            catch
            {
                return;
            }
            if (hasCountedDown)
            {
            playerTimes[i] += Time.deltaTime;            
            }
            bool isAI = true;
            if (player.GetComponent<PlayerController>().enabled)
                isAI = false;

            if (isAI)
            {
                bool isFinished = player.GetComponent<AgentController>().isFinished;

                if (isFinished)
                {
                    player.GetComponent<AgentController>().isFinished = false;
                    playerLaps[i] += 1;
                }
            }
            else
            {
                int checkpointsCollected = player.GetComponent<PlayerController>().checkpointsCollected;
                carRB = player.GetComponent<Rigidbody>();
                var carVel = Mathf.Round((Mathf.Abs(carRB.velocity.x) + Mathf.Abs(carRB.velocity.y) + Mathf.Abs(carRB.velocity.z))*6);
                speedometer.SetText(carVel.ToString());

                if (checkpointsCollected == checkpoints.Count())
                {
                    player.GetComponent<PlayerController>().checkpointsCollected = 0;

                    playerLaps[i] += 1;

                    if (playerLaps[i] <= lapAmount)
                        lapCounter.text = "Lap count: " + playerLaps[i] + "/" + lapAmount;
                }

                timeCounter.text = "Time: " + (Mathf.Round(playerTimes[i]*1000)/1000).ToString();
            }

            if (playerLaps[i] > lapAmount)
            {
                if (isAI)
                {
                    playersFinished.Add(player.name);
                    player.GetComponent<AgentController>().enabled = false;
                }
                else
                {
                    playersFinished.Add(player.name + " (player)");
                    player.GetComponent<PlayerController>().enabled = false;
                    nextCarbtn.SetActive(true);
                }
                playerTimesStr[i] = playerTimes[i].ToString();
            }
        }



        //DNF
        if (playersFinished.Count() > 0)
        {
            DNFTimer += Time.deltaTime;
            DNFTime.text = "DNF timer: "+(Mathf.Round(60-DNFTimer)).ToString();
        }

        // race finished
        if (playersFinished.Count() == players.Count() || DNFTimer > 60)
        {
            // sort array
            float[] fTimes = new float[players.Count()];
            
            for (int i = 0; i < players.Count(); i++)
            {
                fTimes[i] = float.Parse(playerTimesStr[i]);
            }

            Array.Sort(fTimes);

            for (int i = 0; i < players.Count(); i++)
            {
                playerTimesStr[i] = fTimes[i].ToString();

                try
                {
                    playerTimesStr[i] = playerTimesStr[i].Substring(0, 7);
                }
                catch
                {

                }
            }
            for (var i = 0; i < players.Count();i++)
            {
                
                if (!playersFinished.Contains(players[i].name) && !playersFinished.Contains(players[i].name + " (player)")){
                    
                    playersFinished.Add(players[i].name);
                }
            }
            
            SceneManager.LoadScene("WinScreen");
        }

        

    }


    
    private float sTime = 5;
    private void StartGame(){
        sTime -= Time.deltaTime;
        startTime.text = (Mathf.Round(sTime*10)/10).ToString();
        if (sTime/60 <= 5 && sTime/60 >= 0)
        {
            
            for (var i = 0; i < players.Count(); i++)
            {
                players[i].GetComponent<Rigidbody>().isKinematic = true;
            }
        } else if (sTime/60 < 0) {
            
            for (var i = 0; i < players.Count(); i++)
            {
                players[i].GetComponent<Rigidbody>().isKinematic = false;
            }
            startTime.text = "";
            hasCountedDown=true;
        }
        
        
    }
}
