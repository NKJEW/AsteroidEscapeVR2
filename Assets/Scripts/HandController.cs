using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandController : MonoBehaviour {
	// haptic
	public enum HapticType {
		grapple,
		grab,
		detach,
		retractDone
	}
    //public SteamVR_Input_Sources handType;
    //public SteamVR_Action_Vibration hapticAction;

    OVRInput.Controller ovrCon;
    public bool isRightHand;

    float grappleHaptic = 0.01f;
	float grabHaptic = 0.015f;
	float detachHaptic = 0.001f;
	float retractDoneHaptic = 0.002f;

    bool isVibing;
    float nextDisableTime;

    void Awake() {
        ovrCon = isRightHand ? OVRInput.Controller.RTouch : OVRInput.Controller.LTouch;
    }

	public void SetHaptic (HapticType type) {
		float length = 0;
		if (type == HapticType.grapple) {
			length = grappleHaptic;
		} else if (type == HapticType.grab) {
			length = grabHaptic;
		} else if (type == HapticType.detach) {
			length = detachHaptic;
		} else if (type == HapticType.retractDone) {
			length = retractDoneHaptic;
		}
        OVRInput.SetControllerVibration(1 / length, 1, ovrCon);
        isVibing = true;
        nextDisableTime = Time.time + length;
	}

    void Update() {
        if (isVibing && Time.time > nextDisableTime) {
            isVibing = false;
            OVRInput.SetControllerVibration(0, 0, ovrCon);
        }
    }

    public bool TriggerDown() {
        return OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, ovrCon);
    }

    public bool TriggerUp() {
        return OVRInput.GetUp(OVRInput.Button.PrimaryIndexTrigger, ovrCon);
    }

    public bool ButtonDown() {
        return OVRInput.GetDown(OVRInput.Button.One, ovrCon);
    }
}
