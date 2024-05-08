using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float motorTorque = 2000;
    public float brakeTorque = 2000;
    public float maxSpeed = 20;
    public float steeringRange = 30;
    public float steeringRangeAtMaxSpeed = 10;
    public float autoBrake;
    public AudioSource audio;
    WheelControl[] wheels;
    public Rigidbody rigidBody;
    [HideInInspector] public int checkpointsCollected = 0;
    public GameObject[] checkpoints;
    Vector3 startPosition;
    Quaternion startRotation;
    KeyCode resetCarKey = KeyCode.Backspace;

    // Start is called before the first frame update
    void Start()
    {
        if (rigidBody == null)
        {
            rigidBody = GetComponent<Rigidbody> ();
        }

        startPosition = transform.position;
        startRotation = transform.rotation;
        

        // Find all child GameObjects that have the WheelControl script attached
        wheels = GetComponentsInChildren<WheelControl>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {

        float vInput = Input.GetAxis("Vertical");
        float hInput = Input.GetAxis("Horizontal");


        float forwardSpeed = Vector3.Dot(transform.forward, rigidBody.velocity);

        float speedFactor = Mathf.InverseLerp(0, maxSpeed / 4, forwardSpeed);

        float currentMotorTorque = Mathf.Lerp(motorTorque, 0, speedFactor);

        float currentSteerRange = Mathf.Lerp(steeringRange, steeringRangeAtMaxSpeed, speedFactor);
        
        bool isAccelerating = Mathf.Sign(vInput) == Mathf.Sign(forwardSpeed);

        bool isStopping = vInput == 0; // range

        bool isBraking = (vInput < 0 && forwardSpeed > 0) || (vInput > 0 && forwardSpeed < 0);

        if (vInput > 0 && forwardSpeed < 0)
        {
            isAccelerating = false;
        }

        foreach (var wheel in wheels)
        {
            // Apply steering to Wheel colliders that have "Steerable" enabled
            if (wheel.steerable)
            {
                wheel.WheelCollider.steerAngle = hInput * currentSteerRange;
            }

            if (isBraking)
            {
                wheel.WheelCollider.motorTorque = 0;
                wheel.WheelCollider.brakeTorque = Mathf.Abs(vInput) * brakeTorque;
            }
            
            if (isAccelerating)
            {
                // Apply torque to Wheel colliders that have "Motorized" enabled
                wheel.WheelCollider.brakeTorque = 0;
                if (wheel.motorized)
                {
                    wheel.WheelCollider.motorTorque = vInput * currentMotorTorque;
                }
                
            }

            

            if (isStopping)
            {
                // If the user is trying to go in the opposite direction
                // apply brakes to all wheels
                wheel.WheelCollider.brakeTorque = Mathf.Abs(vInput) * brakeTorque + autoBrake;

                if (forwardSpeed < 0)
                {
                    wheel.WheelCollider.brakeTorque = (Mathf.Abs(vInput) * brakeTorque + autoBrake) * 5;
                }

                // wheel.WheelCollider.motorTorque = 0;
            }
        }

        //idk camera mobning rammer hårdt
        // float THINP = hInput/10;
        // if (THINP != hInput)
        // {
        //     THINP+=hInput/10;
        // }


        //lookat.transform.localPosition = new Vector3 (THINP*1.5f,1,4);


    }

    private void Update() {
        Transform currentCheckpoint = checkpoints[checkpointsCollected].transform;
        float checkpintDistance = distanceToCheckpoint(currentCheckpoint);
        

        // send back to previous checkpoint if stuck

        if (Input.GetKeyDown(resetCarKey))
        {
            if (checkpointsCollected == 0)
            {
            transform.position = startPosition;
                transform.rotation = startRotation;
            }

            else
            {
                transform.position = new Vector3(
                    checkpoints[checkpointsCollected - 1].transform.position.x,
                    transform.position.y + 3,
                    checkpoints[checkpointsCollected - 1].transform.position.z
                );

                transform.eulerAngles = new Vector3(
                    transform.eulerAngles.x,
                    checkpoints[checkpointsCollected - 1].transform.eulerAngles.y,
                    transform.eulerAngles.z
                );
            }
        }

        
            

        if (checkpintDistance < 0.5f)
        {
            checkpointsCollected += 1;
            currentCheckpoint.GetComponent<Checkpoint>().isVisible = false;

            Transform nextCheckpoint;

            if (checkpointsCollected <= checkpoints.Count() - 1)
                nextCheckpoint = checkpoints[checkpointsCollected].transform;
            else
                nextCheckpoint = checkpoints[0].transform;

            nextCheckpoint.GetComponent<Checkpoint>().isVisible = true;
        }
    }

    private void OnCollisionEnter(Collision other) {
        if (other.gameObject.tag == "Wall")
        {
            // audio.Play();
        }
    }

    float distanceToCheckpoint(Transform checkpoint)
    {
        var closestPoint = checkpoint.GetComponent<Collider>().ClosestPointOnBounds(transform.position);
        var distanceToCheckpoint = Vector3.Distance(transform.position, closestPoint);
        return distanceToCheckpoint;
    }
}
