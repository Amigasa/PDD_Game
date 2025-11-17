using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class ColoringController : MonoBehaviour
{
    [Header("Основные элементы")]
    public TMP_Text questionText;
    public Button[] answerButtons;
    public Image[] imageParts;
    public GameObject victoryPanel;
    public GameObject defeatPanel;
    public TMP_Text victoryText;

    [Header("Настройки")]
    public int totalQuestions = 4;
    public int maxErrors = 2;

    private int currentQuestion = 0;
    private int correctAnswer;
    private bool isGameActive = true;
    private int errorCount = 0;

    void Start()
    {
        GenerateQuestion();
    }

    void GenerateQuestion()
    {
        if (!isGameActive) return;

        int number1 = Random.Range(2, 10);
        int number2 = Random.Range(2, 10);
        correctAnswer = number1 * number2;

        questionText.text = $"{number1} × {number2} = ?";

        int[] answers = new int[4];
        int correctIndex = Random.Range(0, 4);

        for (int i = 0; i < 4; i++)
        {
            if (i == correctIndex)
            {
                answers[i] = correctAnswer;
            }
            else
            {
                int wrongAnswer;
                do
                {
                    wrongAnswer = correctAnswer + Random.Range(-5, 6);
                } while (wrongAnswer == correctAnswer || wrongAnswer <= 0);

                answers[i] = wrongAnswer;
            }

            answerButtons[i].GetComponentInChildren<TMP_Text>().text = answers[i].ToString();

            int index = i;
            answerButtons[i].onClick.RemoveAllListeners();
            answerButtons[i].onClick.AddListener(() => OnAnswerSelected(index, answers[index]));
        }
    }

    public void OnAnswerSelected(int buttonIndex, int selectedAnswer)
    {
        if (!isGameActive) return;

        if (selectedAnswer == correctAnswer)
        {
            ColorImagePart(currentQuestion, true);
            currentQuestion++;
        }
        else
        {
            ColorImagePart(currentQuestion, false);
            errorCount++;
            currentQuestion++;
        }

        if (errorCount >= maxErrors)
        {
            Defeat();
        }
        else if (currentQuestion >= totalQuestions)
        {
            if (errorCount == 0)
            {
                Victory(true);
            }
            else
            {
                Victory(false);
            }
        }
        else
        {
            GenerateQuestion();
        }
    }

    void ColorImagePart(int partIndex, bool isCorrect)
    {
        if (partIndex < imageParts.Length && imageParts[partIndex] != null)
        {
            Color color = isCorrect ? GetGoodColor(partIndex) : GetBadColor();
            StartCoroutine(AnimateColorChange(imageParts[partIndex], color));
        }
    }

    IEnumerator AnimateColorChange(Image imagePart, Color targetColor)
    {
        Color startColor = imagePart.color;
        float duration = 0.5f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            imagePart.color = Color.Lerp(startColor, targetColor, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        imagePart.color = targetColor;
    }

    Color GetGoodColor(int partIndex)
    {
        Color[] goodColors = {
            new Color(0.4f, 0.7f, 1f),
            new Color(0.3f, 0.8f, 0.3f),
            new Color(1f, 0.8f, 0.3f),
            new Color(0.9f, 0.6f, 0.3f)
        };

        return partIndex < goodColors.Length ? goodColors[partIndex] : Color.white;
    }

    Color GetBadColor()
    {
        return new Color(
            Random.Range(0.8f, 1f),
            Random.Range(0f, 0.2f),
            Random.Range(0.8f, 1f)
        );
    }

    void Victory(bool isPerfect)
    {
        isGameActive = false;

        int levelNumber = GetCurrentLevelNumber();

        // Используем SimpleSlotManager для сохранения
        if (isPerfect)
        {
            // Идеальное прохождение - 3 звезды и разблокировка галереи
            SaveLevelProgress(levelNumber, 3);
            UnlockGalleryImage(levelNumber);
            victoryText.text = "Идеально! Рисунок добавлен в галерею! 🎨";
        }
        else
        {
            // Обычное прохождение - 1 звезда
            SaveLevelProgress(levelNumber, 1);
            victoryText.text = $"Хорошо! Но есть {errorCount} ошибок.\nПопробуй без ошибок для галереи!";
        }

        victoryPanel.SetActive(true);
    }

    void Defeat()
    {
        isGameActive = false;

        int levelNumber = GetCurrentLevelNumber();

        // Сохраняем пройденным но с 0 звезд
        SaveLevelProgress(levelNumber, 0);

        TMP_Text defeatText = defeatPanel.GetComponentInChildren<TMP_Text>();
        if (defeatText != null)
        {
            defeatText.text = errorCount >= maxErrors ?
                "Слишком много ошибок!\nПопробуй еще раз!" :
                "Почти получилось!\nПопробуй еще раз!";
        }

        defeatPanel.SetActive(true);
    }

    int GetCurrentLevelNumber()
    {
        string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

        if (sceneName.Contains("1")) return 1;
        if (sceneName.Contains("2")) return 2;
        if (sceneName.Contains("3")) return 3;
        if (sceneName.Contains("4")) return 4;

        return 1;
    }

    void SaveLevelProgress(int levelNumber, int stars)
    {
        int currentSlot = PlayerPrefs.GetInt("CurrentSaveSlot", 0);

        // Сохраняем прогресс уровня
        PlayerPrefs.SetInt($"Slot_{currentSlot}_Coloring_Level_{levelNumber}_stars", stars);
        PlayerPrefs.SetInt($"Slot_{currentSlot}_Coloring_Level_{levelNumber}_completed", 1);
        PlayerPrefs.Save();

        Debug.Log($"Сохранен прогресс: Раскраска Уровень {levelNumber}, Звезд: {stars}");
    }

    void UnlockGalleryImage(int levelNumber)
    {
        int currentSlot = PlayerPrefs.GetInt("CurrentSaveSlot", 0);

        // Разблокируем рисунок в галерее
        PlayerPrefs.SetInt($"Slot_{currentSlot}_Gallery_Level_{levelNumber}", 1);
        PlayerPrefs.Save();

        Debug.Log($"Рисунок уровня {levelNumber} разблокирован в галерее слота {currentSlot}!");
    }

    // Кнопки UI
    public void RestartLevel()
    {
        SceneManager.LoadScene(
            SceneManager.GetActiveScene().buildIndex
        );
    }

    public void BackToMenu()
    {
        SceneManager.LoadScene(2);
    }

    public void LoadNextLevel()
    {
        int currentLevel = GetCurrentLevelNumber();
        int nextLevel = currentLevel + 1;

        if (nextLevel <= 4)
        {
            string nextSceneName = $"ColoringLevel{nextLevel}";
            if (Application.CanStreamedLevelBeLoaded(nextSceneName))
            {
                SceneManager.LoadScene(nextSceneName);
            }
            else
            {
                BackToMenu();
            }
        }
        else
        {
            BackToMenu();
        }
    }
}