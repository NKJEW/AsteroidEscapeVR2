using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour {
    public static MusicManager instance;

    public AudioClip[] clips;
    AudioSource[] tracks;
    AudioState[] states;

    public float maxTrackVolume;

    float temp;

    struct AudioState {
        public int trackId;
        public float initialVolume;
        public float goalVolume;
        public float time;
        public float timer;
        public bool active;
    }

    void Awake() {
        instance = this;
    }

    void Start() {
        InitializeTracks();
        tracks[0].volume = maxTrackVolume; //hack to make the base track start
    }

    void InitializeTracks() {
        tracks = new AudioSource[clips.Length];
        states = new AudioState[clips.Length];
        for (int i = 0; i < clips.Length; i++) {
            tracks[i] = gameObject.AddComponent<AudioSource>();
            tracks[i].clip = clips[i];
            tracks[i].loop = true;
            tracks[i].Play();
			tracks[i].volume = 0;
			tracks[i].priority = 0;

			states[i] = new AudioState();
        }
    }

	public void UpdateIntensity(float intensity, float time) {
        for (int i = 1; i < clips.Length; i++) {
            UpdateAudioState(i, Mathf.Clamp01(intensity * clips.Length - i), time);
        }
    }

	public void UpdatePitch (float pitch) {
		for (int i = 1; i < clips.Length; i++) {
			tracks[i].pitch = pitch;
		}
	}

	void LateUpdate() {
        for (int i = 0; i < clips.Length; i++) {
            if (states[i].active) {
                UpdateTrack(i);
            }
        }
    }

    void UpdateTrack(int id) {
        tracks[id].volume = Mathf.Lerp(states[id].initialVolume, states[id].goalVolume * maxTrackVolume, states[id].timer);
        states[id].timer += Time.deltaTime;
        if (states[id].timer >= 1f) {
            states[id].active = false;
        }
    }

    void UpdateAudioState(int trackId, float goalVolume, float time) {
        AudioState newAudioState = new AudioState();
        newAudioState.trackId = trackId;
        newAudioState.initialVolume = tracks[trackId].volume;
        newAudioState.goalVolume = goalVolume;
        newAudioState.time = time;
        newAudioState.timer = 0;
        newAudioState.active = true;
        states[trackId] = newAudioState;
    }
}
