﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VR;

public class WormController : MonoBehaviour {
    enum State {
        Waiting,
        Chasing,
        Swallowing,
        Finished,
		Dead
    };
    State state;

	PlayerController player;
	public float startFollowDist;
	public float maxFollowDist;
	public float speed;
	public float speedIncreaseRate;
    public float minTargetUpdateTime;
    public float maxTargetUpdateTime;

    public float spawnDistance; //how far does the player have to be away before the worm will appear?

    [Space(15)]
    public Transform swallowStartPos;
    public Transform swallowEndPos;
	public Transform lookPoint;
    public float swallowTime;

    [Space(15)]
    public float lowIntensityDist;
    public float highIntensityDist;
    public float audioUpdateTime;
    float nextAudioRefresh;

    [Space(15)]
    public float minBellowTime;
    public float maxBellowTime;
    float nextBellow;
    public AudioSource bellowSound;
	public AudioSource hurtSound;
	public AudioSource deathSound;

    Vector3 nextTarget;
    float nextTargetUpdate;

	public int health;

    Rigidbody rb;
	Animator anim;

	void Start () {
		player = FindObjectOfType<PlayerController>();
        rb = GetComponent<Rigidbody>();
		anim = GetComponent<Animator>();
	}

    void Update() {
        if (Time.time > nextAudioRefresh) {
            MusicManager.instance.UpdateIntensity(CalculateMusicIntensity(), audioUpdateTime);
            nextAudioRefresh = Time.time + audioUpdateTime;
        }

        switch (state) {
            case State.Chasing:
                float dist = (transform.position - player.transform.position).magnitude;
                float curSpeed = speed;

                if (transform.position.z > nextTarget.z) {
                    UpdateTarget();
                }

                Vector3 curTarget = nextTarget;

                if (dist > maxFollowDist) {
                    curSpeed *= 10;
                    curTarget = player.transform.position;
                    transform.LookAt(player.transform);
                } else {
                    if ((player.transform.position.z - transform.position.z) < 50f) { //if the worm is close enough
                        curTarget = player.transform.position;
                    }

                    Quaternion goalRot = Quaternion.LookRotation((curTarget - transform.position).normalized, Vector3.up);
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, goalRot, 30f * Time.deltaTime);
                }

                rb.velocity = transform.forward * curSpeed;

                speed += speedIncreaseRate * Time.deltaTime;

                if (Time.time > nextBellow) {
                    Bellow();
                }

                if (Time.time > nextTargetUpdate) {
                    UpdateTarget();
                }

                break;
            case State.Waiting:
                if (player.transform.position.z < spawnDistance) {
                    return;
                }

                Activate();
                break;
		}
	}

    void UpdateTarget() {
        nextTargetUpdate = Time.time + Random.Range(minTargetUpdateTime, maxTargetUpdateTime);

        float zPos = transform.position.z + (speed * maxTargetUpdateTime) + 50f; //add some give so that the worm is never waiting
        Vector2 circlePos = Random.insideUnitCircle * 15f;
        nextTarget = new Vector3(circlePos.x, circlePos.y, zPos);
    }

    void Activate() {
        transform.position = player.transform.position - Vector3.forward * maxFollowDist + Vector3.down * 20; // start arbitrarily below the belt
        state = State.Chasing;
        Bellow();
    }

    void Bellow() {
        bellowSound.Play();
        nextBellow = Time.time + Random.Range(minBellowTime, maxBellowTime);
    }

    public void StartSwallowSequence() {
        state = State.Swallowing;
		rb.isKinematic = true;
		rb.velocity = Vector3.zero;
		rb.angularVelocity = Vector3.zero;

		player.CameraLookAt(lookPoint.position, 1.5f);
		StartCoroutine(SwallowSequence());
    }

    IEnumerator SwallowSequence() {
		Vector3 camOffset = Camera.main.transform.localPosition;

		player.PlayerLerpTo(swallowStartPos.position - camOffset, 1f);
		yield return new WaitForSeconds(1.01f);

        SceneFader.instance.FadeToScene(0, Color.black, swallowTime);

		player.PlayerLerpTo(swallowEndPos.position - camOffset, swallowTime);
		yield return new WaitForSeconds(swallowTime);
        
        OnSwallowComplete();
    }

    void OnSwallowComplete() {
        print("u ded");
        state = State.Finished;
    }

    float CalculateMusicIntensity() {
        float dist = Vector3.Distance(transform.position, player.transform.position);

        if (dist < 10) {
            Bellow();
        }

        return Mathf.Clamp01((speed / 30) + ((dist - lowIntensityDist) / (highIntensityDist - lowIntensityDist))); //linear falloff
    }

	// vitals 
	public void HitByBomb () {
		health--;
		if (health <= 0) {
			Die();
		} else {
			hurtSound.Play();
			player.SlowTimeStart();

			speedIncreaseRate *= 1.6f;
			speed *= 0.6f;
		}
	}

	void Die () {
		Destroy(transform.Find("SwallowZone").gameObject);
		StartCoroutine(SlowAnimation(5f));
		deathSound.Play();


		state = State.Dead;
	}

	IEnumerator SlowAnimation (float time) {
		float timer = 0f;
		while (timer < time) {
			anim.speed = Mathf.Clamp01(1f - (time / timer));
			timer += Time.deltaTime;

			yield return new WaitForEndOfFrame();
		}
	}
}
