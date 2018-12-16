using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SceneFader : MonoBehaviour {
	public static float curFadeAmount { get { return (instance != null) ? instance.fader.color.a : 0; } }

	public static SceneFader instance;
	public Image fader { get; private set; }

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
		SetColor (color);
        StartCoroutine (SwitchScenes(buildIndex, fadeTime));
	}
		
	void Start() {
		fader = gameObject.GetComponentInChildren<Image> ();
	}

	public void SetColor(Color color) {
		fullColor = new Color(color.r, color.g, color.b, 1f);
		zeroColor = new Color(color.r, color.g, color.b, 0f);
	}

	public IEnumerator SwitchScenes(int buildIndex, float fadeTime) {
        yield return StartCoroutine (FadeIn(fadeTime));

		AsyncOperation loadingLevel = SceneManager.LoadSceneAsync (buildIndex);
		yield return new WaitUntil (() => loadingLevel.isDone);

        yield return StartCoroutine (FadeOut(fadeTime));
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
	}

	IEnumerator FadeOut(float fadeTime) {
		fader.color = fullColor;
		float p = 0f;
		float t = Time.fixedUnscaledDeltaTime;

		while(p < 1f) {
			fader.color = Color.Lerp (fullColor, zeroColor, p);
            p += t / fadeTime;
			yield return new WaitForSecondsRealtime (t);
		}
		fader.color = zeroColor;
		fader.raycastTarget = false;
	}
}
