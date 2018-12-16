using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vehicle : MonoBehaviour {
    public float baseForwardSpeed;
    public float maxForwardSpeed;
    public float tiltSpeed;
    public float verticalSpeed;
    public Transform seat;

    bool isActive;

    Rigidbody rb;
    PlayerController player;

    void Start() {
        rb = GetComponent<Rigidbody>();
        player = FindObjectOfType<PlayerController>();
        EnterVehicle();
    }

    public void EnterVehicle() {
        player.transform.position = seat.transform.position;
        player.transform.parent = seat.transform;
        player.DisableGuns();
        player.SetRigidbodyActive(false);
        isActive = true;
        rb.velocity = new Vector3(0, 0, baseForwardSpeed);
    }

    void Update() {
        player.SetRigidbodyActive(false);
        GetMovementInput(new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")));
    }

    public void GetMovementInput(Vector2 input) {
        rb.angularVelocity = new Vector3(rb.angularVelocity.x, rb.angularVelocity.y, -input.x * tiltSpeed);
        rb.velocity = transform.rotation * new Vector3(rb.velocity.x, input.y * verticalSpeed, rb.velocity.z);
    }

    public void GetBoosterInput(Vector2 input) {
        float val = (input.x + 1) / 2f;
        rb.velocity = transform.rotation * new Vector3(rb.velocity.x, rb.velocity.y, Mathf.Lerp(baseForwardSpeed, maxForwardSpeed, input.x));
    }
}
