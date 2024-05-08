using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CarData : MonoBehaviour
{
    public string carType;
    public string carColor;
    private void Awake() {
        DontDestroyOnLoad(gameObject);
    }
}