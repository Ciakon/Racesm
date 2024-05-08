using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents; 
using Unity.MLAgents.Sensors; 
using Unity.MLAgents.Actuators;
using UnityEngine.UIElements;
using System.Linq;
using Unity.Mathematics;
using Unity.VisualScripting;
using System.Reflection;
using System;

public class AgentController: Agent
{
    public float motorTorque = 300;
    public float brakeTorque = 500;
    public float maxSpeed = 400;
    public float steeringRange = 9;
    public float steeringRangeAtMaxSpeed = 7;
    public float autoBrake = 100;
    WheelControl[] wheels;
    public List<GameObject> checkpoints;
    Rigidbody rb;
    Vector3 startPosition;
    Quaternion startRotation;
    int currentStep = 0;
    float totalReward = 0;
    float totalMentalPain = 0;
    int stepsSinceCheckpoint = 0;
    [HideInInspector] public int checkpointsCollected = 0;
    public int maxStepsPerCheckpoint = 300;
    public int distanceBetweenCheckpoints = 5;
    public bool ignoreMentalPain = true;
    bool isEnabled = true;
    public bool isPlaying = false;
    [HideInInspector] public bool isFinished = false; // needed for gamemanager

    protected override void OnDisable()
    {  
        isEnabled = false;
        return;
    }
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        // Find all child GameObjects that have the WheelControl script attached
        wheels = GetComponentsInChildren<WheelControl>();
        
