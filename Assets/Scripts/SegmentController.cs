using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SegmentController : MonoBehaviour {
    public float minMagnitude;
    public float maxMagnitude;
    public int minOscillations;
    public int maxOscillations;

    public void StartDeathSequence(float time) {
        StartCoroutine(DeathSequence(time));
    }

    IEnumerator DeathSequence(float time) {
        float magnitude = Random.Range(minMagnitude, maxMagnitude);
        int numOscillations = Random.Range(3, 5);
        Vector3 offsets = Random.insideUnitSphere;
        Vector3 initialPos = transform.position;

        float t = 0f;
        while (t < time) {
            transform.position = initialPos + new Vector3(
                CalculateValueOnAxis(t, magnitude, offsets.x, time, numOscillations),
                CalculateValueOnAxis(t, magnitude, offsets.y, time, numOscillations),
                CalculateValueOnAxis(t, magnitude, offsets.z, time, numOscillations)
            );
            yield return new WaitForEndOfFrame();
            t += Time.deltaTime;
        }
    }

    float CalculateValueOnAxis(float value, float magnitude, float offset, float time, int numOscillations) {
        return (magnitude * value / time) * Mathf.Sin(2 * Mathf.PI * ((numOscillations * value / time) - offset));
    }
}
