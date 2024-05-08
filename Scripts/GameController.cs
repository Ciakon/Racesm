using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Base class game controller.
/// </summary>
public class GameController :MonoBehaviour
{
	[SerializeField] KeyCode NextCarKey = KeyCode.N;
	[SerializeField] UnityEngine.UI.Button NextCarButton;
	public static GameController Instance;
	public GameObject PlayerCar;
	public GameObject cam;
	public static bool RaceIsStarted { get { return true; } }
	public static bool RaceIsEnded { get { return false; } }

	public List<GameObject> cars;
	int CurrentCarIndex = 0;

	
	private List<string> finished;

	void Start()
	{
		finished=gameObject.GetComponent<GameManager>().playersFinished;

		
	}

	public void Update () 
	{ 
		
		if (Input.GetKeyDown (NextCarKey))
		{
			NextCar ();
		}

		

	}

	public void NextCar()
	{
		for (int i = 0; i < cars.Count; i++)
		{
			if (finished.Contains(cars[i].name))
			{
				cars.Remove(cars[i]);
			}
		}

		CurrentCarIndex = LoopClamp (CurrentCarIndex + 1, 0, cars.Count);
	
		

		PlayerCar = cars[CurrentCarIndex];

		cam.GetComponent<CameraControl>().getCar(PlayerCar);
	}

    public static int LoopClamp (int value, int minValue, int maxValue)
	{
		while (value < minValue || value >= maxValue)
		{
			if (value < minValue)
			{
				value += maxValue - minValue;
			}
			else if (value >= maxValue)
			{
				value -= maxValue - minValue;
			}
		}
		return value;
	}

	
}
