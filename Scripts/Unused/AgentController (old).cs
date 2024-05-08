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

public class AgentControllerOld : Agent
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
    int stepsSinceCheckpoint = 0;
    public int maxStepsPerCheckpoint = 300;


    // Start is called before the first frame update
    [System.Obsolete]
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
        float checkpointAngle = angleToCheckpoint(currentCheckpoint);

        sensor.AddObservation(checkpointAngle);

        float checkpintDistance = distanceToCheckpoint(currentCheckpoint);

        sensor.AddObservation(checkpintDistance);

        // Agent velocity
        var FullVelocityMagnitude = rigidBody.velocity.magnitude; // Velocity including angular velocity

        sensor.AddObservation(FullVelocityMagnitude);

        
        // sensor.AddObservation(wheels[0].WheelCollider.motorTorque);
        // sensor.AddObservation(wheels[0].WheelCollider.brakeTorque);
        // sensor.AddObservation(wheels[0].WheelCollider.steerAngle);

        // // calculate forward velocity
        // var FullVelocityMagnitude = rigidBody.velocity.magnitude; // Velocity including angular velocity
        // var angularMagnitude = rigidBody.angularVelocity.magnitude;

        // var forwardMagnitude = Mathf.Sqrt( Mathf.Pow(FullVelocityMagnitude, 2) - Mathf.Pow(angularMagnitude, 2)); // Agent velocity in forward direction

        // // add obserevations
        // if (forwardMagnitude >= 0.001)
        //     sensor.AddObservation(forwardMagnitude);
        // else
        //     sensor.AddObservation(FullVelocityMagnitude);

        // sensor.AddObservation(angularMagnitude);

    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        
        print("L");

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

        // reward for going forward

        if (vInput == 1f)
        {
            AddReward(0.02f);
        }

        // give benson mental pain for existing (punishment for maximizing first checkpoint by standing still)
        AddReward(-0.002f);

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

        // float reward = (1 - Mathf.InverseLerp(0, 20, checkpintDistance)) / 1000;

        // AddReward(reward);

        if (checkpintDistance < 0.1f)
        {
            currentCheckpoint.GetComponent<Checkpoint>().isCollected = true;
            stepsSinceCheckpoint = 0;

            if (currentCheckpoint == checkpoints[checkpoints.Count - 1].transform)
            {
                AddReward(10f);
                EndEpisode();           
            }
            AddReward(1.0f);
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

    // find angle from agent to middle of checkpoint line.
    float angleToCheckpoint(Transform checkpoint)
    {
        Vector3 checkpointDirection = checkpoint.localPosition - transform.localPosition;

        float angle = Vector3.SignedAngle(transform.forward, checkpointDirection, Vector3.up);
        return angle;
    }

    // punishment for hitting a wall
    private void OnCollisionEnter(Collision other) {
        if (other.gameObject.tag == "Wall")
        {
            AddReward(-0.05f);
        }
    }

    // punishment for staying at a wall
    private void OnCollisionStay(Collision other) {
        if (other.gameObject.tag == "Wall")
        {
            AddReward(-0.005f);
        }
    }
}