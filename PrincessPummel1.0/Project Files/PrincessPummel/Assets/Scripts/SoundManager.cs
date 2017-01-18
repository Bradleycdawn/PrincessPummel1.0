using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class SoundManager : MonoBehaviour {

	public AudioSource bgMusic, sfx;
	public static SoundManager AudioInstance;
	public float lowPitch = 0.95f;
	public float highPitch = 1.05f;

    void Awake () {
		if (AudioInstance == null) {
			DontDestroyOnLoad (gameObject);
            if (GameManager.GameInstance == null)
            {
                bgMusic.volume = 0.75f;
                sfx.volume = 0.85f;
            }
            else
            {
                bgMusic.volume = GameManager.GameInstance.musicVolume;
                sfx.volume = GameManager.GameInstance.sfxVolume;
            }
            AudioInstance = this;
        }
		else if (AudioInstance != this) {
			Destroy (gameObject);
		}
	}
	public void PlaySound (AudioClip clip) {
        if (clip == null) return;
        AudioInstance.sfx.Stop();
        AudioInstance.sfx.clip = clip;
        AudioInstance.sfx.Play();
	}

	public void RandomizeSound (params AudioClip[] clips) {
        Debug.Log("Random SFX Plays.");
		int randomIndex = Random.Range (0, clips.Length);
        AudioInstance.sfx.pitch = Random.Range (lowPitch, highPitch);
        AudioInstance.sfx.clip = clips [randomIndex];
        AudioInstance.sfx.Play ();
	}

	public void PlayMusic (AudioClip clip) {
		if (AudioInstance.bgMusic.clip != null) {
            AudioInstance.bgMusic.Stop ();
		}
		if (clip != null) {
            AudioInstance.bgMusic.clip = clip;
            AudioInstance.bgMusic.loop = true;
            AudioInstance.bgMusic.Play ();
		}
	}

	public void StopMusic () {
		if (AudioInstance.bgMusic.clip != null) {
            AudioInstance.bgMusic.Stop ();
		}
	}

	public void MusicVol (float vol) {
        AudioInstance.bgMusic.volume = vol;
	}

	public void SoundVol (float vol) {
        AudioInstance.sfx.volume = vol;
	}
}
