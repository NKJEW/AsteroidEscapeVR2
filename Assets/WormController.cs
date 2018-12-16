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

    Rigidbody rb;

	void Start () {
		player = FindObjectOfType<PlayerController>();
        rb = GetComponent<Rigidbody>();
	}
	
	void Update () {
        if (Time.time > audioUpdateTime) {
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

        GetComponent<AudioSource>().Play(); //for testing purposes
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
		Vector3 initialPos = player.transform.position;
		float p = 0f;
		while (p < 1f) {
			player.transform.position = Vector3.Lerp(initialPos, swallowStartPos.position, p);

			yield return new WaitForEndOfFrame();

			p += Time.deltaTime;
		}

		p = 0f;
		Vector3 camOffset = Camera.main.transform.localPosition;
        while (p < 1f) {
			player.transform.position = Vector3.Lerp(swallowStartPos.position, swallowEndPos.position, p) - camOffset;

			yield return new WaitForEndOfFrame();

			p += (Time.deltaTime / swallowTime);
        }

        OnSwallowComplete();
    }

    void OnSwallowComplete() {
        print("u ded");
        state = State.Finished;
    }

    float CalculateMusicIntensity() {
        float dist = Vector3.Distance(transform.position, player.transform.position);

        return Mathf.Clamp01((dist - lowIntensityDist) / (highIntensityDist - lowIntensityDist)); //linear falloff
    }
}
