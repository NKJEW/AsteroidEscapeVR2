using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LinePuffGenerator :MonoBehaviour {
	public List<LinePair> linePairs = new List<LinePair>();

	public GameObject particlePrefab;
	public Material puffMat;

	void Start () {
		Init(6f);
	}

	public void Init (float density) {
		foreach(LinePair pair in linePairs) {
			SpawnLine(pair, density);
		}
	}

	void SpawnLine (LinePair pair, float density) {
		int numParticles = Mathf.RoundToInt(Vector3.Distance(pair.startPos.position, pair.endPos.position) * density);
		for (int i = 0; i < numParticles; i++) {
			GameObject newPart = Instantiate(particlePrefab, Vector3.Lerp(pair.startPos.position, pair.endPos.position, Random.value), Random.rotation);
			newPart.GetComponent<Renderer>().material = puffMat;
			newPart.GetComponent<ExplosionParticle>().Init(Random.Range(0f, 0.1f), Random.Range(0.9f, 1.7f), Random.Range(0.2f, 0.4f), transform.forward, Vector3.zero);
		}
	}

	[System.Serializable]
	public struct LinePair {
		public Transform startPos;
		public Transform endPos;
	}
}
