using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlamingAsteroid : MonoBehaviour {
    public GameObject explosionPart;
    public Material redMat;
    public Material smokeMat;

    float nextSpawn;

    void LateUpdate() {
        if (Time.time > nextSpawn) {
            nextSpawn = Time.time + 0.05f;

            //GameObject redPart = CreateNewPart(0f);
            //redPart.GetComponent<Renderer>().material = redMat;

            GameObject smokePart = CreateNewPart(0);
            smokePart.GetComponent<Renderer>().material = smokeMat;
        }
    }

    GameObject CreateNewPart(float delay) {
        Vector3 offset = Random.insideUnitSphere * 0.3f;
        GameObject newPart = Instantiate(explosionPart, transform.position + offset + (transform.right * 3), Random.rotation);
        newPart.GetComponent<ExplosionParticle>().Init(delay, 1f, Random.Range(1f, 1.25f), offset, Vector3.zero);
        return newPart;
    }
}
