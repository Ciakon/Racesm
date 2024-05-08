using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


//script from FREAKINGREX, but changes to integrate it proberly/ work how we want it to work
//https://assetstore.unity.com/packages/templates/systems/arcade-car-controller-lite-version-145489

public class CameraControl :MonoBehaviour
{
 
    [SerializeField] KeyCode SetCameraKey = KeyCode.C;                              //Set next camore on PC hotkey.
	[SerializeField] UnityEngine.UI.Button NextCameraButton;
    [SerializeField] List<CameraPreset> CamerasPreset = new List<CameraPreset>();  //Camera presets

	int ActivePresetIndex = 0;
	CameraPreset ActivePreset;
	public GameObject Loader;
	public GameObject TargetCar;
	GameController GameController { get { return GameController.Instance; } }

    float SqrMinDistance;

    Vector3 TargetPoint
	{
		get
		{
			if (TargetCar == null)
			{
				return transform.position;
			}
            Rigidbody carRB = TargetCar.GetComponent<Rigidbody>(); 
			Vector3 result = carRB.velocity * ActivePreset.VelocityMultiplier*0.05f;
			result += TargetCar.transform.position;
			result.y = 0;
			return result;
		}
	}

	public void getCar(GameObject car)
	{
		TargetCar = car;
	}
	
		

    private void Awake() {
        CamerasPreset.ForEach (c => c.CameraHolder.SetActive(false));
		UpdateActiveCamera ();

		//TargetCar = Loader.GetComponent<CarLoader>().targetcar;

		if (NextCameraButton)
		{
			NextCameraButton.onClick.AddListener (SetNextCamera);
		}
    }

    private void FixedUpdate ()
	{
		if (ActivePreset.EnableRotation && (TargetPoint - transform.position).sqrMagnitude >= SqrMinDistance)
		{
			Quaternion rotation = Quaternion.LookRotation (TargetPoint - transform.position, Vector3.up);
			ActivePreset.CameraHolder.transform.rotation = Quaternion.Lerp (ActivePreset.CameraHolder.transform.rotation, rotation, Time.deltaTime * ActivePreset.SetRotationSpeed);
		}

		transform.position = Vector3.LerpUnclamped (transform.position, TargetPoint, Time.deltaTime * ActivePreset.SetPositionSpeed);


		if (Input.GetKeyDown (SetCameraKey))
		{
			SetNextCamera ();
		}
	}



    private IEnumerator Start ()
	{
		// TargetCar = Loader.GetComponent<CarLoader>().targetcar;
	
		while (GameController == null)
		{
			yield return null;
		}
		transform.position = TargetPoint;
	}

    public void SetNextCamera ()
	{
		ActivePresetIndex = LoopClamp (ActivePresetIndex + 1, 0, CamerasPreset.Count);
		UpdateActiveCamera ();
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

    public void UpdateActiveCamera ()
	{

		if (ActivePreset != null)
		{
			ActivePreset.CameraHolder.SetActive(false);
		}

		ActivePreset = CamerasPreset[ActivePresetIndex];
		ActivePreset.CameraHolder.SetActive(true);

		SqrMinDistance = ActivePreset.MinDistanceForRotation * 2;

		if (ActivePreset.EnableRotation && (TargetPoint - transform.position).sqrMagnitude >= SqrMinDistance)
		{
			Quaternion rotation = Quaternion.LookRotation (TargetPoint - transform.position, Vector3.up);
			ActivePreset.CameraHolder.transform.rotation = rotation;
		}
	}




    [System.Serializable]

    class CameraPreset
	{
        public GameObject CameraHolder;   
		public float SetPositionSpeed = 1;              //Change position speed.
		public float VelocityMultiplier;                //Velocity of car multiplier.

		public bool EnableRotation;
		public float MinDistanceForRotation = 0.1f;     //Min distance for potation, To avoid uncontrolled rotation.
		public float SetRotationSpeed = 1;              //Change rotation speed.
	}
}
