using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Experimental : MonoBehaviour
{
    public static Experimental instance;

    public GameObject one;
    public GameObject two;
    public GameObject three;
    public GameObject four;
    public GameObject five;

    public Text console;

    void Awake() {
        instance = this;
    }

    void Update() {
        if (OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.RTouch)) {
            one.SetActive(!one.activeInHierarchy);
        }
    }

    public void ActivateTwo() {
        two.SetActive(!two.activeInHierarchy);
    }

    public void ActivateThree() {
        three.SetActive(!three.activeInHierarchy);
    }

    public void ActivateFour() {
        four.SetActive(!four.activeInHierarchy);
    }

    public void ActivateFive() {
        five.SetActive(!five.activeInHierarchy);
    }

    public void UpdateConsole(string str) {
        console.text = str;
    }
}
