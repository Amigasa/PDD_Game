using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Источники звука")]
    public AudioSource musicSource;
    public AudioSource sfxSource;
    public AudioSource uiSource;

    [Header("Фоновая музыка")]
    public AudioClip mainMenuMusic;
    public AudioClip galleryMusic;
    public AudioClip tutorialMusic;
    public AudioClip battleMapMusic;
    public AudioClip coloringMapMusic;
    public AudioClip battleGameMusic;
    public AudioClip coloringGameMusic;
    public AudioClip victoryMusic;
    public AudioClip defeatMusic;

    [Header("Звуки UI")]
    public AudioClip buttonClick;
    public AudioClip buttonHover;
    public AudioClip panelOpen;
    public AudioClip panelClose;

    [Header("Звуки игры - Битва")]
    public AudioClip battleCorrectAnswer;
    public AudioClip battleWrongAnswer;
    public AudioClip playerAttack;
    public AudioClip enemyAttack;
    public AudioClip playerDamage;
    public AudioClip enemyDamage;
    public AudioClip starAppear;
    public AudioClip levelComplete;

    [Header("Звуки игры - Разукрашка")]
    public AudioClip coloringCorrectAnswer;
    public AudioClip coloringWrongAnswer;
    public AudioClip colorAppear;
    public AudioClip drawingComplete;

    [Header("Настройки")]
    public float musicVolume = 0.7f;
    public float sfxVolume = 1.0f;
    public float uiVolume = 1.0f;

    private Dictionary<string, AudioClip> soundDictionary = new Dictionary<string, AudioClip>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeSoundDictionary();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Загружаем настройки громкости
        LoadAudioSettings();
    }

    void InitializeSoundDictionary()
    {
        // Музыка
        soundDictionary["MainMenuMusic"] = mainMenuMusic;
        soundDictionary["GalleryMusic"] = galleryMusic;
        soundDictionary["TutorialMusic"] = tutorialMusic;
        soundDictionary["BattleMapMusic"] = battleMapMusic;
        soundDictionary["ColoringMapMusic"] = coloringMapMusic;
        soundDictionary["BattleGameMusic"] = battleGameMusic;
        soundDictionary["ColoringGameMusic"] = coloringGameMusic;
        soundDictionary["VictoryMusic"] = victoryMusic;
        soundDictionary["DefeatMusic"] = defeatMusic;

        // UI звуки
        soundDictionary["ButtonClick"] = buttonClick;
        soundDictionary["ButtonHover"] = buttonHover;
        soundDictionary["PanelOpen"] = panelOpen;
        soundDictionary["PanelClose"] = panelClose;

        // Битва
        soundDictionary["BattleCorrectAnswer"] = battleCorrectAnswer;
        soundDictionary["BattleWrongAnswer"] = battleWrongAnswer;
        soundDictionary["PlayerAttack"] = playerAttack;
        soundDictionary["EnemyAttack"] = enemyAttack;
        soundDictionary["PlayerDamage"] = playerDamage;
        soundDictionary["EnemyDamage"] = enemyDamage;
        soundDictionary["StarAppear"] = starAppear;
        soundDictionary["LevelComplete"] = levelComplete;

        // Разукрашка
        soundDictionary["ColoringCorrectAnswer"] = coloringCorrectAnswer;
        soundDictionary["ColoringWrongAnswer"] = coloringWrongAnswer;
        soundDictionary["ColorAppear"] = colorAppear;
        soundDictionary["DrawingComplete"] = drawingComplete;
    }

    // === УПРАВЛЕНИЕ МУЗЫКОЙ ===
    public void PlayMusic(string musicName)
    {
        if (soundDictionary.ContainsKey(musicName) && soundDictionary[musicName] != null)
        {
            musicSource.clip = soundDictionary[musicName];
            musicSource.volume = musicVolume;
            musicSource.loop = true;
            musicSource.Play();
        }
    }

    public void StopMusic()
    {
        musicSource.Stop();
    }

    public void PauseMusic()
    {
        musicSource.Pause();
    }

    public void ResumeMusic()
    {
        musicSource.UnPause();
    }

    public void PlayVictoryMusic()
    {
        PlayOneShotMusic(victoryMusic);
    }

    public void PlayDefeatMusic()
    {
        PlayOneShotMusic(defeatMusic);
    }

    private void PlayOneShotMusic(AudioClip clip)
    {
        if (clip != null)
        {
            musicSource.clip = clip;
            musicSource.volume = musicVolume;
            musicSource.loop = false;
            musicSource.Play();
        }
    }

    // === ЗВУКИ SFX ===
    public void PlaySFX(string soundName)
    {
        if (soundDictionary.ContainsKey(soundName) && soundDictionary[soundName] != null)
        {
            sfxSource.PlayOneShot(soundDictionary[soundName], sfxVolume);
        }
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip != null)
        {
            sfxSource.PlayOneShot(clip, sfxVolume);
        }
    }

    // === ЗВУКИ UI ===
    public void PlayUI(string soundName)
    {
        if (soundDictionary.ContainsKey(soundName) && soundDictionary[soundName] != null)
        {
            uiSource.PlayOneShot(soundDictionary[soundName], uiVolume);
        }
    }

    public void PlayButtonClick()
    {
        PlayUI("ButtonClick");
    }

    public void PlayButtonHover()
    {
        PlayUI("ButtonHover");
    }

    // === НАСТРОЙКИ ГРОМКОСТИ ===
    public void SetMusicVolume(float volume)
    {
        musicVolume = volume;
        musicSource.volume = volume;
        SaveAudioSettings();
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = volume;
        SaveAudioSettings();
    }

    public void SetUIVolume(float volume)
    {
        uiVolume = volume;
        SaveAudioSettings();
    }

    private void SaveAudioSettings()
    {
        PlayerPrefs.SetFloat("MusicVolume", musicVolume);
        PlayerPrefs.SetFloat("SFXVolume", sfxVolume);
        PlayerPrefs.SetFloat("UIVolume", uiVolume);
        PlayerPrefs.Save();
    }

    private void LoadAudioSettings()
    {
        musicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.7f);
        sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1.0f);
        uiVolume = PlayerPrefs.GetFloat("UIVolume", 1.0f);

        musicSource.volume = musicVolume;
    }

    // === БЫСТРЫЕ МЕТОДЫ ДЛЯ КОНКРЕТНЫХ СЦЕН ===
    public void PlayMainMenuMusic() => PlayMusic("MainMenuMusic");
    public void PlayGalleryMusic() => PlayMusic("GalleryMusic");
    public void PlayTutorialMusic() => PlayMusic("TutorialMusic");
    public void PlayBattleMapMusic() => PlayMusic("BattleMapMusic");
    public void PlayColoringMapMusic() => PlayMusic("ColoringMapMusic");
    public void PlayBattleGameMusic() => PlayMusic("BattleGameMusic");
    public void PlayColoringGameMusic() => PlayMusic("ColoringGameMusic");
}
