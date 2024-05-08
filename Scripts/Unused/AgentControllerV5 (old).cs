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

public class AgentControllerV5 : Agent
{
    public float motorTorque = 300;
    public float brakeTorque = 500;
    public float maxSpeed = 400;
    public float steeringRange = 9;
    public float steeringRangeAtMaxSpeed = 7;
    public float autoBrake = 100;
    WheelControl[] wheels;
    Rigidbody rigidBody;
    public List<GameObject> checkpoints;
    Vector3 startPosition;
    Quaternion startRotation;
    int currentStep = 0;
    float totalReward = 0;
    float totalMentalPain = 0;
    int stepsSinceCheckpoint = 0;
    int checkpointsReached = 0;
    public int maxStepsPerCheckpoint = 300;
    public int distanceBetweenCheckpoints = 5;
    public bool ignoreMentalPain = false;

    // Start is called before the first frame update
    void Start()
    {
        rigidBody = GetComponent<Rigidbody>();
        // Find all child GameObjects that have the WheelControl script attached
        wheels = GetComponentsInChildren<WheelControl>();

        startPosition = transform.localPosition;
        startRotation = transform.localRotation;
        
    }

    public override void OnEpisodeBegin()
    {
        stepsSinceCheckpoint = 0;
        checkpointsReached = 0;
        totalReward = 0;
        totalMentalPain = 0;

        // reset wheels
        foreach (var wheel in wheels)
        {
            wheel.WheelCollider.brakeTorque = 0;
            wheel.WheelCollider.motorTorque = 0;
            wheel.WheelCollider.steerAngle = 0;
        }


        // reset car
        transform.localPosition = startPosition;
        transform.localRotation = startRotation;
        rigidBody.velocity = Vector3.zero;
        rigidBody.angularVelocity = Vector3.zero;

        // reset checkpoints
        foreach (GameObject checkpoint in checkpoints)
        {
            checkpoint.GetComponent<Checkpoint>().isCollected = false;
        }
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        Transform currentCheckpoint = checkpoints[0].transform;
        foreach (GameObject checkpoint in checkpoints)
        {
            bool isCollected = checkpoint.GetComponent<Checkpoint>().isCollected;

            if (!isCollected)
            {
                currentCheckpoint = checkpoint.transform;
                break;
            }
        }

        // Agent rotation
        sensor.AddObservation(transform.localRotation.y);

        Vector3 position = transform.localPosition;
        Vector3 checkpointPosition = currentCheckpoint.localPosition;

        Vector2 toCheckpoint = new Vector2(
            checkpointPosition.x - position.x,
            checkpointPosition.z - position.z
        );

        // Normalized vector in direction of checkpoint and distancce to checkpoint.
        sensor.AddObservation(toCheckpoint.normalized);
        sensor.AddObservation(distanceToCheckpoint(currentCheckpoint));


        Vector2 velocity = new Vector2(rigidBody.velocity.x, rigidBody.velocity.z);

        // Velocity
        sensor.AddObservation(velocity);

        sensor.AddObservation(angleToCheckpoint(currentCheckpoint));
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
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

        float forwardSpeed = Vector3.Dot(transform.forward, rigidBody.velocity);
        // Calculate how close the car is to top speed
        // as a number from zero to one
        float speedFactor = Mathf.InverseLerp(0, maxSpeed / 4, forwardSpeed);

        // Use that to calculate how much torque is available 
        // (zero torque at top speed)
        float currentMotorTorque = Mathf.Lerp(motorTorque, 0, speedFactor);

        // …and to calculate how much to steer 
        // (the car steers more gently at top speed)

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
        Transform currentCheckpoint = checkpoints[0].transform;
        foreach (GameObject checkpoint in checkpoints)
        {
            bool isCollected = checkpoint.GetComponent<Checkpoint>().isCollected;

            if (!isCollected)
            {
                currentCheckpoint = checkpoint.transform;
                break;
            }
        }

        float checkpintDistance = distanceToCheckpoint(currentCheckpoint);

        float reward = (1 - Mathf.InverseLerp(0, distanceBetweenCheckpoints, checkpintDistance)) / 1000;

        totalReward += reward;
        AddReward(reward);

        print(GetCumulativeReward().ToString());
        

        if (checkpintDistance < 0.1f)
        {
            currentCheckpoint.GetComponent<Checkpoint>().isCollected = true;
            stepsSinceCheckpoint = 0;
            checkpointsReached += 1;

            // If last checkpoint
            if (currentCheckpoint == checkpoints[checkpoints.Count - 1].transform)
            {
                AddReward(10f);
                EndEpisode();           
            }

            //TODO fix variable names

            AddReward(1f);
            AddReward(-totalReward);

            totalReward = 0;

            print("checkpoint");
        }

        currentStep += 1;
        stepsSinceCheckpoint += 1;

        if (stepsSinceCheckpoint >= maxStepsPerCheckpoint)
        {
            stepsSinceCheckpoint = 0;
            EndEpisode();
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
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
        Vector3 checkpointDirection = checkpoint.localPosition - transform.localPosition;

        float angle = Vector3.SignedAngle(transform.forward, checkpointDirection, Vector3.up);
        return angle;
    }
}