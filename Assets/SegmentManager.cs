using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SegmentManager : MonoBehaviour {
    public float segmentLength;
    public GameObject segmentPrefab;
    public int segmentCount;
    public Transform followPoint; // the worm's head
    Rigidbody followRb;

    List<Vector3> targetPositions = new List<Vector3>();
    List<Transform> segments = new List<Transform>();
    float timeRatio = 0f;

    bool isActive;

	[Header("Death")]
	public float segmentExplodeDelayMin;
	public float segmentExplodeDelayMax;



	void Start () {
        isActive = true;

        followRb = followPoint.GetComponentInParent<Rigidbody>();

        //initilize segment positions
        Vector3 lastPos = followPoint.position;
        for (int i = 0; i < segmentCount + 1; i++) {
            targetPositions.Add(lastPos);

            if (i < segmentCount) {
                GameObject newSegment = Instantiate(segmentPrefab, lastPos, Quaternion.identity, transform);
                segments.Add(newSegment.transform);
            }

            lastPos -= Vector3.forward * segmentLength;
        }

        for (int i = 0; i < segments.Count; i++) {
            UpdateSegment(i);
        }
    }

    void Update () {
        if (!isActive) {
            return;
        }

        float speed = followRb.velocity.magnitude;
		if (speed <= 0.05f) {
			return;
		}

        float timeStep = (speed / segmentLength) * Time.deltaTime;
        timeRatio += timeStep;
        if (timeRatio >= 1f) {
            TargetReached();
            timeRatio = 0f;
        }

        for (int i = 0; i < segments.Count; i++) {
            UpdateSegment(i);
        }
    }

    void TargetReached () {
        targetPositions.RemoveAt(targetPositions.Count - 1);
        targetPositions.Insert(0, followPoint.position);
    }

    void UpdateSegment (int index) {
        if (index < segmentCount) {
            segments[index].position = Vector3.Lerp(targetPositions[index + 1], targetPositions[index], timeRatio);

            Transform lookTarget;
            if (index == 0) {
                lookTarget = followPoint;
            } else {
                lookTarget = segments[index - 1];
            }
            segments[index].LookAt(lookTarget);
        }
    }

	public void StartDeathSequence () {
		isActive = false;
		StartCoroutine(StartExplosionSequence());
	}

	IEnumerator StartExplosionSequence () {
		for (int i = segmentCount - 1; i >= 0; i--) {
			float timer = 0;
			float targetTime = Random.Range(segmentExplodeDelayMin, segmentExplodeDelayMax);
			while (timer < targetTime) {
				timer += Time.deltaTime;
				yield return new WaitForEndOfFrame();
			}
			segments[i].GetComponent<SegmentController>().StartDeathSequence();
		}
		followPoint.GetComponentInParent<WormController>().GetComponent<SegmentController>().StartDeathSequence();
	}
}
