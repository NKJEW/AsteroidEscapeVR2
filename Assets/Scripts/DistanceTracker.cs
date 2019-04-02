using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DistanceTracker : MonoBehaviour {
    Transform player;
    Transform worm;
    Transform cam;

    Text trackerText;

    void Start() {
        player = FindObjectOfType<PlayerController>().transform;
        worm = FindObjectOfType<WormController>().transform;
        cam = player.GetComponentInChildren<Camera>().transform;

        trackerText = GetComponentInChildren<Text>();
    }

    void LateUpdate() {
        transform.position = player.transform.position;
        transform.rotation = Quaternion.Euler(0, cam.transform.rotation.eulerAngles.y, 0);

        trackerText.text = Mathf.RoundToInt(Vector3.Distance(player.position, worm.position)).ToString() + " m";
    }
}
