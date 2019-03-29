using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using UnityEngine.SceneManagement;

public class TitleScene : MonoBehaviour {
    SteamVR_Input_Sources inputSources;
    public SteamVR_Action_Single triggerAction;

    void Update() {
        if (SteamVR_Input._default.inActions.GrabPinch.GetStateDown(inputSources)) {
            SwitchScenes();
        }
    }

    void SwitchScenes() {
        SceneManager.LoadScene(1);
    }
}
