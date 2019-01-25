using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SegmentController : MonoBehaviour {
	[Header("Shaking")]
	public float duration;
	public float minMagnitude;
    public float maxMagnitude;
    public int minOscillations;
    public int maxOscillations;

	[Header("Exploding")]
	public Material explosionMat;
	public Color lightColor;
	public AudioClip explosionSound;
	public int maxExplosions;
	public float explosionRadius;

	[Header("Debris")]
	public GameObject[] debrisPrefabs;
	public float debrisSpinForce;
	public int minDebris;
	public int maxDebris;

	[Header("Head Misc")]
	public bool isHead = false;
	public GameObject jawPrefab;


	private void Start () {
		//StartDeathSequence();
	}
	public void StartDeathSequence() {
        StartCoroutine(DeathSequence());
    }

    IEnumerator DeathSequence() {
        float magnitude = Random.Range(minMagnitude, maxMagnitude);
        int numOscillations = Random.Range(minOscillations, maxOscillations);
        Vector3 offsets = Random.insideUnitSphere;
        Vector3 initialPos = transform.position;

        float t = 0f;
        while (t < duration) {
            transform.position = initialPos + new Vector3(
                CalculateValueOnAxis(t, magnitude, offsets.x, duration, numOscillations),
                CalculateValueOnAxis(t, magnitude, offsets.y, duration, numOscillations),
                CalculateValueOnAxis(t, magnitude, offsets.z, duration, numOscillations)
            );
			t += Time.deltaTime;
			yield return new WaitForEndOfFrame();
        }

		Explode();
    }

    float CalculateValueOnAxis(float value, float magnitude, float offset, float time, int numOscillations) {
        return (magnitude * value / time) * Mathf.Sin(2 * Mathf.PI * ((numOscillations * value / time) - offset));
    }

	void Explode () {
		GetComponentInChildren<Collider>().enabled = false;

		int debrisCount = Random.Range(minDebris, maxDebris + 1);
		int explosionCount = Random.Range(1, maxExplosions + 1);

		// spawn debris
		for (int i = 0; i < debrisCount; i++) {
			Vector3 spawnPos = transform.position + (Random.onUnitSphere * explosionRadius);
			GameObject prefab = debrisPrefabs[Random.Range(0, debrisPrefabs.Length)];
			GameObject newDebris = Instantiate(prefab, spawnPos, Random.rotation);
			newDebris.GetComponent<Rigidbody>().AddTorque(Random.onUnitSphere * debrisSpinForce);
			Destroy(newDebris, 8f);
		}

		if (isHead) {
			SpawnHeadDebris();
		}

		// explode
		for (int i = 0; i < explosionCount; i++) {
			Vector3 spawnPos = transform.position + (Random.insideUnitSphere * explosionRadius);
			TerrainGenerator.instance.CreateExplosion(spawnPos, 7f, explosionMat, lightColor, explosionSound, true, false);
		}

		Destroy(gameObject);
	}

	void SpawnHeadDebris () {
		Transform mouth = transform.Find("Mouth");
		Transform[] jaws = new Transform[4];
		for (int i = 0; i < 4; i++) {
			jaws[i] = mouth.Find("Jaw" + i).GetChild(0).GetChild(0);
		}
		foreach (var jaw in jaws) {
			GameObject newJaw = Instantiate(jawPrefab, jaw.transform.position, jaw.transform.rotation);
			Destroy(newJaw, 20f);
		}
	}
}
