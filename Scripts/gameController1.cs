using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class gameController1 : MonoBehaviour
{
    [SerializeField] KeyCode NextCarKey = KeyCode.N;
	[SerializeField] UnityEngine.UI.Button NextCarButton;
	public static gameController1 Instance;
	public GameObject PlayerCar;
	public static bool RaceIsStarted { get { return true; } }
	public static bool RaceIsEnded { get { return false; } }

    PlayerController m_PlayerCar;
	List<PlayerController> Cars = new List<PlayerController>();
	int CurrentCarIndex = 0;

	protected virtual void Awake ()
	{

		Instance = this;

		//Find all cars in current game.
		Cars.AddRange (GameObject.FindObjectsOfType<PlayerController> ());
		Cars = Cars.OrderBy(c => c.name).ToList();

		foreach (var car in Cars)
		{
			var userControl = car.GetComponent<PlayerController>();
			var audioListener = car.GetComponent<AudioListener>();

			if (userControl == null)
			{
				userControl = car.gameObject.AddComponent<PlayerController> ();
			}

			if (audioListener == null)
			{
				audioListener = car.gameObject.AddComponent<AudioListener> ();
			}

			userControl.enabled = false;
			audioListener.enabled = false;
		}

		m_PlayerCar = Cars[0];
		m_PlayerCar.GetComponent<PlayerController> ().enabled = true;
		m_PlayerCar.GetComponent<AudioListener> ().enabled = true;

		if (NextCarButton)
        {
			NextCarButton.onClick.AddListener (NextCar);
		}
	}

	void Update () 
	{ 
		if (Input.GetKeyDown (NextCarKey))
		{
			NextCar ();
		}

	}

	private void NextCar ()
	{
		m_PlayerCar.GetComponent<PlayerController> ().enabled = false;
		m_PlayerCar.GetComponent<AudioListener> ().enabled = false;

		CurrentCarIndex = LoopClamp (CurrentCarIndex + 1, 0, Cars.Count);

		m_PlayerCar = Cars[CurrentCarIndex];
		m_PlayerCar.GetComponent<PlayerController> ().enabled = true;
		m_PlayerCar.GetComponent<AudioListener> ().enabled = true;
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
