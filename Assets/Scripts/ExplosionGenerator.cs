using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionGenerator : MonoBehaviour {
    public Material smokeMat;
    public GameObject particlePrefab;

    Light explosionLight;
    public AnimationCurve lightCurve;

	public float maxLoadDistance;
    public float explosionForce;
	public AnimationCurve falloffCurve;

    float size;

    public void Init(float newSize, Material glowMat, Color lightColor, bool isPlayerBomb) {
        size = newSize;

        if (!isPlayerBomb && Vector3.Distance(FindObjectOfType<PlayerController>().transform.position, transform.position) > maxLoadDistance) {
            Destroy(gameObject);
            return;
        }

        int numGlowPieces = Random.Range(5, 7) * Mathf.CeilToInt(size / 2);
        for (int i = 0; i < numGlowPieces; i++) {
            Vector3 offset = Random.onUnitSphere * Random.Range(0.2f, 0.4f) * size;
            GameObject newPart = Instantiate(particlePrefab, transform.position + offset, Random.rotation);
            newPart.GetComponent<Renderer>().material = glowMat;
            newPart.GetComponent<ExplosionParticle>().Init(Random.Range(0f, 0.1f), Random.Range(1.7f, 2.4f), Random.Range(0.7f, 1f) * size, offset, Vector3.zero);
        }

        int numSmokePieces = Random.Range(9, 12) * Mathf.CeilToInt(size / 4);
        for (int i = 0; i < numSmokePieces; i++) {
            Vector3 offset = Random.onUnitSphere * Random.Range(1.2f, 1.4f) * size;
            GameObject newPart = Instantiate(particlePrefab, transform.position + offset, Random.rotation);
            newPart.GetComponent<Renderer>().material = smokeMat;
            newPart.GetComponent<ExplosionParticle>().Init(Random.Range(0.2f, 0.3f), Random.Range(1.8f, 2.3f), Random.Range(0.7f, 0.9f) * size / 1.25f, offset, Vector3.zero);
        }

        explosionLight = GetComponent<Light>();
        explosionLight.color = lightColor;
        StartCoroutine(LightSequence());

        Collider[] allCols = Physics.OverlapSphere(transform.position, size * 3f);
		bool alreadyHitWorm = false;
        foreach (Collider col in allCols) {
			if (isPlayerBomb) {
				if (Vector3.Distance(transform.position, col.transform.position) <= size) { // in damaging radius
					WormController possibleWorm = col.GetComponentInParent<WormController>();
					if (!alreadyHitWorm && possibleWorm != null) {
						possibleWorm.HitByBomb();
						alreadyHitWorm = true;
					} else if (col.tag == "Asteroid") {
						col.GetComponent<AstriodController>().Explode();
					}
				}
			}
			

            Rigidbody rb = col.GetComponentInParent<Rigidbody>();
            if (rb != null && col.tag != "Bomb") {
				Vector3 diff = (rb.transform.position - transform.position);
				float magnitude = falloffCurve.Evaluate(Mathf.Clamp01(diff.magnitude / (size*3)));

                rb.AddForce(explosionForce * diff.normalized * magnitude);
            }
        }
    }

    IEnumerator LightSequence() {
        float p = 0;
        while (p < 1f) {
            explosionLight.range = lightCurve.Evaluate(p) * 10 * size;
            yield return new WaitForEndOfFrame();
            p += Time.deltaTime / 3;
        }

        Destroy(gameObject);
    }
}
