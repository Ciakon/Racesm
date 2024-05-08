using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Garage : MonoBehaviour
{
    GameObject CurrentCar;
    string currentColor = "Default";
    public Button[] CarButtons;
    public GameObject[] Garages;
    public GameObject[] CarBodies;
    public Button[] ColorButtons;
    public Material[] Colors;
    quaternion currentRotation;
    public Button startButton;
    public GameObject carData;
    void Start()
    {
        CarButtons[0].onClick.AddListener(SelectHotrod);
        CarButtons[1].onClick.AddListener(SelectCoupe);
        CarButtons[2].onClick.AddListener(SelectBolide);
        CarButtons[3].onClick.AddListener(SelectIcecreamTruck);
        CarButtons[4].onClick.AddListener(SelectSportscar);
        CarButtons[5].onClick.AddListener(SelectSchoolBus);
        CarButtons[6].onClick.AddListener(SelectBulldozer);
        CarButtons[7].onClick.AddListener(SelectPicupTruck);

        ColorButtons[0].onClick.AddListener(SelectBlack);
        ColorButtons[1].onClick.AddListener(SelectBlue);
        ColorButtons[2].onClick.AddListener(SelectBrown);
        ColorButtons[3].onClick.AddListener(SelectGray);
        ColorButtons[4].onClick.AddListener(SelectGreen);
        ColorButtons[5].onClick.AddListener(SelectOrange);
        ColorButtons[6].onClick.AddListener(SelectPink);
        ColorButtons[7].onClick.AddListener(SelectRed);
        ColorButtons[8].onClick.AddListener(SelectTurquoise);
        ColorButtons[9].onClick.AddListener(SelectViolet);
        ColorButtons[10].onClick.AddListener(SelectWhite);
        ColorButtons[11].onClick.AddListener(SelectYellow);

        startButton.onClick.AddListener(startGame);

        disable();
        SelectHotrod();
    }

    private void FixedUpdate() {
        CurrentCar.transform.eulerAngles += Vector3.up * 0.7f;
        currentRotation = CurrentCar.transform.rotation;
    }

    void disable()
    {
        foreach (GameObject carGarage in Garages)
        {
            carGarage.SetActive(false);
        }
    }

    void SelectCar(int i)
    {
        disable();
        Garages[i].SetActive(true);
        CurrentCar = CarBodies[i];
        CurrentCar.transform.rotation = currentRotation;
        currentColor = "Default";
    }

    void startGame()
    {
        SceneManager.LoadScene(1);
        carData.GetComponent<CarData>().carType = CurrentCar.name;
        carData.GetComponent<CarData>().carColor = currentColor;
    }

    void SelectHotrod()
    {
        SelectCar(0);
    }

    void SelectCoupe()
    {
        SelectCar(1);
    }

    void SelectBolide()
    {
        SelectCar(2);
    }

    void SelectIcecreamTruck()
    {
        SelectCar(3);
    }

    void SelectSportscar()
    {
        SelectCar(4);
    }

    void SelectSchoolBus()
    {
        SelectCar(5);
    }

    void SelectBulldozer()
    {
        SelectCar(6);
    }

    void SelectPicupTruck()
    {
        SelectCar(7);
    }

    void SelectBlack()
    {
        Material color = Colors[0];
        CurrentCar.GetComponent<ChangeCarColor>().ChangeColor(color);
        currentColor = "Black";
    }

    void SelectBlue()
    {
        Material color = Colors[1];
        CurrentCar.GetComponent<ChangeCarColor>().ChangeColor(color);
        currentColor = "Blue";
    }

    void SelectBrown()
    {
        Material color = Colors[2];
        CurrentCar.GetComponent<ChangeCarColor>().ChangeColor(color);
        currentColor = "Brown";
    }

    void SelectGray()
    {
        Material color = Colors[3];
        CurrentCar.GetComponent<ChangeCarColor>().ChangeColor(color);
        currentColor = "Gray";
    }

    void SelectGreen()
    {
        Material color = Colors[4];
        CurrentCar.GetComponent<ChangeCarColor>().ChangeColor(color);
        currentColor = "Green";
    }

    void SelectOrange()
    {
        Material color = Colors[5];
        CurrentCar.GetComponent<ChangeCarColor>().ChangeColor(color);
        currentColor = "Orange";
    }

    void SelectPink()
    {
        Material color = Colors[6];
        CurrentCar.GetComponent<ChangeCarColor>().ChangeColor(color);
        currentColor = "Pink";
    }

    void SelectRed()
    {
        Material color = Colors[7];
        CurrentCar.GetComponent<ChangeCarColor>().ChangeColor(color);
        currentColor = "Red";
    }

    void SelectTurquoise()
    {
        Material color = Colors[8];
        CurrentCar.GetComponent<ChangeCarColor>().ChangeColor(color);
        currentColor = "Turquoise";
    }

    void SelectViolet()
    {
        Material color = Colors[9];
        CurrentCar.GetComponent<ChangeCarColor>().ChangeColor(color);
        currentColor = "Violet";
    }

    void SelectWhite()
    {
        Material color = Colors[10];
        CurrentCar.GetComponent<ChangeCarColor>().ChangeColor(color);
        currentColor = "White";
    }

    void SelectYellow()
    {
        Material color = Colors[11];
        CurrentCar.GetComponent<ChangeCarColor>().ChangeColor(color);
        currentColor = "Yellow";
    }
}
