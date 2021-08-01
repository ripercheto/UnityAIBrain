using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
    public Rigidbody body;
    public float movementSpeed = 10;
    public float smoothTime = 0.5f;
    public Vector3 velocity;

    private void Update()
    {
        body.velocity = Vector3.SmoothDamp(body.velocity, velocity, ref velocity, smoothTime);
    }
}
