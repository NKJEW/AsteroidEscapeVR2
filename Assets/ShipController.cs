using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipController : MonoBehaviour {
	public static ShipController instance;

	[Header("Lighting")]
	public Light[] hangerLights;
	public Color emergencyColor;

	[Header("Misc")]
	public Transform hangerDebrisContainer;
	public float suckForce;
	public float spinForce;
	public WindZone wind;
	public ParticleSystem airParticles;
	public AudioSource alarm;
    public AudioSource doors;
    public AudioSource buttonSound;
	public LinePuffGenerator puffs;

	[Header("Explosion")]
	public Material explosionMat;
	public Color explosionColor;
	public AudioClip explosionSound;
	public Transform explosionDebrisContainer;

	Animator anim;

	private void Awake () {
		instance = this;
	}
	void Start () {
		anim = GetComponent<Animator>();
	}

	public void OpenHanger () {
		TerrainGenerator.instance.GenerateTerrain();
		StartCoroutine(HangerOpenRoutine());
	}

	private void ChangeLightColor (Color newColor) {
		foreach (var light in hangerLights) {
			light.color = newColor;
		}
		transform.Find("ShipMain").GetComponent<MeshRenderer>().materials[4].color = emergencyColor; // hacky
	}

	void SuckDebris () {
		for (int i = 0; i < hangerDebrisContainer.childCount; i++) {
			Rigidbody item = hangerDebrisContainer.GetChild(i).GetComponent<Rigidbody>();
			item.isKinematic = false;
			StartCoroutine(SuckObject(item, 1.2f));
			Destroy(item.gameObject, 20f);
		}
		ParticleSystem.EmissionModule em = airParticles.emission;
		em.rateOverTime = em.rateOverTime.constant / 2f;

		StartCoroutine(SuckObject(FindObjectOfType<PlayerController>().GetComponent<Rigidbody>(), 1.5f));
	}

	private void OnTriggerEnter (Collider other) {
		if (other.gameObject.layer == 12) { // interactable layer
			OpenHanger();
		}
	}

    void Update() {
        if (Input.GetKeyDown(KeyCode.Space)) {
            OpenHanger();
        }
    }

	private void OnCollisionEnter (Collision collision) {
		if (collision.gameObject.tag == "Deadly") {
			Explode();
		}
	}

	void Explode () {
		// enable debris
		explosionDebrisContainer.parent = null;
		for (int i = 0; i < explosionDebrisContainer.childCount; i++) {
			Rigidbody item = explosionDebrisContainer.GetChild(i).GetComponent<Rigidbody>();
			item.gameObject.SetActive(true);
			item.isKinematic = false;
			item.AddTorque(Random.onUnitSphere * spinForce * 1000f);
			Destroy(item.gameObject, 20f);
		}

		TerrainGenerator.instance.CreateExplosion(transform.position, 12f, explosionMat, explosionColor, explosionSound, true, false);
		Destroy(gameObject);
	}

	IEnumerator HangerOpenRoutine () {
        buttonSound.Play();
		anim.SetTrigger("Open");
		ChangeLightColor(emergencyColor);
		yield return new WaitForSeconds(1f);
        doors.Play();

		puffs.Init(4f);
		yield return new WaitForSeconds(0.4f);
		SuckDebris();
		alarm.Play();

		//yield return new WaitForSeconds(3f);
		//FindObjectOfType<DistanceTracker>().gameObject.SetActive(true);
	}

	IEnumerator SuckObject (Rigidbody rb, float duration) {
		float timer = 0f;
		while (timer < duration) {
			timer += Time.deltaTime;
			float forceRatio = timer / duration;
			wind.windMain = 100f * forceRatio;
			rb.AddForce(transform.forward * suckForce * forceRatio);
			rb.AddTorque(Random.onUnitSphere * spinForce * forceRatio);
			yield return new WaitForEndOfFrame();
		}
	}
}
