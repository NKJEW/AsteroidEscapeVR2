using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class HandController : MonoBehaviour {
	// haptic
	public enum HapticType {
		grapple,
		grab,
		detach,
		retractDone
	}
	public SteamVR_Input_Sources handType;
	public SteamVR_Action_Vibration hapticAction;

	ushort grappleHaptic = 5000;
	ushort grabHaptic = 8000;
	ushort detachHaptic = 1000;
	ushort retractDoneHaptic = 500;

	public void SetHaptic (HapticType type) {
		ushort length = 0;
		if (type == HapticType.grapple) {
			length = grappleHaptic;
		} else if (type == HapticType.grab) {
			length = grabHaptic;
		} else if (type == HapticType.detach) {
			length = detachHaptic;
		} else if (type == HapticType.retractDone) {
			length = retractDoneHaptic;
		}
		TriggerHapticPulse(length);
	}

	void TriggerHapticPulse (ushort microSecondsDuration) {
		float seconds = (float)microSecondsDuration / 1000000f;
		hapticAction.Execute(0, seconds, 1f / seconds, 1, handType);
	}
}
