using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VR;
using Valve.VR;

public class PlayerController : MonoBehaviour {
	public GameObject gunPrefab;
	public Transform[] hands;

	// camera
	Quaternion freezeOffset;
	Quaternion lookAtOffset = Quaternion.identity;
	Quaternion initialRot;
	Quaternion targetRot;
	bool lookAtActive = false;
	float lookAtTime;
	float lookAtTimer = 0f;
	bool lockCamera = false;

    List<GunController> guns = new List<GunController>();

	// Use this for initialization
	void Start () {
		for (int i = 0; i < hands.Length; i++) {
			GameObject gun = Instantiate(gunPrefab, hands[i]);
			gun.transform.localPosition = Vector3.zero;
			gun.transform.localRotation = Quaternion.identity;
			guns.Add(gun.GetComponent<GunController>());
		}

		guns[0].otherGun = guns[1];
		guns[1].otherGun = guns[0];

		// temp
		//CameraLookAt(Vector3.right * 10f, 5f);
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
		} else {
			transform.rotation = lookAtOffset;
		}
	}

    void StartSwallow() {
        guns[0].Detach();
        Destroy(guns[0].gameObject);
        guns[1].Detach();
        Destroy(guns[1].gameObject);
		GetComponent<Rigidbody>().isKinematic = true;
        Destroy(GetComponent<Collider>());

        FindObjectOfType<WormController>().StartSwallowSequence();
    }

    void OnTriggerEnter(Collider other) {
        if (other.gameObject.CompareTag("WormMouth")) {
            StartSwallow();
        }
    }
}
