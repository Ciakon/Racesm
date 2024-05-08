using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Unity.Burst.Intrinsics;
using UnityEngine;

public class CarLoader : MonoBehaviour
{
    public GameObject[] Cars;
    string carType;
    string carColor;
    public Material[] Colors;
    public GameObject cam;

    public GameObject targetcar;
    void Start()
    {
        carType = GameObject.Find("Car Data").GetComponent<CarData>().carType;
        carColor = GameObject.Find("Car Data").GetComponent<CarData>().carColor;

        foreach (GameObject car in Cars)
        {
            if (car.name == carType)
            {
                replaceCar(car, carColor);
            }
        }
    }

    void replaceCar(GameObject car, string color)
    {
        car.GetComponent<PlayerController>().enabled = true;
        car.GetComponent<AgentController>().enabled = false;

        cam.GetComponent<CameraControl>().getCar(car);
        //LookAt = car.transform;

        if (color == "Default")
        {
            return;
        }

        if (color == "Black")
        {
            car.GetComponent<ChangeCarColor>().ChangeColor(Colors[0]);
        }
        if (color == "Blue")
        {
            car.GetComponent<ChangeCarColor>().ChangeColor(Colors[1]);
        }
        if (color == "Brown")
        {
            car.GetComponent<ChangeCarColor>().ChangeColor(Colors[2]);
        }
        if (color == "Gray")
        {
            car.GetComponent<ChangeCarColor>().ChangeColor(Colors[3]);
        }
        if (color == "Green")
        {
            car.GetComponent<ChangeCarColor>().ChangeColor(Colors[4]);
        }
        if (color == "Orange")
        {
            car.GetComponent<ChangeCarColor>().ChangeColor(Colors[5]);
        }
        if (color == "Pink")
        {
            car.GetComponent<ChangeCarColor>().ChangeColor(Colors[6]);
        }
        if (color == "Red")
        {
            car.GetComponent<ChangeCarColor>().ChangeColor(Colors[7]);
        }
        if (color == "Turquoise")
        {
            car.GetComponent<ChangeCarColor>().ChangeColor(Colors[8]);
        }
        if (color == "Violet")
        {
            car.GetComponent<ChangeCarColor>().ChangeColor(Colors[9]);
        }
        if (color == "White")
        {
            car.GetComponent<ChangeCarColor>().ChangeColor(Colors[10]);
        }
        if (color == "Yellow")
        {
            car.GetComponent<ChangeCarColor>().ChangeColor(Colors[11]);
        }

        
    }
}
