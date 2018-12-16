using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grabable : MonoBehaviour {
	Transform anchor = null;
	bool hasAnchor = false;
	Rigidbody rb;
	GunController attachedCon;

	private void Start () {
		rb = GetComponent<Rigidbody>();
	}

	public Vector3 GetAnchorPos () {
		return anchor.position;
	}

	public void CreateAnchor (Vector3 anchorPos, GunController newCon) {
		if (!hasAnchor) {
			anchor = new GameObject("Anchor").transform;
			anchor.transform.parent = transform;
			hasAnchor = true;
		}
		anchor.position = anchorPos;
		attachedCon = newCon;
	}

	public void AddForce (Vector3 force) {
		if (rb != null) {
			rb.AddForce(force);
		}
	}

	public void DestoryAnchor ()
	{
		if (hasAnchor) {
			Destroy(anchor.gameObject);
			hasAnchor = false;
		}
		attachedCon = null;
	}

	public void AsteroidDestroyed () {
		if (attachedCon != null) {
			attachedCon.Detach();
		}
	}
}
