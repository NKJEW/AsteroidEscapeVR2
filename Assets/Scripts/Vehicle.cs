using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vehicle : MonoBehaviour {
    public float baseForwardSpeed;
    public float maxForwardSpeed;
    public float tiltSpeed;
    public float verticalSpeed;
    public Transform seat;
	public InteractionObject joy;
	public InteractionObject speedJoy;

	bool isActive;

    Rigidbody rb;
    PlayerController player;

    void Start() {
        rb = GetComponent<Rigidbody>();
        player = FindObjectOfType<PlayerController>();
        //EnterVehicle();
    }

    public void EnterVehicle() {
		player.SetRigidbodyActive(false);
		player.PlayerLerpTo(seat.position - Camera.main.transform.localPosition, 1f);
		//player.CameraLookAt(player.transform.position + transform.forward, 1f);
        //player.transform.position = seat.transform.position;
        player.transform.parent = seat.transform;
        player.EnableHands(PlayerController.HandType.interactionHands);
        isActive = true;
        //rb.velocity = new Vector3(0, 0, baseForwardSpeed);
    }

	private void Update () {
		if (isActive) {
			GetMovementInput(joy.output);
			GetBoosterInput(speedJoy.output);
			//Vector2 tempInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
			//GetMovementInput(tempInput);
			//GetBoosterInput(Vector2.right);
		}
	}

	public void GetMovementInput(Vector2 input) {
		transform.Rotate(new Vector3(input.y * tiltSpeed * Time.deltaTime, 0f, -input.x * tiltSpeed * Time.deltaTime), Space.Self);
        //rb.angularVelocity = new Vector3(input.y * tiltSpeed, 0f, -input.x * tiltSpeed);
    //    rb.velocity = transform.rotation * new Vector3(rb.velocity.x, input.y * verticalSpeed, rb.velocity.z);
    }

    public void GetBoosterInput(Vector2 input) {
		transform.Translate(Vector3.forward * Mathf.LerpUnclamped(baseForwardSpeed, maxForwardSpeed, input.x) * Time.deltaTime);
    }
}
