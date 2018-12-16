using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VR;

public class WormController : MonoBehaviour {
    enum State {
        Waiting,
        Chasing,
        Swallowing,
        Finished
    };
    State state;

	PlayerController player;
	public float startFollowDist;
	public float maxFollowDist;
	public float speed;
	public float speedIncreaseRate;

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
    AudioSource bellowSound;

    Rigidbody rb;

	void Start () {
		player = FindObjectOfType<PlayerController>();
        rb = GetComponent<Rigidbody>();
        bellowSound = GetComponent<AudioSource>();
	}
	
	void Update () {
        if (Time.time > nextAudioRefresh) {
            MusicManager.instance.UpdateIntensity(CalculateMusicIntensity(), audioUpdateTime);
            nextAudioRefresh = Time.time + audioUpdateTime;
        }

        switch (state) {
            case State.Chasing:
                float dist = (transform.position - player.transform.position).magnitude;
                float curSpeed = speed;
                if (dist > maxFollowDist) {
                    curSpeed *= 10;
                }
                transform.LookAt(player.transform);
                rb.velocity = transform.forward * curSpeed;

                speed += speedIncreaseRate * Time.deltaTime;

                if (Time.time > nextBellow) {
                    Bellow();
                }

                break;
            case State.Waiting:
                if (player.transform.position.z < spawnDistance) {
                    return;
                }

                Activate();
                break;
            case State.Finished:
                break;
        }
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
}
