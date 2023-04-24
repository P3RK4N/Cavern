using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMovement : MonoBehaviour
{

[SerializeField]
float maxSpeed = 1.0f;
[SerializeField]
float wanderStrength = 1.0f;
[SerializeField]
float steerStrength = 1.0f;

    Vector2 velocity;
    Vector2 direction;

    // Start is called before the first frame update
    void Start()
    {
        velocity = Vector2.zero;
        direction = Random.insideUnitCircle;

        int seed = Random.Range(0, 1000000000);
        Debug.Log(seed);
        Random.InitState(seed);
    }

    // Update is called once per frame
    void Update()
    {
        move();
    }

    void move()
    {
        direction = (direction + Random.insideUnitCircle * wanderStrength).normalized;

        Vector2 wantedVelocity = direction * maxSpeed;
        Vector2 steerVelocity = (wantedVelocity - velocity) * steerStrength;
        Vector2 acceleration = Vector2.ClampMagnitude(steerVelocity, steerStrength);

        velocity = Vector2.ClampMagnitude(velocity + acceleration * Time.deltaTime, maxSpeed);
        Vector3 velocity3d = new Vector3(velocity.x, 0, velocity.y);

        transform.position += velocity3d;

        transform.rotation = Quaternion.LookRotation(velocity3d, Vector3.up);
    }

    void OnTriggerEnter(Collider other) 
    {
        Vector3 point = other.ClosestPoint(transform.position);
        Debug.Log(point);
        Debug.DrawRay(point, transform.position - point, Color.green, 5);
    }
}
