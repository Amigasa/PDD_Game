using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class LevelSelectionManager : MonoBehaviour
{
    [System.Serializable]
    public class LevelInfo
    {
        public int sceneBuildIndex;
        public Button levelButton;
        public Image buttonImage;
        public TMP_Text levelText;

        [Header("Спрайты кнопки")]
        public Sprite grayButtonSprite;   // Серая (уровень закрыт)
        public Sprite redButtonSprite;    // Красная (открыт, но не пройден)
        public Sprite redPressedSprite;   // Красная нажатая (для анимации)
        public Sprite greenButtonSprite;  // Зелёная (пройден)
    }

    [Header("Level Settings")]
    public LevelInfo[] levels;

    [Header("Звуки")]
    public AudioSource audioSource;
    public AudioClip buttonPressSound;   // Звук нажатия
    public AudioClip buttonGreenSound;   // Звук успеха

    void Start()
    {
        RefreshAllLevels();

        // Проверяем, не вернулись ли мы после прохождения уровня
        int justCompleted = PlayerPrefs.GetInt("JustCompletedLevel", -1);
        if (justCompleted > 0)
        {
            // Запускаем анимацию для только что пройденного уровня
            StartCoroutine(AnimateButtonPress(justCompleted - 1));
            PlayerPrefs.DeleteKey("JustCompletedLevel");
            PlayerPrefs.Save();
        }
    }

    public void RefreshAllLevels()
    {
        for (int i = 0; i < levels.Length; i++)
        {
            UpdateLevelDisplay(i);
        }
    }

    void UpdateLevelDisplay(int levelIndex)
    {
        LevelInfo level = levels[levelIndex];
        int currentSlot = PlayerPrefs.GetInt("CurrentSaveSlot", 0);

        bool completed = PlayerPrefs.GetInt($"Slot_{currentSlot}_Level_{levelIndex + 1}_completed", 0) == 1;
        bool unlocked = IsLevelUnlocked(levelIndex);

        // Текст уровня
        if (level.levelText != null)
            level.levelText.text = (levelIndex + 1).ToString();

        // Спрайт кнопки
        if (level.buttonImage != null)
        {
            if (!unlocked)
                level.buttonImage.sprite = level.grayButtonSprite;
            else if (completed)
                level.buttonImage.sprite = level.greenButtonSprite;
            else
                level.buttonImage.sprite = level.redButtonSprite;
        }

        // Кнопка активна, если уровень открыт
        if (level.levelButton != null)
            level.levelButton.interactable = unlocked;
    }

    // Анимация нажатия (Красная -> Нажатая -> Зелёная) после прохождения уровня
    IEnumerator AnimateButtonPress(int levelIndex)
    {
        LevelInfo level = levels[levelIndex];
        if (level.buttonImage == null) yield break;

        // 1. Ставим красную нажатую
        level.buttonImage.sprite = level.redPressedSprite;
        if (audioSource != null && buttonPressSound != null)
            audioSource.PlayOneShot(buttonPressSound);

        yield return new WaitForSeconds(0.3f);

        // 2. Ставим зелёную
        level.buttonImage.sprite = level.greenButtonSprite;
        if (audioSource != null && buttonGreenSound != null)
            audioSource.PlayOneShot(buttonGreenSound);
    }

    bool IsLevelUnlocked(int levelIndex)
    {
        if (levelIndex == 0) return true; // 1 уровень всегда открыт
        int currentSlot = PlayerPrefs.GetInt("CurrentSaveSlot", 0);
        return PlayerPrefs.GetInt($"Slot_{currentSlot}_Level_{levelIndex}_completed", 0) == 1;
    }

    public void LoadLevel(int levelIndex)
    {
        if (levelIndex < 0 || levelIndex >= levels.Length) return;
        if (!IsLevelUnlocked(levelIndex)) return;

        // Запускаем анимацию и загрузку через корутину
        StartCoroutine(LoadLevelWithAnimation(levelIndex));
    }

    IEnumerator LoadLevelWithAnimation(int levelIndex)
    {
        LevelInfo level = levels[levelIndex];

        // 1. Показываем нажатую кнопку
        if (level.buttonImage != null)
            level.buttonImage.sprite = level.redPressedSprite;

        // 2. Играем звук нажатия (если есть)
        if (audioSource != null && buttonPressSound != null)
            audioSource.PlayOneShot(buttonPressSound);

        // 3. Ждем 0.3 секунды (чтобы анимация была видна)
        yield return new WaitForSeconds(0.3f);

        // 4. Только теперь сохраняем прогресс и грузим сцену
        PlayerPrefs.SetInt("CurrentLevel", levelIndex + 1);
        PlayerPrefs.Save();

        SceneManager.LoadScene(levels[levelIndex].sceneBuildIndex);
    }

    public void BackToMenu()
    {
        SceneManager.LoadScene(0);
    }
}