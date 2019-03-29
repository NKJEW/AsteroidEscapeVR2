﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class GunController : MonoBehaviour {
	public int conID;
	enum State
	{
		detached,
		extending,
		retracting,
		grappling,
		grabing,
		bombing
	};
	State state = State.detached;

	public float maxRange;
	public float swingForce;
	public float grabThrowForceMul;
	public float maxThrowForce;
	public float minGrappleTolerence;
	public float maxGrappleTolerence;
	public int grappleResolution;
	public float grabTolerence;
	public float grabberSpeed;
	public float bombLaunchRecoilForce;
	float grabberTime;
	float grabberTimer = 0f;
	float lastDist;
	Vector3 lastPos;
	Vector3 grabOffset;
	public float swingMultiplier;
	public LayerMask mask;
	public LayerMask collidableLayers;
	public Grabable grabed = null;

    [Space(15)]
    public GameObject puffParticle;
    public Material puffMat;

    [Space(15)]
    public AudioSource grappleSound;
    public AudioSource holdSound;
    public AudioSource bombGrabSound;
    public float lowPitch;
    public float highPitch;

	// visuals


	// input
	SteamVR_Input_Sources inputSources;
	public SteamVR_Action_Single triggerAction;

	// references
	public GunController otherGun;
	HandController hand;
	Animator grabAnimator;
	Transform grabber;
	Transform spawn;
	LineRenderer lr;
	PlayerController pc;
	Rigidbody rb;
	MeshRenderer barrelRenderer;
	CapsuleCollider capsule;
	
	void Start() {
		hand = transform.parent.GetComponent<HandController>();
		spawn = transform.Find("Offset").Find("Spawn");
		grabber = transform.Find("Offset").Find("Grabber");
		grabAnimator = grabber.GetComponent<Animator>();
		lr = GetComponent<LineRenderer>();
		lr.enabled = false;
		pc = GetComponentInParent<PlayerController>();
		rb = transform.parent.GetComponentInParent<Rigidbody>();
		capsule = GetComponentInParent<CapsuleCollider>();
		
		// initialize input
		if (transform.parent.name.Contains ("Left")) {
			inputSources = SteamVR_Input_Sources.LeftHand;
		} else {
			inputSources = SteamVR_Input_Sources.RightHand;
		}
	}

	void GrappleAttach () {
		state = State.grappling;

		UpdateGrappler(1f);

		Vector3 diff = grabed.GetAnchorPos() - spawn.position;
		lastDist = diff.magnitude;

		holdSound.Play();

		hand.SetHaptic(HandController.HapticType.grapple);
	}

	void Extend (AttachData data) {
		if (otherGun.grabed != null) {
			if (data.obj == otherGun.grabed.transform) {
				return;
			}
		}
		state = State.extending;
        grappleSound.Play();

		GetNewAnchor(data.pos, data.obj);

		lr.enabled = true;
		grabber.parent = null;
		UpdateGrappler(0f);

		// calculate timer
		grabberTimer = 0f;
		float dist = (grabed.GetAnchorPos() - spawn.position).magnitude;
		grabberTime = dist / grabberSpeed;
	}

	public void OtherGunGrabbed (Transform obj)
	{
		if (state == State.grabing) {
			Detach();
		} else if (state == State.grappling) {
			if (obj == grabed.transform) {
				Detach();
			}
		}
	}

	void GrabAttach (AttachData data) {
		otherGun.OtherGunGrabbed(data.obj);
		GetNewAnchor(data.pos, data.obj);

		if (data.obj.tag == "Bomb") {
			AttachBomb(data.obj.GetComponent<Bomb>());
		} else {
			state = State.grabing;
			holdSound.Stop();
			rb.isKinematic = true;

			grabOffset = rb.transform.position - data.pos;
			lastPos = spawn.position;

			grabAnimator.SetBool("IsGrabbing", true);
		}

		hand.SetHaptic(HandController.HapticType.grab);
	}

	void EndGrab (bool attached, bool hasBomb = false)
	{
		state = State.detached;

		if (hasBomb) {
			LaunchBomb();
		} else {
			rb.isKinematic = false;
			if (!attached) {
				// calculate departing velocity
				Vector3 diff = lastPos - spawn.position;
				Vector3 force = Vector3.ClampMagnitude(diff * grabThrowForceMul, maxThrowForce);
				rb.AddForce(force);
			}
		}

		if (grabed != null) {
			grabed.DestoryAnchor();
			grabed = null;
		}

		grabAnimator.SetBool("IsGrabbing", false);
	}

	public void Detach (bool attached = false)
	{
        holdSound.Stop();	

		if (state == State.grabing || state == State.bombing) {
			EndGrab(attached, state == State.bombing);
		} else {
			Retract();
		}

		//hand.SetHaptic(HandController.HapticType.detach);
	}

	void Retract () {
		state = State.retracting;
	}

	void FinishRetract () {
		grabber.parent = transform.GetChild(0);
		UpdateGrappler(0f);
		lr.enabled = false;

		state = State.detached;

		if (grabed != null) {
			grabed.DestoryAnchor();
			grabed = null;
		}

		hand.SetHaptic(HandController.HapticType.retractDone);
	}

	void AttachBomb (Bomb bomb) {
		bomb.Attach();

		Transform bombPoint = transform.Find("Offset").Find("BombPoint");
		bomb.transform.parent = bombPoint;
		bomb.transform.localPosition = Vector3.zero;
		bomb.transform.rotation = bombPoint.rotation;

        bombGrabSound.Play();

		state = State.bombing;
	}

	void LaunchBomb () {
		Bomb bomb = grabed as Bomb;
		bomb.transform.parent = null;
		bomb.Launch(rb.velocity);
		//CreatePuff();
		rb.AddForce(-bomb.transform.forward * bombLaunchRecoilForce);
	}

	void CreatePuff() {
        for (int i = 0; i < 6; i++) {
            Vector3 offset = Random.insideUnitSphere * 0.05f;
            GameObject newPart = Instantiate(puffParticle, spawn.position + offset, Random.rotation);
			newPart.GetComponent<Renderer>().material = puffMat;
            newPart.GetComponent<ExplosionParticle>().Init(Random.Range(0f, 0.2f), 1f, Random.Range(0.1f, 0.15f), offset, rb.velocity);
        }
    }

	void GetNewAnchor (Vector3 position, Transform obj)
	{
		grabed = obj.GetComponent<Grabable>();
		grabed.CreateAnchor(position, this);
	}

	void UpdateGrappler (float timeRatio)
	{
		Vector3 curPos = spawn.position;
		if (timeRatio == 0f) {
			grabber.transform.position = spawn.position;
			grabber.transform.rotation = spawn.rotation;
		} else if (timeRatio != 1f && grabed != null) {
			grabber.transform.LookAt(grabed.GetAnchorPos());
			curPos = Vector3.Lerp(spawn.position, grabed.GetAnchorPos(), timeRatio);
		} else if (timeRatio == 1f) {
			curPos = grabed.GetAnchorPos();
		}

		lr.SetPosition(0, spawn.position);
		lr.SetPosition(1, curPos);
		grabber.transform.position = curPos;
	}

	void Update() {
		if (SteamVR_Input._default.inActions.GrabPinch.GetStateDown(inputSources)) {
			if (state == State.detached) {
				// grab takes priority
				AttachData? target = GetGrabTarget();
				if (target.HasValue) {
					GrabAttach(target.Value);
				} else {
					AttachData data = GetGrappleTarget();
					if (data.obj != null) {
						Extend(data);
					}
				}
			}
		}

		if (state != State.retracting && SteamVR_Input._default.inActions.GrabPinch.GetStateUp(inputSources)) {
			Detach();
		}

		if (state == State.extending) {
			grabberTimer += Time.deltaTime;
			if (grabberTimer > grabberTime) {
				GrappleAttach();
			} else {
				UpdateGrappler(grabberTimer / grabberTime);
			}
		} else if (state == State.retracting) {
			grabberTimer -= Time.deltaTime * 2f;
			if (grabberTimer <= 0f) {
				FinishRetract();
			} else {
				UpdateGrappler(grabberTimer / grabberTime);
			}
		} else if (state == State.grappling) {
			UpdateGrappler(1f);

			Vector3 diff = grabed.GetAnchorPos() - spawn.position;
			float dist = diff.magnitude;
			float distDiff = dist - lastDist;
			float forceMul = 1f;
			if (distDiff > 0) {
				forceMul = 1f + (distDiff * swingMultiplier); // to make it so that the force is greater when you are on the upside of a swing
			}

            //holdSound.pitch = (lowPitch - highPitch) * Mathf.Clamp01(dist / maxRange) + highPitch;

			if (dist > grabTolerence * 2f) {
				Vector3 force = diff.normalized * swingForce * forceMul * Time.deltaTime;
				rb.AddForce(force);
				grabed.AddForce(-force);
			}
			lastDist = dist;

			AttachData? possibleGrab = GetGrabTarget();
			if (possibleGrab.HasValue) {
				FinishRetract();
				GrabAttach(possibleGrab.Value);
			}
		} else if (state == State.grabing) {
			Vector3 handDiff = lastPos - spawn.position;
			Vector3 targetPos = grabed.GetAnchorPos() + grabOffset + handDiff;

			// test to see if the move would put the player into an object
			Vector3 capsuleOffset = new Vector3(capsule.center.x, 0f, capsule.center.z);
			Collider[] colls = Physics.OverlapCapsule(targetPos + capsuleOffset + Vector3.up * (capsule.radius), targetPos + capsuleOffset + Vector3.up * (capsule.height - capsule.radius), capsule.radius, collidableLayers);

			if (colls.Length > 0) {
				pc.RecenterPlayer(true);
			} else { // position valid
				grabOffset += handDiff;
				rb.transform.position = grabOffset + grabed.GetAnchorPos();
				//RecenterPlayer();
			}

			lastPos = spawn.position;

			// check to make sure still on wall
			AttachData? possibleGrab = GetGrabTarget();
			if (!possibleGrab.HasValue) {
				Detach();
				return;
			}

			// apply downward force to object we are grabbing
			grabed.AddForce(rb.mass * Physics.gravity);
		}

		//if (otherGun.state != State.grabing && state != State.grabing) {
		//	RecenterPlayer();
		//}
	}

	AttachData GetGrappleTarget ()
	{
		RaycastHit hit;
		float distStep = maxRange / grappleResolution;
		float sizeStep = (maxGrappleTolerence - minGrappleTolerence) / grappleResolution;

		float curSize = minGrappleTolerence;
		Vector3 curPos = spawn.position;

		for (int i = 0; i < grappleResolution; i++) {
			Physics.SphereCast(curPos, curSize, spawn.transform.forward, out hit, distStep, mask);
			//Debug.DrawLine(curPos + Vector3.up * curSize, curPos + spawn.transform.forward * distStep, Color.yellow, 1f);
			if (hit.transform != null) {
				return new AttachData(hit.point, hit.transform);
			}
			curPos += spawn.transform.forward * distStep;
			curSize += sizeStep;
		}
		return new AttachData (Vector3.zero, null);
	}

	AttachData? GetGrabTarget ()
	{
		Collider[] colls = Physics.OverlapSphere(spawn.position, grabTolerence, mask);
		if (colls.Length > 0) {
			return new AttachData (transform.position, colls[0].transform);
		} else {
			return null;
		}
	}

	private struct AttachData
	{
		public Vector3 pos;
		public Transform obj;

		public AttachData (Vector3 pos, Transform obj)
		{
			this.pos = pos;
			this.obj = obj;
		}
	}
}
