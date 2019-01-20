using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipController : MonoBehaviour {
	public static ShipController instance;

	[Header("Lighting")]
	public Light[] hangerLights;
	public Color emergencyColor;

	[Header("Misc")]
	public Transform debrisContainer;
	public float suckForce;
	public float spinForce;
	public WindZone wind;
	public ParticleSystem airParticles;

	Animator anim;

	private void Awake () {
		instance = this;
	}
	void Start () {
		anim = GetComponent<Animator>();
	}

	public void OpenHanger () {
		anim.SetTrigger("Open");
		ChangeLightColor(emergencyColor);
		SuckDebris();
	}

	private void ChangeLightColor (Color newColor) {
		foreach (var light in hangerLights) {
			light.color = newColor;
		}
		transform.Find("ShipMain").GetComponent<MeshRenderer>().materials[4].color = emergencyColor; // hacky
	}

	void SuckDebris () {
		for (int i = 0; i < debrisContainer.childCount; i++) {
			Rigidbody item = debrisContainer.GetChild(i).GetComponent<Rigidbody>();
			item.isKinematic = false;
			StartCoroutine(SuckObject(item, 1.2f));
			Destroy(item.gameObject, 20f);
		}
		wind.windMain = 30f;
		airParticles.Stop();

		StartCoroutine(SuckObject(FindObjectOfType<PlayerController>().GetComponent<Rigidbody>(), 1.5f));
	}

	IEnumerator SuckObject (Rigidbody rb, float duration) {
		float timer = 0f;
		while (timer < duration) {
			timer += Time.deltaTime;
			float forceRatio = timer / duration;
			rb.AddForce(transform.forward * suckForce * forceRatio);
			rb.AddTorque(Random.onUnitSphere * spinForce * forceRatio);
			yield return new WaitForEndOfFrame();
		}
	}
}
