using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VR;
using Valve.VR;

public class PlayerController :MonoBehaviour {
	public GameObject gadgetPrefab;
	public GameObject interactionHandPrefab;
	public Transform[] hands;

	public float deathHeight;
	bool hasFallen;

	// camera
	Quaternion freezeOffset;
	Quaternion lookAtOffset = Quaternion.identity;
	Quaternion initialRot;
	Quaternion targetRot;
	bool lookAtActive = false;
	float lookAtTime;
	float lookAtTimer = 0f;
	bool lockCamera = false;
	bool lerpActive = false;

	List<GunController> gadgets = new List<GunController>();
	List<InteractionController> interactionHands = new List<InteractionController>();

	Rigidbody rb;
	CapsuleCollider capsule;

	// vehicles
	Vehicle curVehicle;

	// time slow
	[Header("Time Slow")]
	public AnimationCurve timeCurve;
	public float timeSlowLength;
	float timeSlowTimer;
	bool slowActive = false;

	// Use this for initialization
	void Start () {
		for (int i = 0; i < hands.Length; i++) {
			GameObject gadget = Instantiate(gadgetPrefab, hands[i]);
			gadget.transform.localPosition = Vector3.zero;
			gadget.transform.localRotation = Quaternion.identity;
			gadgets.Add(gadget.GetComponent<GunController>());

			GameObject interactionHand = Instantiate(interactionHandPrefab, hands[i]);
			interactionHand.transform.localPosition = Vector3.zero;
			interactionHand.transform.localRotation = Quaternion.identity;
			interactionHands.Add(interactionHand.GetComponent<InteractionController>());
		}

		gadgets[0].otherGun = gadgets[1];
		gadgets[1].otherGun = gadgets[0];
		interactionHands[0] = interactionHands[1];
		interactionHands[1] = interactionHands[0];

		gadgets[0].conID = 0;
		gadgets[1].conID = 1;
		interactionHands[0].conID = 0;
		interactionHands[1].conID = 1;

		DisableHands();
		EnableHands(HandType.gadgets);

		rb = GetComponent<Rigidbody>();
		capsule = GetComponent<CapsuleCollider>();

		RecenterPlayer(true);
	}

	public void DisableHands () {
		foreach (var gadget in gadgets) {
			gadget.Detach();
			gadget.gameObject.SetActive(false);
		}
		foreach (var con in interactionHands) {
			con.Detach();
			con.gameObject.SetActive(false); ;
		}
	}

	public enum HandType {
		gadgets,
		interactionHands
	}
	public void EnableHands (HandType type) {
		if (type == HandType.gadgets) {
			foreach (var gadget in gadgets) {
				gadget.gameObject.SetActive(true);
			}
			foreach (var con in interactionHands) {
				con.Detach();
				con.gameObject.SetActive(false);
			}
		} else if (type == HandType.interactionHands) {
			foreach (var gadget in gadgets) {
				gadget.Detach();
				gadget.gameObject.SetActive(false);
			}
			foreach (var con in interactionHands) {
				con.gameObject.SetActive(true);
			}
		}
	}

	public void PlayerLerpTo (Vector3 point, float time) {
		StartCoroutine(LerpToPos(point, time));
	}

	IEnumerator LerpToPos (Vector3 targetPos, float time) {
		lerpActive = true;
		Vector3 initialPos = transform.position;
		float p = 0f;
		while (p < 1f) {
			transform.position = Vector3.Lerp(initialPos, targetPos, p);

			yield return new WaitForEndOfFrame();

			p += Time.deltaTime / time;
		}
		lerpActive = false;
	}

	public void CameraLookAt (Vector3 point, float time) {
		initialRot = transform.rotation;
		targetRot = Quaternion.LookRotation(point - transform.position) * Camera.main.transform.rotation;
		lookAtTime = time;
		lookAtTimer = 0f;
		lookAtActive = true;
		//LockCamera();
	}

	public void LockCamera () {
		lockCamera = true;
	}

	public void UnlockCamera () {
		lockCamera = false;
		transform.rotation = Quaternion.identity;
	}

	void LateUpdate () {
		if (lookAtActive) {
			lookAtTimer += Time.deltaTime;
			if (lookAtTimer >= lookAtTime) {
				// finish look at
				lookAtOffset = targetRot;
				lookAtActive = false;
			} else {
				lookAtOffset = Quaternion.Lerp(initialRot, targetRot, lookAtTimer / lookAtTime);
			}
		}
		if (lockCamera) {
			//transform.position = -UnityEngine.XR.InputTracking.GetLocalPosition(UnityEngine.XR.XRNode.CenterEye);
			freezeOffset = Quaternion.Inverse(UnityEngine.XR.InputTracking.GetLocalRotation(UnityEngine.XR.XRNode.CenterEye));
			transform.rotation = lookAtOffset * freezeOffset;
		} else if (lookAtActive) {
			transform.rotation = lookAtOffset;
		}

        if (!hasFallen && transform.position.y < deathHeight) {
            hasFallen = true;
            SceneFader.instance.FadeToScene(0, Color.black, 0.5f);
        }
	}

    void StartSwallow() {
        DisableHands();
		//Destroy(GetComponent<Collider>());
		SetRigidbodyActive(false);
        FindObjectOfType<WormController>().StartSwallowSequence();
    }

    public void SetRigidbodyActive(bool isActive) {
        rb.isKinematic = !isActive;
        if (!isActive) {
            rb.velocity = Vector3.zero;
        }
    }

    void OnTriggerEnter(Collider other) {
        if (other.gameObject.CompareTag("WormMouth")) {
            StartSwallow();
        } else if (other.gameObject.CompareTag("Vehicle")) {
			curVehicle = other.gameObject.GetComponent<Vehicle>();
			curVehicle.EnterVehicle();
		}
    }

	// recentering
	public void RecenterPlayer (bool compensate = false) {
		Vector3 curCenter = capsule.center;
		capsule.center = new Vector3(Camera.main.transform.localPosition.x, capsule.center.y, Camera.main.transform.localPosition.z);
		if (compensate) {
			transform.Translate(-capsule.center + curCenter);
		}
	}

	void Update () {
		if (lookAtActive || lerpActive) {
			return;
		}

		if (curVehicle != null) {
			RecenterPlayer(true);
		} else {
			RecenterPlayer();
		}

		if (Input.GetKeyDown(KeyCode.T)) {
			SlowTimeStart();
		}

		// time slow
		if (slowActive) {
			timeSlowTimer += Time.unscaledDeltaTime;
			if (timeSlowTimer >= timeSlowLength) {
				SlowTimeEnd();
			} else {
				float ratio = timeSlowTimer / timeSlowLength;
				float targetTimeScale = Mathf.Clamp01(timeCurve.Evaluate(ratio));
				Time.timeScale = targetTimeScale;
				MusicManager.instance.UpdatePitch(targetTimeScale * targetTimeScale);
			}
		}
	}

	// time slow
	public void SlowTimeStart () {
		if (!slowActive) {
			timeSlowTimer = 0f;
			slowActive = true;
		}
	}

	void SlowTimeEnd () {
		slowActive = false;
		Time.timeScale = 1f;
	} 
}
