using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionJoystick : InteractionObject {
    public Transform rotBase;
    public float maxAngle;

    void Update () {
        if (interacting) {
			//Vector3 defaultUp = transform.up;
			//Vector3 curUp = interactingCon.transform.GetChild(0).up;
			//float a = Vector3.Angle(defaultUp, curUp);
			//if (a > maxAngle) {
			//	curUp = Vector3.RotateTowards(curUp, defaultUp, (a - maxAngle) * Mathf.Deg2Rad, 0f);
			//}
			//transform.localRotation = Quaternion.LookRotation(curUp, Vector3.up);


			rotBase.rotation =  interactingCon.transform.GetChild(0).rotation;
			Vector3 clampedEulers = new Vector3(Mathf.Clamp(rotBase.localEulerAngles.x, -maxAngle, maxAngle), rotBase.localEulerAngles.y, Mathf.Clamp(rotBase.localEulerAngles.z, -maxAngle, maxAngle));
			//rotBase.localRotation = Quaternion.Euler(clampedEulers);
			// calculate output
			float x = Mathf.Clamp(rotBase.localEulerAngles.x / maxAngle, -1f, 1f);
			float z = Mathf.Clamp(rotBase.localEulerAngles.z / maxAngle, -1f, 1f);
			output = new Vector2(x, z);
		} else {
			transform.localRotation = Quaternion.identity;
			output = Vector2.zero;
		}
	}
}
