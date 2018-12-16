using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionJoystick : InteractionObject {
    public Transform rotBase;
    public float maxAngle;

    void Update () {
        if (interacting) {
            Vector3 targetEulers = interactingCon.transform.localEulerAngles;
            Vector3 clampedEulers = new Vector3(Mathf.Clamp(targetEulers.x, -maxAngle, maxAngle), 0f, Mathf.Clamp(targetEulers.z, -maxAngle, maxAngle));
            rotBase.rotation = Quaternion.Euler(clampedEulers);

            // calculate output
            float x = rotBase.localEulerAngles.x / maxAngle;
            float z = rotBase.localEulerAngles.z / maxAngle;
            output = new Vector2(x, z);
        }
	}
}
