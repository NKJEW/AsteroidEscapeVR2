﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bomb : Grabable {
    public static bool hasGrabbedBomb;

	public float launchSpeed;
	Transform worm;
	public float homingStrength;
	public float fuze;
	public bool armed = false;
    public Material explosionMat;
    public Color lightColor;

    public AudioSource tickSound;
    public AudioClip explosionSound;

    public GameObject grabText;

    float nextTick;
    float curTickRate = 1f;

	Collider coll;
	Animator anim;
	Transform blinkingEffect;
	TrailRenderer tr;

	protected override void Init () {
		coll = GetComponent<Collider>();
		anim = GetComponent<Animator>();
		blinkingEffect = transform.Find("Blinking");
		tr = GetComponent<TrailRenderer>();
		tr.enabled = false;
        grabText.SetActive(!hasGrabbedBomb);
	}

	public void Attach () {
		coll.enabled = false;
		rb.isKinematic = true;
		transform.Find("Blinking").gameObject.SetActive(false);
		if (!hasGrabbedBomb) {
			Bomb[] allBombs = FindObjectsOfType<Bomb>();
			foreach (Bomb bomba in allBombs) {
				bomba.DisableGrabText();
			}
			hasGrabbedBomb = true;
		}
	}

    public void DisableGrabText() {
        grabText.SetActive(false);
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
		tr.enabled = true;

        nextTick = Time.time + curTickRate;

		worm = FindObjectOfType<WormController>().transform;
	}

	void Update () {
		if (armed) {
			fuze -= Time.deltaTime;
			if (fuze <= 0f) {
				Explode();
			} else {
				// home in on worm
				if (worm != null) {
					Vector3 targetVelocity = (worm.position - transform.position);
					rb.velocity = Vector3.RotateTowards(rb.velocity, targetVelocity, homingStrength * Time.deltaTime, 0f);
				}
			}

            if (Time.time > nextTick) {
                nextTick = Time.time + curTickRate;
                curTickRate *= 0.75f;
                tickSound.Play();
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
        TerrainGenerator.instance.CreateExplosion(transform.position, 7f, explosionMat, lightColor, explosionSound, true, true);
		ObjectDestroyed();
		Destroy(gameObject);
	}
}
