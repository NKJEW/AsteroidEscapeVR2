using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SceneFader : MonoBehaviour {
	public static float curFadeAmount { get { return (instance != null) ? instance.fader.color.a : 0; } }

	public static SceneFader instance;
	public Image fader { get; private set; }
    public Text messageText { get; private set; }
    public bool isFadedOut;

	Color fullColor;
	Color zeroColor;

	void Awake() {
		if (instance == null) {
			instance = this;
			DontDestroyOnLoad (this);
		} else {
			Destroy (gameObject);
		}
	}

	public void FadeToScene(int buildIndex, Color color, float fadeTime) {
		GetComponent<Canvas>().worldCamera = Camera.main;
		SetColor(color);
        StartCoroutine (SwitchScenes(buildIndex, fadeTime));
	}

    public void FadeToText(string message, Color color, float fadeTime, float delay) {
        GetComponent<Canvas>().worldCamera = Camera.main;
        SetColor(color);
        StartCoroutine(ShowText(message, fadeTime, delay));
    }

	void Start() {
		fader = gameObject.GetComponentInChildren<Image> ();
        messageText = gameObject.GetComponentInChildren<Text> ();
        messageText.enabled = false;
	}

	public void SetColor(Color color) {
		fullColor = new Color(color.r, color.g, color.b, 1f);
		zeroColor = new Color(color.r, color.g, color.b, 0f);
	}

	IEnumerator SwitchScenes(int buildIndex, float fadeTime) {
        if (!isFadedOut) {
            yield return StartCoroutine(FadeIn(fadeTime));
        }

		AsyncOperation loadingLevel = SceneManager.LoadSceneAsync (buildIndex);
		yield return new WaitUntil (() => loadingLevel.isDone);

		GetComponent<Canvas>().worldCamera = FindObjectOfType<Camera>();
		yield return new WaitForSeconds(0.2f);

		yield return StartCoroutine (FadeOut(fadeTime));
	}

    IEnumerator ShowText(string message, float fadeTime, float delay) {
        yield return new WaitForSeconds(delay);

        if (!isFadedOut) {
            yield return StartCoroutine(FadeIn(fadeTime));
        }

        messageText.text = message;
        messageText.enabled = true;
    }
		
	IEnumerator FadeIn(float fadeTime) {
		fader.raycastTarget = true;
		fader.color = zeroColor;
		float p = 0f;
		float t = Time.fixedUnscaledDeltaTime;

		while(p < 1f) {
			fader.color = Color.Lerp (zeroColor, fullColor, p);
            p += t / fadeTime;
			yield return new WaitForSecondsRealtime (t);
		}
		fader.color = fullColor;

        isFadedOut = true;
	}

	IEnumerator FadeOut(float fadeTime) {
		fader.color = fullColor;
		float p = 0f;
		float t = Time.fixedUnscaledDeltaTime;

		while(p < 1f) {
			fader.color = Color.Lerp (fullColor, zeroColor, p);
            p += t / 0.2f;
			yield return new WaitForSecondsRealtime (t);
		}
		fader.color = zeroColor;
		fader.raycastTarget = false;

        isFadedOut = false;
	}
}
