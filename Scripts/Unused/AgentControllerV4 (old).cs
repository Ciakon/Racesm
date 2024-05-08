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

public class AgentControllerV4 : Agent
{
    public float motorForce = 300;
    public float steeringRange = 9;
    Rigidbody rigidBody;
    public List<GameObject> checkpoints;
    Vector3 startPosition;
    Quaternion startRotation;
    int currentStep = 0;
    int stepsSinceCheckpoint = 0;
    public int maxStepsPerCheckpoint = 300;
    public int distanceBetweenCheckpoints = 5;

    // Start is called before the first frame update
    void Start()
    {
        rigidBody = GetComponent<Rigidbody>();
        startPosition = transform.localPosition;
        startRotation = transform.localRotation;
    }

    public override void OnEpisodeBegin()
    {
        stepsSinceCheckpoint = 0;

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
        AddReward(-0.002f);

        Vector3 movementForce = vInput * motorForce * transform.forward;
        float carAngle = transform.rotation.eulerAngles.y + steeringRange * hInput;

        float x = transform.rotation.eulerAngles.x;
        float z = transform.rotation.eulerAngles.z;

        transform.rotation = Quaternion.Euler(x, carAngle, z);

        rigidBody.AddForce(movementForce, ForceMode.Impulse);

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

        AddReward(reward);

        if (checkpintDistance < 0.1f)
        {
            currentCheckpoint.GetComponent<Checkpoint>().isCollected = true;
            stepsSinceCheckpoint = 0;

            // If last checkpoint
            if (currentCheckpoint == checkpoints[checkpoints.Count - 1].transform)
            {
                AddReward(10f);
                EndEpisode();           
            }
            AddReward(1f);
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

    private void OnTriggerEnter(Collider other) {
        
    }

    // // punishment for hitting a wall
    // private void OnCollisionEnter(Collision other) {
    //     if (other.gameObject.tag == "Wall")
    //     {
    //         AddReward(-0.05f);
    //     }
    // }

    // // punishment for staying at a wall
    // private void OnCollisionStay(Collision other) {
    //     if (other.gameObject.tag == "Wall")
    //     {
    //         AddReward(-0.005f);
    //     }
    // }
}