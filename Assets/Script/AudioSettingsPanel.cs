using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AudioSettingsPanel : MonoBehaviour
{
    [Header("Sliders")]
    public Slider musicSlider;
    public Slider sfxSlider;
    public Slider uiSlider;

    [Header("Texts")]
    public TMP_Text musicText;
    public TMP_Text sfxText;
    public TMP_Text uiText;

    void Start()
    {
        // Загружаем текущие настройки
        musicSlider.value = AudioManager.Instance.musicVolume;
        sfxSlider.value = AudioManager.Instance.sfxVolume;
        uiSlider.value = AudioManager.Instance.uiVolume;

        UpdateTexts();

        // Подписываемся на изменения
        musicSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        sfxSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
        uiSlider.onValueChanged.AddListener(OnUIVolumeChanged);
    }

    void OnMusicVolumeChanged(float value)
    {
        AudioManager.Instance.SetMusicVolume(value);
        musicText.text = $"Музыка: {Mathf.RoundToInt(value * 100)}%";
    }

    void OnSFXVolumeChanged(float value)
    {
        AudioManager.Instance.SetSFXVolume(value);
        sfxText.text = $"Звуки: {Mathf.RoundToInt(value * 100)}%";
    }

    void OnUIVolumeChanged(float value)
    {
        AudioManager.Instance.SetUIVolume(value);
        uiText.text = $"Интерфейс: {Mathf.RoundToInt(value * 100)}%";
    }

    void UpdateTexts()
    {
        musicText.text = $"Музыка: {Mathf.RoundToInt(musicSlider.value * 100)}%";
        sfxText.text = $"Звуки: {Mathf.RoundToInt(sfxSlider.value * 100)}%";
        uiText.text = $"Интерфейс: {Mathf.RoundToInt(uiSlider.value * 100)}%";
    }
}