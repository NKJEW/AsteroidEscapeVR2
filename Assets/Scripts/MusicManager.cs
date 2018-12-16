using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour {
    public static MusicManager instance;

    public AudioClip[] clips;
    AudioSource[] tracks;
    AudioState[] states;

    int numActiveTracks;
    public float maxTrackVolume;

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
        UpdateIntensity(0, 0);
    }

    void InitializeTracks() {
        tracks = new AudioSource[clips.Length];
        states = new AudioState[clips.Length];
        for (int i = 0; i < clips.Length; i++) {
            tracks[i] = gameObject.AddComponent<AudioSource>();
            tracks[i].clip = clips[i];
            tracks[i].loop = true;
            tracks[i].Play();

            states[i] = new AudioState();
        }
    }

	public void UpdateIntensity(float intensity, float time) {
        numActiveTracks = Mathf.FloorToInt(intensity * clips.Length);
        print(intensity);
        for (int i = 1; i < clips.Length; i++) {
            if (numActiveTracks >= i) {
                UpdateAudioState(i, 1, time);
            } else {
                UpdateAudioState(i, 0, time);
            }
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