        startPosition = transform.position;
        startRotation = transform.rotation;
        
    }

    public override void OnEpisodeBegin()
    {
        if (!isEnabled)
            return;

        stepsSinceCheckpoint = 0;
        totalReward = 0;
        totalMentalPain = 0;
        checkpointsCollected = 0;

        // don't reset car unless in training
        if (isPlaying)
            return;

        // reset wheels
        foreach (var wheel in wheels)
        {
            wheel.WheelCollider.brakeTorque = 0;
            wheel.WheelCollider.motorTorque = 0;
            wheel.WheelCollider.steerAngle = 0;
        }


        // reset car
        transform.position = startPosition;
        transform.rotation = startRotation;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // //randomize car position

        // float rng = UnityEngine.Random.Range(-3f, 3f);

        // transform.position = new Vector3(
        //     transform.position.x,
        //     transform.position.y,
        //     transform.position.z + rng
        // );

    }

    public override void CollectObservations(VectorSensor sensor)
    {
        if (!isEnabled)
        {
            for (int i = 0; i < 6; i++)
                sensor.AddObservation(0);
            return;
        }

        Transform currentCheckpoint = checkpoints[checkpointsCollected].transform;
        
        // distance to next checkpoint
        sensor.AddObservation(distanceToCheckpoint(currentCheckpoint));

        // relative angle to checkpoint
        sensor.AddObservation(angleToCheckpoint(currentCheckpoint));

        // relative vector pointing to checkpoint
        Vector3 position = transform.position;
        Vector3 checkpointPosition = currentCheckpoint.position;

        Vector3 toCheckpoint = new Vector3(
            checkpointPosition.x - position.x,
            0,
            checkpointPosition.z - position.z
        );

        float carAngle = transform.eulerAngles.y;

        toCheckpoint = Quaternion.Euler(0, -carAngle, 0) * toCheckpoint.normalized;

        sensor.AddObservation(toCheckpoint.x);
        sensor.AddObservation(toCheckpoint.z);

    
        // relative Velocity
        Vector3 velocity = new Vector3(
            rb.velocity.x,
            0,
            rb.velocity.z
        );

        Vector3 relativeVelocity = Quaternion.Euler(0, -carAngle, 0) * velocity;

        sensor.AddObservation(relativeVelocity.x);
        sensor.AddObservation(relativeVelocity.z);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {  
        if (!isEnabled)
            return;
        // Actions size = 2 [vertical speed, horizontal speed] = [-1..1, -1..1] // discrete = [{0, 1, 2}, {0, 1, 2}] = [{-1, 0, 1}...]
        float vInput = 0;
        float hInput = 0;

        if (actions.DiscreteActions[0] == 0)
            vInput = -1f;
        if (actions.DiscreteActions[0] == 1)
            vInput = 1f;

        if (actions.DiscreteActions[1] == 0)
            hInput = -1f;
        if (actions.DiscreteActions[1] == 1)
            hInput = 1f;

        // give benson mental pain for existing (punishment for maximizing first checkpoint by standing still)
        // AddReward(-0.002f);
        AddReward(-0.0018f); // less pain because of V4
        totalMentalPain -= 0.0018f;

        if (ignoreMentalPain)
            totalReward -= 0.0018f;

        float forwardSpeed = Vector3.Dot(transform.forward, rb.velocity);

        float speedFactor = Mathf.InverseLerp(0, maxSpeed / 4, forwardSpeed);

        float currentMotorTorque = Mathf.Lerp(motorTorque, 0, speedFactor);

        float currentSteerRange = Mathf.Lerp(steeringRange, steeringRangeAtMaxSpeed, speedFactor);

        // Check whether the user input is in the same direction 
        // as the car's velocity
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
                wheel.WheelCollider.brakeTorque = Mathf.Abs(vInput) * brakeTorque;
                //wheel.WheelCollider.motorTorque = 0;
            }
            
            if (isAccelerating)
            {
                // Apply torque to Wheel colliders that have "Motorized" enabled
                if (wheel.motorized)
                {
                    wheel.WheelCollider.motorTorque = vInput * currentMotorTorque;
                }
                wheel.WheelCollider.brakeTorque = 0;
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

            }
        }

        // rewards
        Transform currentCheckpoint = checkpoints[checkpointsCollected].transform;

        float checkpintDistance = distanceToCheckpoint(currentCheckpoint);
        float reward = (1 - Mathf.InverseLerp(0, distanceBetweenCheckpoints, checkpintDistance)) / 500;

        totalReward += reward;
        AddReward(reward);

        float checkpointAngle = angleToCheckpoint(currentCheckpoint);

        if (checkpointAngle > 0)
            reward = (1 - Mathf.InverseLerp(0, 60, checkpointAngle)) / 2000;
        else
            reward = Mathf.InverseLerp(-60, 0, checkpointAngle) / 2000;

        AddReward(reward);

        if (checkpintDistance < 0.1f)
        {
            stepsSinceCheckpoint = 0;
            
            // If last checkpoint
            if (checkpointsCollected == checkpoints.Count - 1)
            {
                AddReward(10f);

                if (isPlaying)
                { 
                    isFinished = true;
                }

                EndEpisode();         

                

            }

            checkpointsCollected += 1;

            //TODO fix variable names

            AddReward(1f);
            AddReward(-totalReward);

            totalReward = 0;
        }

        currentStep += 1;
        stepsSinceCheckpoint += 1;

        if (stepsSinceCheckpoint >= maxStepsPerCheckpoint)
        {
            stepsSinceCheckpoint = 0;

            if (isPlaying) // send back to previous checkpoint if stuck
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

                rb.velocity = Vector3.zero;
            }
            else
                EndEpisode();
        }

        // print(GetCumulativeReward());
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        if (!isEnabled)
            return;

        var discreteActionsOut = actionsOut.DiscreteActions;

        discreteActionsOut[0] = 2;
        discreteActionsOut[1] = 2;

        if (Input.GetAxis("Vertical") < -0.5)
            discreteActionsOut[0] = 0;
        if (Input.GetAxis("Vertical") > 0.5)
            discreteActionsOut[0] = 1;

        if (Input.GetAxis("Horizontal") < -0.5)
            discreteActionsOut[1] = 0;
        if (Input.GetAxis("Horizontal") > 0.5)
            discreteActionsOut[1] = 1;
    }

    // finds distance from agent to closest point on the checkpoint line
    float distanceToCheckpoint(Transform checkpoint)
    {
        var closestPoint = checkpoint.GetComponent<Collider>().ClosestPointOnBounds(transform.position);
        var distanceToCheckpoint = Vector3.Distance(transform.position, closestPoint);
        return distanceToCheckpoint;
    }

    float angleToCheckpoint(Transform checkpoint)
    {
        Vector3 checkpointDirection = checkpoint.position - transform.position;

        float angle = Vector3.SignedAngle(transform.forward, checkpointDirection, Vector3.up);
        return angle;
    }

    private void OnCollisionEnter(Collision other) {
        if (!isEnabled)
            return;

        // if (other.gameObject.tag == "NPC")
        // {
        //     AddReward(0.1f);
        // }
        if (other.gameObject.tag == "Player")
        {
            AddReward(-0.5f);
        }
    }
}