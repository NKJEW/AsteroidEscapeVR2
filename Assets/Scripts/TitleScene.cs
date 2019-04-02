using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class TitleScene : MonoBehaviour {
    public SteamVR_Input_Sources inputSources;
    public SteamVR_Action_Single triggerAction;

	bool switching = false;

    void Update() {
        if (!switching && SteamVR_Input._default.inActions.GrabPinch.GetStateDown(inputSources)) {
            SwitchScenes();
        }
    }

    void SwitchScenes() {
		//print("INPPIUT");
		switching = true;
		SceneFader.instance.Fade(1.5f, 0f, true);
    }
}
