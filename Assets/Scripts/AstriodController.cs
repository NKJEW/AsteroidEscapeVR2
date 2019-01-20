using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AstriodController : MonoBehaviour {
	public float massFactor;
	public float scaleVariance;
    public int debrisFactor;

    public Material defaultExplosionMat;
    public Color defaultExplosionLightColor;

    public AudioClip explosionSound;

    int size;

    Rigidbody rb;

    public void Init(int newSize) {
        size = newSize;

        transform.rotation = Random.rotation;
        transform.localScale = new Vector3(1 + Random.Range(-scaleVariance, scaleVariance), 1 + Random.Range(-scaleVariance, scaleVariance), 1 + Random.Range(-scaleVariance, scaleVariance));

        Mesh mesh = TerrainGenerator.instance.GetMeshOfSize(size);
        GetComponent<MeshFilter>().mesh = mesh;
        GetComponent<MeshCollider>().sharedMesh = mesh;

        rb = GetComponent<Rigidbody>();
		rb.mass = massFactor * Mathf.Pow(size + 1, 3);
    }

    //temporary behavior
    void OnCollisionEnter(Collision other) {
		if (other.gameObject.CompareTag("Deadly")) {
			Explode(other.relativeVelocity.magnitude);
		}
    }

	public void Explode (float relativeVel = 10f) {
        TerrainGenerator.instance.CreateExplosion(transform.position, 2.5f + size * 2.5f, defaultExplosionMat, defaultExplosionLightColor, explosionSound);
		//newParticles.GetComponent<Rigidbody>().velocity = other.relativeVelocity;
		if (size > 0) { //0 is a fragment
			CreateDebris(relativeVel);
		}

		GetComponent<Grabable>().ObjectDestroyed();
		Destroy(gameObject);
	}

    void CreateDebris(float collisionForce) {
        int numDebris = size * debrisFactor;
        for (int i = 0; i < numDebris; i++) {
            Vector3 spawnOffset = Random.onUnitSphere * size;

            GameObject newDebrisPiece = Instantiate(TerrainGenerator.instance.asteroidPrefab, transform.position + spawnOffset * 1.5f, Quaternion.identity, transform.root);
            // newDebrisPiece.GetComponent<Rigidbody>().velocity = spawnOffset * Random.Range(0.00001f, 0.0001f) * collisionForce;
            newDebrisPiece.GetComponent<AstriodController>().Init(0);
        }
    }
}
