using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionObject : MonoBehaviour {
    protected InteractionController interactingCon;
    protected bool interacting = false;
    public Transform handParent;

    public Vector2 output = Vector2.zero;

    public void StartInteraction (InteractionController con) {
        interactingCon = con; 
        interacting = true;
    }

    public Transform GetHandParent () {
        return handParent;
    }

    public void EndInteracting () {
        interactingCon = null;
        interacting = false;
    }
}
