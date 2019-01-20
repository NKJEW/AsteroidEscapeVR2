using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionParticle : MonoBehaviour {
    public AnimationCurve curve;

    public void Init(float delay, float time, float maxSize, Vector3 diffToCenter) {
        Vector3 axisOfRotation = Vector3.Cross(diffToCenter, Vector3.up).normalized;
        StartCoroutine(ExplodeSequence(delay, time, maxSize, axisOfRotation));
    }

    IEnumerator ExplodeSequence(float delay, float time, float maxSize, Vector3 axisOfRotation) {
        float rotSpeed = (2 * Random.Range(0, 2) - 1) * 50f; //either -50 or 50
        transform.localScale = Vector3.zero;
        yield return new WaitForSeconds(delay);

        float p = 0f;
        while (p < 1f) {
            transform.localScale = Vector3.one * curve.Evaluate(p) * maxSize;
            transform.Rotate(axisOfRotation, rotSpeed * Time.deltaTime);
            yield return new WaitForEndOfFrame();
            p += Time.deltaTime / time;
        }

        Destroy(gameObject);
    }
}
