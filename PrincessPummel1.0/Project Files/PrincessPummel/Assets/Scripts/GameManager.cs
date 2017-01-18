using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour {

    public GameObject pausePanel;
    public GameObject powerUpInfo;
	public static GameManager GameInstance;
	[Range(0.0f, 1.0f)]
	public float musicVolume = 0.75f;
	[Range(0.0f, 1.0f)]
	public float sfxVolume = 0.85f;
	public AudioClip[] MusicList = new AudioClip[8];
    public AudioClip[] SFXList = new AudioClip[6];
    public Dictionary<Song, AudioClip> MusicLibrary = new Dictionary<Song, AudioClip>();
    public Dictionary<sfx, AudioClip> SFXLibrary = new Dictionary<sfx, AudioClip>();
	private Song currentSong = Song.MAIN_MENU;

    public enum Song
    {
        MAIN_MENU,
        TUTORIAL_LEVEL,
        LEVEL_1,
        BOSS_1,
        LEVEL_2,
        BOSS_2,
        BOSS_VICTORY,
        CREDITS
    }

    public enum sfx
    {
        NULL,
        BUTTON_SELECT,
        BUTTON_PRESS,
        NEXT_SCENE,
        NEXT_LEVEL,
        PAUSE
    }

#region PersistentPlayerData
	public int playerHealth = 100, playerPower = 0;
	public double playerStamina = 100;
	[HideInInspector]
	public bool playerEarthPower = false, playerIcePower = false, playerFirePower = false, playerWindPower = false;
#endregion
    
	void Awake() {
        if (GameInstance == null) {
			DontDestroyOnLoad (gameObject);
			GameInstance = this;
            Song s = Song.MAIN_MENU;
            foreach (AudioClip a in MusicList)
            {
                MusicLibrary.Add(s, a);
                s++;
            }
            sfx fx = sfx.NULL;
            foreach (AudioClip a in SFXList)
            {
                SFXLibrary.Add(fx, a);
                fx++;
            }
        }
		else if (GameInstance != this) {
            GameInstance.powerUpInfo = this.powerUpInfo;
            GameInstance.pausePanel = this.pausePanel;
			Destroy (gameObject);
		}
        GameInstance.PlayMusic();
	}

    public void PlayPauseSound()
    {
        AudioClip clip;
        SFXLibrary.TryGetValue(sfx.PAUSE, out clip);
        SoundManager.AudioInstance.PlaySound(clip);
    }

    //Played when coming up to a boss.
    public void PlayBossMusic()
    {
        int currentScene = SceneManager.GetActiveScene().buildIndex;
        AudioClip clip;
        switch (currentScene)
        {
            case (9):
                if (GameInstance.currentSong == Song.BOSS_1) break;
                SoundManager.AudioInstance.StopMusic();
                GameInstance.currentSong = Song.BOSS_1;
                MusicLibrary.TryGetValue(Song.BOSS_1, out clip);
                SoundManager.AudioInstance.PlayMusic(clip);
                break;
            case (13):
                if (GameInstance.currentSong == Song.BOSS_2) break;
                SoundManager.AudioInstance.StopMusic();
                GameInstance.currentSong = Song.BOSS_2;
                MusicLibrary.TryGetValue(Song.BOSS_2, out clip);
                SoundManager.AudioInstance.PlayMusic(clip);
                break;
            default:
                break;
        }
    }

    // Played when boss is defeated.
    public void PlayBossVictoryMusic()
    {
		if (GameInstance.currentSong == Song.BOSS_VICTORY) return;
        GameInstance.currentSong = Song.BOSS_VICTORY;
        AudioClip clip;
        MusicLibrary.TryGetValue(Song.BOSS_VICTORY, out clip);
		SoundManager.AudioInstance.PlayMusic (clip);
    }

    // Played at the start of a new level (skips when it is on the same level, but a new scene has loaded).
    public void PlayMusic ()
    {
        int currentScene = SceneManager.GetActiveScene().buildIndex;
        AudioClip clip;
        switch (currentScene)
        {
            //Main Menu:
            case (0):
                if (GameInstance.currentSong == Song.MAIN_MENU) break;
                GameInstance.currentSong = Song.MAIN_MENU;
                MusicLibrary.TryGetValue(Song.MAIN_MENU, out clip);
                SoundManager.AudioInstance.PlayMusic(clip);
                break;
            //Tutorial Level:
            case (1):
            case (2):
            case (3):
            case (4):
            case (5):
                if (GameInstance.currentSong == Song.TUTORIAL_LEVEL) break;
                GameInstance.currentSong = Song.TUTORIAL_LEVEL;
                MusicLibrary.TryGetValue(Song.TUTORIAL_LEVEL, out clip);
                SoundManager.AudioInstance.PlayMusic(clip);
                break;
            //Level 1:
            case (6):
            case (7):
			case (8):
                if (GameInstance.currentSong == Song.LEVEL_1) break;
                GameInstance.currentSong = Song.LEVEL_1;
                MusicLibrary.TryGetValue(Song.LEVEL_1, out clip);
                SoundManager.AudioInstance.PlayMusic(clip);
                break;
            //Boss 1:
            case (9):
                if (GameInstance.currentSong == Song.BOSS_1) break;
                SoundManager.AudioInstance.StopMusic();
                break;
            //Level 2:
            case (10):
			case (11):
			case (12):
                if (GameInstance.currentSong == Song.LEVEL_2) break;
                GameInstance.currentSong = Song.LEVEL_2;
                MusicLibrary.TryGetValue(Song.LEVEL_2, out clip);
                SoundManager.AudioInstance.PlayMusic(clip);
                break;
            //Boss 2:
            case (13):
                if (GameInstance.currentSong == Song.BOSS_2) break;
                GameInstance.PlayBossMusic();
                break;
            //End Screen/Credits:
            case (14):
                if (GameInstance.currentSong == Song.CREDITS) break;
                GameInstance.currentSong = Song.CREDITS;
                MusicLibrary.TryGetValue(Song.CREDITS, out clip);
                SoundManager.AudioInstance.PlayMusic(clip);
                break;
            //Music for other scenes not listed here:
            default:
                if (GameInstance.currentSong == Song.MAIN_MENU) break;
                GameInstance.currentSong = Song.MAIN_MENU;
                MusicLibrary.TryGetValue(Song.MAIN_MENU, out clip);
                SoundManager.AudioInstance.PlayMusic(clip);
                break;
        }
    }

    public void RestartLevel() {
        AudioClip clip;
        SFXLibrary.TryGetValue(sfx.BUTTON_PRESS, out clip);
        SoundManager.AudioInstance.PlaySound(clip);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void ExitLevel() {
        AudioClip clip;
        SFXLibrary.TryGetValue(sfx.BUTTON_PRESS, out clip);
        SoundManager.AudioInstance.PlaySound(clip);
        SceneManager.LoadScene("MainMenu");
        SoundManager.AudioInstance.MusicVol(musicVolume);
    }

    public void Play()
    {
        playerFirePower = false;
        playerEarthPower = false;
        playerIcePower = false;
        playerWindPower = false;
        playerPower = 0;
        AudioClip clip;
        SFXLibrary.TryGetValue(sfx.BUTTON_PRESS, out clip);
        SoundManager.AudioInstance.PlaySound(clip);
        SceneManager.LoadScene(1);
    }

    public void Resume()
    {
        AudioClip clip;
        SFXLibrary.TryGetValue(sfx.BUTTON_PRESS, out clip);
        SoundManager.AudioInstance.PlaySound(clip);
        Time.timeScale = 1;
        GameInstance.pausePanel.SetActive(false);
		SoundManager.AudioInstance.MusicVol (musicVolume);
    }

    public void Continue()
    {
        AudioClip clip;
        SFXLibrary.TryGetValue(sfx.BUTTON_PRESS, out clip);
        SoundManager.AudioInstance.PlaySound(clip);
        Time.timeScale = 1;
        GameInstance.powerUpInfo.SetActive(false);
    }

    public void ExitGame()
    {
        AudioClip clip;
        SFXLibrary.TryGetValue(sfx.BUTTON_PRESS, out clip);
        SoundManager.AudioInstance.PlaySound(clip);
        Application.Quit();
    }

}
