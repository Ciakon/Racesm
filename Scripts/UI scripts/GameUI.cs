using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using JetBrains.Annotations;
using Unity.Mathematics;

public class GameUI : MonoBehaviour
{
    
    public GameObject car;
    private float carVel;
    public Rigidbody carRB;

    public TextMeshPro speedometer;
    // Start is called before the first frame update
    void Start()
    {
        if (speedometer == null)
        {
            speedometer = new TextMeshPro();
        }
        if (carRB == null)
        {
            carRB = GetComponent<Rigidbody>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        carVel = (math.abs(carRB.velocity.x) + math.abs(carRB.velocity.y) + math.abs(carRB.velocity.z))*5;
        //print(carVel);
        speedometer.SetText(carVel.ToString());
    }
}
