using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleScene : MonoBehaviour {
	bool switching = false;

    void Update() {
        if (!switching && OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.RTouch)) {
            SwitchScenes();
        }
    }

    void SwitchScenes() {
		switching = true;
		SceneFader.instance.Fade(1.5f, 0f, true, 1);
    }
}
