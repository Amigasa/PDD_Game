using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LevelSelectionManager : MonoBehaviour
{
    [System.Serializable]
    public class LevelInfo
    {
        public string levelId;        // "Battle_Level_1", "Battle_Level_2"...
        public Button levelButton;    // Кнопка уровня
        public GameObject[] stars;    // 3 звезды под кнопкой
        public TMP_Text levelText;    // Текст на кнопке (1, 2, 3...)
    }

    [Header("Level Settings")]
    public LevelInfo[] levels;

    [Header("UI References")]
    public Button backButton;

    void Start()
    {
        InitializeLevels();
        SetupButtonListeners();
    }

    void InitializeLevels()
    {
        for (int i = 0; i < levels.Length; i++)
        {
            UpdateLevelDisplay(i);
        }
    }

    void UpdateLevelDisplay(int levelIndex)
    {
        LevelInfo level = levels[levelIndex];
        int starsEarned = PlayerPrefs.GetInt(level.levelId + "_stars", 0);
        bool isCompleted = PlayerPrefs.GetInt(level.levelId + "_completed", 0) == 1;
        bool isUnlocked = IsLevelUnlocked(levelIndex);

        // Обновляем звезды
        for (int starIndex = 0; starIndex < level.stars.Length; starIndex++)
        {
            if (level.stars[starIndex] != null)
            {
                level.stars[starIndex].SetActive(starIndex < starsEarned);
            }
        }

        // Настраиваем кнопку
        if (level.levelButton != null)
        {
            level.levelButton.interactable = isUnlocked;

            // Визуальная индикация пройденных уровней
            Image buttonImage = level.levelButton.GetComponent<Image>();
            if (buttonImage != null)
            {
                if (isCompleted)
                    buttonImage.color = Color.green; // Пройден
                else if (isUnlocked)
                    buttonImage.color = Color.white; // Доступен
                else
                    buttonImage.color = Color.gray;  // Заблокирован
            }
        }
    }

    bool IsLevelUnlocked(int levelIndex)
    {
        // Первый уровень всегда доступен
        if (levelIndex == 0) return true;

        // Уровень доступен если предыдущий пройден
        string previousLevelId = levels[levelIndex - 1].levelId;
        return PlayerPrefs.GetInt(previousLevelId + "_completed", 0) == 1;
    }

    void SetupButtonListeners()
    {
        // Назначаем обработчики для кнопок уровней
        for (int i = 0; i < levels.Length; i++)
        {
            int levelIndex = i; // Важно для замыкания
            if (levels[i].levelButton != null)
            {
                levels[i].levelButton.onClick.RemoveAllListeners();
                levels[i].levelButton.onClick.AddListener(() => LoadLevel(levelIndex));
            }
        }

        // Кнопка "Выйти"
        if (backButton != null)
        {
            backButton.onClick.RemoveAllListeners();
        }
    }

    public void LoadLevel(int levelIndex)
    {
        if (levelIndex < 0 || levelIndex >= levels.Length) return;

        string levelId = levels[levelIndex].levelId;
        Debug.Log($"Загрузка уровня: {levelId}");


        // Сохраняем текущий уровень для загрузки в BattleLevelController
        PlayerPrefs.SetString("CurrentLevel", levelId);
        PlayerPrefs.Save();
    }

    // Обновить отображение (вызвать при возвращении в меню)
    public void RefreshLevels()
    {
        InitializeLevels();
    }
    public void ShowLevelPreview(int levelIndex)
    {
        // Показывать информацию об уровне (лучший результат и т.д.)
        LevelInfo level = levels[levelIndex];
        int stars = PlayerPrefs.GetInt(level.levelId + "_stars", 0);
        Debug.Log($"Уровень {levelIndex + 1}: {stars} звезд");
    }
}
