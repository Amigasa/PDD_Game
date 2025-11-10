using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class LevelSelectionManager : MonoBehaviour
{
    [System.Serializable]
    public class LevelInfo
    {
        public string levelId;
        public Button levelButton;
        public GameObject[] stars;
        public TMP_Text levelText;
    }

    [Header("Level Settings")]
    public LevelInfo[] levels;

    void Start()
    {
        RefreshAllLevels();
    }

    public void RefreshAllLevels()
    {
        // Обновляем отображение всех уровней на основе текущего слота
        for (int i = 0; i < levels.Length; i++)
        {
            UpdateLevelDisplay(i);
        }
    }

    void UpdateLevelDisplay(int levelIndex)
    {
        LevelInfo level = levels[levelIndex];
        LevelProgress progress = SlotSaveSystem.LoadLevelProgress(level.levelId);
        bool isUnlocked = IsLevelUnlocked(levelIndex);

        // Обновляем звезды
        for (int starIndex = 0; starIndex < level.stars.Length; starIndex++)
        {
            if (level.stars[starIndex] != null)
            {
                level.stars[starIndex].SetActive(starIndex < progress.starsEarned);
            }
        }

        // Настраиваем кнопку
        if (level.levelButton != null)
        {
            level.levelButton.interactable = isUnlocked;

            // Визуальная индикация
            Image buttonImage = level.levelButton.GetComponent<Image>();
            if (buttonImage != null)
            {
                if (progress.isCompleted)
                    buttonImage.color = Color.green;
                else if (isUnlocked)
                    buttonImage.color = Color.white;
                else
                    buttonImage.color = Color.gray;
            }
        }

        // Обновляем текст уровня
        if (level.levelText != null)
        {
            level.levelText.text = progress.isCompleted ? "✓" : (levelIndex + 1).ToString();
        }
    }

    bool IsLevelUnlocked(int levelIndex)
    {
        if (levelIndex == 0) return true;

        // Уровень доступен если предыдущий пройден в ТЕКУЩЕМ СЛОТЕ
        string previousLevelId = levels[levelIndex - 1].levelId;
        return SlotSaveSystem.LoadLevelProgress(previousLevelId).isCompleted;
    }

    public void LoadLevel(int levelIndex)
    {
        if (levelIndex < 0 || levelIndex >= levels.Length) return;

        string levelId = levels[levelIndex].levelId;
        Debug.Log($"Загрузка уровня: {levelId} для слота {SlotSaveSystem.GetCurrentSlot()}");

        // Сохраняем текущий уровень для загрузки
        PlayerPrefs.SetString("CurrentLevel", levelId);
        PlayerPrefs.Save();

        // Загружаем сцену битвы
        UnityEngine.SceneManagement.SceneManager.LoadScene("BattleScene");
    }
}
