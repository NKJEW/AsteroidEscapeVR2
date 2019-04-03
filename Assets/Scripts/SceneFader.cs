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

	public void Fade(float fadeTime, float delay, bool reloadsLevel) {
		GetComponent<Canvas>().worldCamera = Camera.main;
        SetColor(Color.black);
        if (reloadsLevel) {
            StartCoroutine(SwitchScenes(1, fadeTime, delay));
        } else {
            StartCoroutine(FadeIn(fadeTime, delay));
        }
	}

    public void FadeWithText(string message, float textFadeTime, float textFadeDelay, float screenFadeTime, float screenFadeDelay, bool reloadsLevel) {
        Fade(screenFadeTime, screenFadeDelay, reloadsLevel);
        StartCoroutine(ShowText(message, textFadeTime, textFadeDelay));
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

	IEnumerator SwitchScenes(int buildIndex, float fadeTime, float delay) {
        if (!isFadedOut) {
            yield return StartCoroutine(FadeIn(fadeTime, delay));
        }

		AsyncOperation loadingLevel = SceneManager.LoadSceneAsync (buildIndex);
		yield return new WaitUntil(() => loadingLevel.isDone);
        GetComponent<Canvas>().worldCamera = Camera.main;
		yield return new WaitForSeconds(0.2f);
		messageText.enabled = false;
		messageText.text = "";

		yield return StartCoroutine(FadeOut(fadeTime));
	}

    IEnumerator ShowText(string message, float fadeTime, float delay) {
        yield return new WaitForSeconds(delay);

        messageText.text = message;
        messageText.enabled = true;
        messageText.color = Color.clear;

        float p = 0f;
        float t = Time.fixedUnscaledDeltaTime;

        while (p < 1f) {
            messageText.color = Color.Lerp(Color.clear, Color.white, p);
            p += t / fadeTime;
            yield return new WaitForSecondsRealtime(t);
        }

        messageText.color = Color.white;
    }
		
	IEnumerator FadeIn(float fadeTime, float delay = 0f) {
        yield return new WaitForSeconds(delay);

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
