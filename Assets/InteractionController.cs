using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class InteractionController : MonoBehaviour {
    public int conID;
    enum State {
        idle,
        grabing
    }
    State state = State.idle;

    public float interactionSize;
    InteractionObject curInteractionObj;

    // input
    SteamVR_Input_Sources inputSources;
    public SteamVR_Action_Single triggerAction;

    // references
    public InteractionController otherCon;
    public LayerMask grabMask;
    HandController hand;
    Transform center;
    Transform handVisuals;

	void Start () {
        handVisuals = transform.Find("Hand");
        center = transform.Find("Center");

        // initialize input
        if (transform.parent.name.Contains("Left")) {
            inputSources = SteamVR_Input_Sources.LeftHand;
        } else {
            inputSources = SteamVR_Input_Sources.RightHand;
        }
	}
	
	void Update () {
        if (SteamVR_Input._default.inActions.GrabPinch.GetStateDown(inputSources)) {
            // trigger down
            if (state == State.idle) {
                Transform possibleTarget = GetGrabTarget();
                if (possibleTarget != null) {
                    GrabAttach(possibleTarget);
                }
            }
        } else if (SteamVR_Input._default.inActions.GrabPinch.GetStateUp(inputSources)) {
            // trigger up
            if (state == State.grabing) {
                Detach();
            }
        } else if (state == State.grabing) {
            // check to make sure still in range
            if (GetGrabTarget(1.5f) != curInteractionObj.transform) {
                Detach();
            } else {
                // is still in range > update visuals
            }
        }
	}

    void GrabAttach (Transform target) {
        // make sure other hand is not grabbing
        if (otherCon.curInteractionObj.transform == target) {
            return;
        }

        curInteractionObj = target.GetComponent<InteractionObject>();
        curInteractionObj.StartInteraction(this);

        // visuals
        handVisuals.parent = curInteractionObj.GetHandParent();
        handVisuals.localPosition = Vector3.zero;
        handVisuals.localRotation = Quaternion.identity;

        state = State.grabing;
    }

    void Detach () {
        curInteractionObj.EndInteracting();

        // visuals
        handVisuals.parent = transform.GetChild(0);
        handVisuals.localPosition = Vector3.zero;
        handVisuals.localRotation = Quaternion.identity;

        state = State.idle;
    }

    Transform GetGrabTarget (float sizeMul = 1f) {
        Collider[] colls = Physics.OverlapSphere(center.position, interactionSize * sizeMul, grabMask);
        if (colls.Length > 0) {
            return colls[0].transform;
        } else {
            return null;
        }
    }
}
