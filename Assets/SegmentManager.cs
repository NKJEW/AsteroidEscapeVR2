using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SegmentManager : MonoBehaviour {
    public float segmentLength;
    public GameObject segmentPrefab;
    public int segmentCount;
    public Transform followPoint; // the worm's head
    Rigidbody followRb;

    public List<Vector3> targetPositions = new List<Vector3>();
    public List<Transform> segments = new List<Transform>();
    public float timeRatio = 0f;



	void Start () {
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
        float speed = followRb.velocity.magnitude;
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
        print("reached");
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
}
