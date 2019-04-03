using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DistanceTracker : MonoBehaviour {
    Transform player;
    Transform worm;
    Transform cam;

    Text trackerText;

	float maxDisplayedDist = 150;

	bool active = true;

	[Header("Active")]
	bool awaitingEnabling = true;
	Vector3 playerPos;
	public float enableDistance;

	[Header("Flashing")]
	public Color defaultColor;
	public Color warningColor;
	public float flashDistance;
	public float flashPeriod;
	float flashTimer;
	bool shouldFlash = false;
	bool flashing = false;

    void Start() {
        player = FindObjectOfType<PlayerController>().transform;
		playerPos = player.position;
        worm = FindObjectOfType<WormController>().transform;
        cam = player.GetComponentInChildren<Camera>().transform;

        trackerText = GetComponentInChildren<Text>();

		ToggleActive(false);
    }

	public void ToggleActive (bool active) {
		if (!active) {
			flashing = false;
		}

		this.active = active;
		trackerText.enabled = active;
	}

    void LateUpdate() {
		if (awaitingEnabling) {
			float dist = Vector3.Distance(playerPos, player.position);
			if (dist > enableDistance) {
				ToggleActive(true);
				awaitingEnabling = false;
			}
		}
		if (!active) {
			return;
		}

        transform.position = player.transform.position;
        transform.rotation = Quaternion.Euler(0, cam.transform.rotation.eulerAngles.y, 0);

		// flashing
		float distance = Vector3.Distance(player.position, worm.position);
		if (!flashing && distance < flashDistance) {
			StartFlash();
		} else if (shouldFlash && distance >= flashDistance) {
			shouldFlash = false;
		}

		if (flashing) {
			ApplyFlash();
		}

		// text
		if (distance <= maxDisplayedDist) {
			trackerText.text = "" + Mathf.RoundToInt(distance) + " m";
		} else {
			trackerText.text = "" + maxDisplayedDist + "+ m";
		}
	}

	void StartFlash () {
		flashTimer = 0f;
		shouldFlash = true;
		flashing = true;
	}

	void ApplyFlash () {
		flashTimer += Time.deltaTime;
		if (flashTimer > flashPeriod) { // cycle complete
			trackerText.color = defaultColor;
			if (shouldFlash) {
				flashTimer = 0f;
			} else {
				// end flashing
				flashing = false;
			}
		} else { // mid cycle update colors
			float timeRatio = flashTimer / flashPeriod;
			float curVal = (0.5f * Mathf.Cos(2f * 3.141f * timeRatio)) + 0.5f;
			trackerText.color = Color.Lerp(warningColor, defaultColor, curVal);
		}
	}
}
