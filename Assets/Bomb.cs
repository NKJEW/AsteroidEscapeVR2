﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bomb : Grabable {
	public float launchSpeed;
	public float fuze;
	public bool armed = false;

	Collider coll;
	Animator anim;
	Transform blinkingEffect;

	protected override void Init () {
		coll = GetComponent<Collider>();
		anim = GetComponent<Animator>();
		blinkingEffect = transform.Find("Blinking");
	}

	public void Attach () {
		coll.enabled = false;
		rb.isKinematic = true;
		transform.Find("Blinking").gameObject.SetActive(false);
	}

	public void Launch (Vector3 inheritVel) {
		armed = true;
		coll.enabled = true;
		coll.isTrigger = true;

		gameObject.layer = 0;

		rb.isKinematic = false;
		rb.drag = 0f;
		rb.velocity = (transform.forward * launchSpeed);
		rb.AddTorque(Random.insideUnitSphere * 50f, ForceMode.VelocityChange);

		anim.SetTrigger("Arm");
	}

	void Update () {
		if (armed) {
			fuze -= Time.deltaTime;
			if (fuze <= 0f) {
				Explode();
			}
		}
	}

	private void LateUpdate () {
		if (!armed) {
			blinkingEffect.transform.LookAt(Camera.main.transform.position);
		}
	}

	private void OnTriggerEnter (Collider other) {
		if (armed && other.tag != "Player") {
			Explode();
		}
	}

	void Explode () {
		TerrainGenerator.instance.CreateExplosion(transform.position, 8f);
		ObjectDestroyed();
		Destroy(gameObject);
	}
}
