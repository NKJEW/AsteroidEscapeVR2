using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MessageController : MonoBehaviour {
    public Text messageText;
    string cachedMessage;

    public void ShowMessage(string message, float time) {
        cachedMessage = message;
        CancelInvoke(); //abort all old messages
        Invoke("DisplayText", time);
    }

    void DisplayText() {
        
    }
}
