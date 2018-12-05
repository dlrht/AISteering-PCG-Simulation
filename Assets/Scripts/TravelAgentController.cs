﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TravelAgentController : MonoBehaviour {

    public float mass;
    public float maxForce;
    public float maxSpeed;
    public float MAX_SEE_AHEAD;
    public float MAX_AVOID_FORCE;
    private Vector3 velocity;
    public Transform target;
    private float size;
    private Rigidbody rb;

	// Use this for initialization
	void Start () {
        // Choose a random exit
        target = GameManager.INSTANCE.exitDoorways[Random.Range(0, GameManager.INSTANCE.exitDoorways.Length)].transform;
        size = GetComponent<Renderer>().bounds.size.magnitude;
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        Steer();  
    }

    void Steer() {
        var steering = Vector3.zero;
        steering += Seek(target);
        //steering = CollisionAvoidance();
        steering = Vector3.ClampMagnitude(steering,  maxForce); // truncate(steering, max_force)
        steering = steering / mass; // acceleration
        velocity = Vector3.ClampMagnitude(velocity + steering, maxSpeed);
        transform.forward = velocity.normalized;

        
        steering = CollisionAvoidance();
        steering = Vector3.ClampMagnitude(steering, maxForce); // truncate(steering, max_force)
        steering = steering / mass; // acceleration
        velocity = Vector3.ClampMagnitude(velocity + steering, maxSpeed);
        
        //transform.position += velocity * Time.deltaTime;
        // rb.position = rb.position + (velocity * Time.deltaTime);

        rb.MovePosition(transform.position + (velocity * Time.deltaTime));
        transform.forward = velocity.normalized;

        Debug.DrawRay(transform.position, velocity.normalized * MAX_SEE_AHEAD, Color.black);
        // Debug.DrawRay(transform.position, desiredVelocity.normalized * 2, Color.magenta);
    }

    Vector3 Seek(Transform target)
    {
        Vector3 t = target.transform.position;
        t = new Vector3(t.x, 0.225f, t.z);

        var desiredVelocity = t - transform.position;
        desiredVelocity = desiredVelocity.normalized * maxSpeed;

        return desiredVelocity - velocity;
    }

    Vector3 CollisionAvoidance()
    {
        // var dynamic_length = velocity.magnitude / maxSpeed;
        // var ahead = transform.position + (velocity.normalized * dynamic_length);

        var ahead = transform.position + (velocity.normalized * MAX_SEE_AHEAD);
        var ahead2 = transform.position + (velocity.normalized * (MAX_SEE_AHEAD * 0.5f));

        // Vector3 fwd = transform.TransformDirection(Vector3.forward);
        RaycastHit hit;
        Vector3 avoidance_force = Vector3.zero;

        if (Physics.SphereCast(transform.position, size*0.5f, velocity, out hit, MAX_SEE_AHEAD))
        {
            GameObject go = hit.transform.gameObject;
            if (go == GameManager.INSTANCE.exitDoorways[0] || go == GameManager.INSTANCE.exitDoorways[1]) return Vector3.zero;
            var obstacle_center = go.transform.GetComponent<Renderer>().bounds.center;
            obstacle_center = new Vector3(obstacle_center.x, 0.225f, obstacle_center.z);

            avoidance_force = ahead - obstacle_center;
            avoidance_force = avoidance_force.normalized * MAX_AVOID_FORCE;
        }

        return avoidance_force;
    }

    public void Respawn()
    {
        GameObject doorway = GameManager.INSTANCE.entranceDoorway;
        transform.position = new Vector3(doorway.transform.position.x, 0.225f, doorway.transform.position.z);
    }
}