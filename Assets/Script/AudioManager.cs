using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

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
    private string currentMusic = "";
    private bool audioInitialized = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeAudioSystem();
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

        // Автоматически включаем музыку для текущей сцены
        PlayMusicForScene(SceneManager.GetActiveScene().name);

        // Следим за сменой сцен
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void InitializeAudioSystem()
    {
        // Создаем AudioSource если их нет
        if (musicSource == null)
            musicSource = gameObject.AddComponent<AudioSource>();
        if (sfxSource == null)
            sfxSource = gameObject.AddComponent<AudioSource>();
        if (uiSource == null)
            uiSource = gameObject.AddComponent<AudioSource>();

        // Настраиваем AudioSource для мобильных устройств
        ConfigureAudioSource(musicSource, true, "Music");
        ConfigureAudioSource(sfxSource, false, "SFX");
        ConfigureAudioSource(uiSource, false, "UI");

        // Проверяем поддержку аудио
        if (!SystemInfo.supportsAudio)
        {
            Debug.LogWarning("AudioManager: Audio is not supported on this device!");
        }
        else
        {
            Debug.Log("AudioManager: Audio system initialized successfully");
            audioInitialized = true;
        }
    }

    void ConfigureAudioSource(AudioSource source, bool isMusic, string sourceName)
    {
        source.playOnAwake = false;
        source.spatialBlend = 0f; // 2D звук
        source.loop = isMusic;

        // Настройки для мобильной оптимизации
        source.bypassEffects = true;
        source.bypassListenerEffects = true;
        source.bypassReverbZones = true;

        if (isMusic)
        {
            source.volume = musicVolume;
            source.priority = 0; // Высокий приоритет для музыки
        }
        else
        {
            source.volume = sfxVolume;
            source.priority = 128; // Средний приоритет для SFX
        }

        Debug.Log($"AudioManager: Configured {sourceName} source - Loop: {isMusic}, Volume: {source.volume}");
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

        Debug.Log($"AudioManager: Sound dictionary initialized with {soundDictionary.Count} sounds");
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"AudioManager: Scene changed to {scene.name}");
        PlayMusicForScene(scene.name);
    }

    public void PlayMusicForScene(string sceneName)
    {
        if (!audioInitialized) return;

        string requiredMusic = GetMusicForScene(sceneName);

        // Если уже играет нужная музыка - не переключаем
        if (currentMusic == requiredMusic && musicSource.isPlaying)
        {
            Debug.Log($"AudioManager: Music already playing ({currentMusic})");
            return;
        }

        Debug.Log($"AudioManager: Switching music {currentMusic} -> {requiredMusic}");
        currentMusic = requiredMusic;

        switch (sceneName)
        {
            case "MainMenu":
                PlayMainMenuMusic();
                break;
            case "Gallery":
                PlayGalleryMusic();
                break;
            case "Tutorial":
                PlayTutorialMusic();
                break;
            case "Man":
                PlayBattleMapMusic();
                break;
            case "Woman":
                PlayColoringMapMusic();
                break;
            case "BattleLevel1":
            case "BattleLevel2":
            case "BattleLevel3":
            case "BattleLevel4":
            case "BattleLevel5":
            case "BattleLevel6":
            case "BattleLevel7":
            case "BattleLevel8":
                PlayBattleGameMusic();
                break;
            case "ColoringLevel1":
            case "ColoringLevel2":
            case "ColoringLevel3":
            case "ColoringLevel4":
                PlayColoringGameMusic();
                break;
            default:
                // Для любых других сцен используем музыку главного меню
                PlayMainMenuMusic();
                break;
        }
    }

    string GetMusicForScene(string sceneName)
    {
        switch (sceneName)
        {
            case "MainMenu": return "MainMenu";
            case "Gallery": return "Gallery";
            case "Tutorial": return "Tutorial";
            case "Man": return "BattleMap";
            case "Woman": return "ColoringMap";
            case "BattleLevel1":
            case "BattleLevel2":
            case "BattleLevel3":
            case "BattleLevel4":
            case "BattleLevel5":
            case "BattleLevel6":
            case "BattleLevel7":
            case "BattleLevel8":
                return "BattleGame";
            case "ColoringLevel1":
            case "ColoringLevel2":
            case "ColoringLevel3":
            case "ColoringLevel4":
                return "ColoringGame";
            default: return "MainMenu";
        }
    }

    // === УПРАВЛЕНИЕ МУЗЫКОЙ ===
    public void PlayMainMenuMusic()
    {
        PlayMusic(mainMenuMusic, "Main Menu");
    }

    public void PlayGalleryMusic()
    {
        PlayMusic(galleryMusic, "Gallery");
    }

    public void PlayTutorialMusic()
    {
        PlayMusic(tutorialMusic, "Tutorial");
    }

    public void PlayBattleMapMusic()
    {
        PlayMusic(battleMapMusic, "Battle Map");
    }

    public void PlayColoringMapMusic()
    {
        PlayMusic(coloringMapMusic, "Coloring Map");
    }

    public void PlayBattleGameMusic()
    {
        PlayMusic(battleGameMusic, "Battle Game");
    }

    public void PlayColoringGameMusic()
    {
        PlayMusic(coloringGameMusic, "Coloring Game");
    }

    public void PlayVictoryMusic()
    {
        PlayMusic(victoryMusic, "Victory", false);
    }

    public void PlayDefeatMusic()
    {
        PlayMusic(defeatMusic, "Defeat", false);
    }

    private void PlayMusic(AudioClip clip, string musicName, bool loop = true)
    {
        if (!audioInitialized || clip == null) return;

        musicSource.clip = clip;
        musicSource.volume = musicVolume;
        musicSource.loop = loop;
        musicSource.Play();

        Debug.Log($"AudioManager: Playing {musicName} music");
    }

    public void StopMusic()
    {
        if (musicSource.isPlaying)
        {
            musicSource.Stop();
            Debug.Log("AudioManager: Music stopped");
        }
    }

    // === ЗВУКИ SFX ===
    public void PlaySFX(string soundName)
    {
        if (!audioInitialized) return;

        if (soundDictionary.ContainsKey(soundName) && soundDictionary[soundName] != null)
        {
            if (sfxSource != null && sfxSource.isActiveAndEnabled)
            {
                sfxSource.PlayOneShot(soundDictionary[soundName], sfxVolume);
                Debug.Log($"AudioManager: Playing SFX - {soundName}");
            }
        }
        else
        {
            Debug.LogWarning($"AudioManager: Sound '{soundName}' not found in dictionary!");
        }
    }

    public void PlaySFX(AudioClip clip)
    {
        if (!audioInitialized || clip == null) return;

        if (sfxSource != null && sfxSource.isActiveAndEnabled)
        {
            sfxSource.PlayOneShot(clip, sfxVolume);
        }
    }

    // === СПЕЦИАЛЬНЫЕ МЕТОДЫ ДЛЯ ИГРЫ ===
    public void PlayBattleCorrectAnswer()
    {
        PlaySFX("BattleCorrectAnswer");
    }

    public void PlayBattleWrongAnswer()
    {
        PlaySFX("BattleWrongAnswer");
    }

    public void PlayColoringCorrectAnswer()
    {
        PlaySFX("ColoringCorrectAnswer");
    }

    public void PlayColoringWrongAnswer()
    {
        PlaySFX("ColoringWrongAnswer");
    }

    public void PlayVictorySound()
    {
        PlaySFX("LevelComplete");
    }

    public void PlayDefeatSound()
    {
        PlaySFX("PanelClose");
    }

    public void PlayButtonClickSound()
    {
        PlayUI("ButtonClick");
    }

    public void PlayColorAppearSound()
    {
        PlaySFX("ColorAppear");
    }

    public void PlayStarAppearSound()
    {
        PlaySFX("StarAppear");
    }

    // === ЗВУКИ UI ===
    public void PlayUI(string soundName)
    {
        if (!audioInitialized) return;

        if (soundDictionary.ContainsKey(soundName) && soundDictionary[soundName] != null)
        {
            if (uiSource != null && uiSource.isActiveAndEnabled)
            {
                uiSource.PlayOneShot(soundDictionary[soundName], uiVolume);
            }
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
        musicVolume = Mathf.Clamp01(volume);
        if (musicSource != null)
            musicSource.volume = musicVolume;
        SaveAudioSettings();
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        SaveAudioSettings();
    }

    public void SetUIVolume(float volume)
    {
        uiVolume = Mathf.Clamp01(volume);
        SaveAudioSettings();
    }

    private void SaveAudioSettings()
    {
        PlayerPrefs.SetFloat("MusicVolume", musicVolume);
        PlayerPrefs.SetFloat("SFXVolume", sfxVolume);
        PlayerPrefs.SetFloat("UIVolume", uiVolume);
        PlayerPrefs.Save();

        Debug.Log($"AudioManager: Settings saved - Music: {musicVolume}, SFX: {sfxVolume}, UI: {uiVolume}");
    }

    private void LoadAudioSettings()
    {
        musicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.7f);
        sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1.0f);
        uiVolume = PlayerPrefs.GetFloat("UIVolume", 1.0f);

        if (musicSource != null)
            musicSource.volume = musicVolume;

        Debug.Log($"AudioManager: Settings loaded - Music: {musicVolume}, SFX: {sfxVolume}, UI: {uiVolume}");
    }

    // === МЕТОДЫ ДЛЯ ТЕСТИРОВАНИЯ ===
    public void TestAllSounds()
    {
        if (!audioInitialized) return;

        Debug.Log("AudioManager: Testing all sounds...");
        StartCoroutine(TestSoundSequence());
    }

    private IEnumerator TestSoundSequence()
    {
        PlayBattleCorrectAnswer();
        yield return new WaitForSeconds(0.3f);
        PlayBattleWrongAnswer();
        yield return new WaitForSeconds(0.3f);
        PlayColoringCorrectAnswer();
        yield return new WaitForSeconds(0.3f);
        PlayColoringWrongAnswer();
        yield return new WaitForSeconds(0.3f);
        PlayButtonClickSound();
        yield return new WaitForSeconds(0.3f);
        PlayColorAppearSound();
        yield return new WaitForSeconds(0.3f);
        PlayStarAppearSound();

        Debug.Log("AudioManager: Sound test completed");
    }

    // === СТАТУС АУДИО СИСТЕМЫ ===
    public bool IsAudioSupported()
    {
        return audioInitialized;
    }

    public string GetAudioStatus()
    {
        return audioInitialized ? "Audio system is working" : "Audio system is not supported";
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}