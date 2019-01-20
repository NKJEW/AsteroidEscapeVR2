using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LinePuffGenerator : MonoBehaviour {
    public Transform startPos;
    public Transform endPos;

    public GameObject particlePrefab;
    public Material puffMat;

    void Start() {
        Init(6f);
    }

    public void Init(float density) {
        int numParticles = Mathf.RoundToInt(Vector3.Distance(startPos.position, endPos.position) * density);
        for (int i = 0; i < numParticles; i++) {
            GameObject newPart = Instantiate(particlePrefab, Vector3.Lerp(startPos.position, endPos.position, Random.value), Random.rotation);
            newPart.GetComponent<Renderer>().material = puffMat;
            newPart.GetComponent<ExplosionParticle>().Init(Random.Range(0f, 0.1f), Random.Range(0.9f, 1.7f), Random.Range(0.2f, 0.4f), transform.forward, Vector3.zero);
        }
    }
}
