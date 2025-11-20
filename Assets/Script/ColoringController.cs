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
    public GameObject victoryPanel;
    public GameObject defeatPanel;
    public TMP_Text victoryText;

    [Header("Части рисунка")]
    public GameObject[] correctParts;
    public GameObject[] wrongParts;
    public Image baseDrawing;

    [Header("Настройки")]
    public int totalQuestions = 4;
    public int maxErrors = 2;
    public string levelId = "Coloring_Level_1"; // levelId задается в Inspector

    private int currentQuestion = 0;
    private int correctAnswer;
    private bool isGameActive = true;
    private int errorCount = 0;
    private int correctAnswers = 0;

    void Start()
    {
        // Используем levelId из Inspector, как в BattleLevelController
        if (string.IsNullOrEmpty(levelId))
        {
            levelId = "Coloring_Level_1"; // fallback
        }

        InitializeDrawing();
        GenerateQuestion();

        Debug.Log($"Загружен уровень раскраски: {levelId}");
    }

    void InitializeDrawing()
    {
        foreach (GameObject part in correctParts)
        {
            if (part != null) part.SetActive(false);
        }
        foreach (GameObject part in wrongParts)
        {
            if (part != null) part.SetActive(false);
        }

        if (baseDrawing != null) baseDrawing.gameObject.SetActive(true);
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
            correctAnswers++;
            ShowPart(currentQuestion, true);
        }
        else
        {
            errorCount++;
            ShowPart(currentQuestion, false);
        }

        currentQuestion++;

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

    void ShowPart(int partIndex, bool isCorrect)
    {
        if (partIndex < correctParts.Length && partIndex < wrongParts.Length)
        {
            if (isCorrect)
            {
                if (correctParts[partIndex] != null)
                {
                    correctParts[partIndex].SetActive(true);
                    StartCoroutine(AnimateAppearance(correctParts[partIndex]));
                }
                if (wrongParts[partIndex] != null)
                    wrongParts[partIndex].SetActive(false);
            }
            else
            {
                if (wrongParts[partIndex] != null)
                {
                    wrongParts[partIndex].SetActive(true);
                    StartCoroutine(AnimateAppearance(wrongParts[partIndex]));
                }
                if (correctParts[partIndex] != null)
                    correctParts[partIndex].SetActive(false);
            }
        }
    }

    IEnumerator AnimateAppearance(GameObject part)
    {
        if (part == null) yield break;

        Image partImage = part.GetComponent<Image>();
        if (partImage != null)
        {
            float duration = 0.5f;
            float elapsed = 0f;
            Color originalColor = partImage.color;
            Color transparentColor = new Color(originalColor.r, originalColor.g, originalColor.b, 0f);

            partImage.color = transparentColor;

            while (elapsed < duration)
            {
                partImage.color = Color.Lerp(transparentColor, originalColor, elapsed / duration);
                elapsed += Time.deltaTime;
                yield return null;
            }

            partImage.color = originalColor;
        }
    }

    void Victory(bool isPerfect)
    {
        isGameActive = false;

        // Сохраняем прогресс с levelId из Inspector
        SaveLevelProgress(isPerfect);

        if (isPerfect)
        {
            UnlockGalleryImage();
            victoryText.text = "Идеально! Рисунок добавлен в галерею! 🎨";
        }
        else
        {
            victoryText.text = $"Хорошо! Правильных ответов: {correctAnswers}/{totalQuestions}";
        }

        victoryPanel.SetActive(true);
    }

    void Defeat()
    {
        isGameActive = false;

        // Сохраняем обычный прогресс
        SaveLevelProgress(false);

        TMP_Text defeatText = defeatPanel.GetComponentInChildren<TMP_Text>();
        if (defeatText != null)
        {
            defeatText.text = errorCount >= maxErrors ?
                "Слишком много ошибок!\nПопробуй еще раз!" :
                $"Почти получилось!\nПравильных ответов: {correctAnswers}/{totalQuestions}";
        }

        defeatPanel.SetActive(true);
    }

    // ОБНОВЛЕННЫЙ МЕТОД - использует levelId из Inspector
    void SaveLevelProgress(bool isPerfect)
    {
        int currentSlot = PlayerPrefs.GetInt("CurrentSaveSlot", 0);

        // Сохраняем что уровень пройден
        string completedKey = $"Slot_{currentSlot}_{levelId}_completed";
        PlayerPrefs.SetInt(completedKey, 1);

        // Сохраняем количество звезд
        int stars = isPerfect ? 3 : (correctAnswers >= totalQuestions * 0.7f ? 2 : 1);
        string starsKey = $"Slot_{currentSlot}_{levelId}_stars";
        PlayerPrefs.SetInt(starsKey, stars);

        PlayerPrefs.Save();

        Debug.Log($"Сохранен прогресс: {levelId}, Звезд: {stars}, Идеально: {isPerfect}");
    }

    void UnlockGalleryImage()
    {
        int currentSlot = PlayerPrefs.GetInt("CurrentSaveSlot", 0);

        // Определяем номер уровня из levelId (например "Coloring_Level_1" -> 1)
        int levelNumber = 1;
        if (levelId.Contains("2")) levelNumber = 2;
        else if (levelId.Contains("3")) levelNumber = 3;
        else if (levelId.Contains("4")) levelNumber = 4;

        string galleryKey = $"Slot_{currentSlot}_Gallery_Level_{levelNumber}";
        PlayerPrefs.SetInt(galleryKey, 1);
        PlayerPrefs.Save();

        Debug.Log($"Рисунок уровня {levelNumber} разблокирован в галерее слота {currentSlot}!");
    }

    // Кнопки UI
    public void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void BackToMenu()
    {
        SceneManager.LoadScene(2);
    }

    public void LoadNextLevel()
    {
        // Определяем следующий уровень из levelId
        int currentLevel = 1;
        if (levelId.Contains("2")) currentLevel = 2;
        else if (levelId.Contains("3")) currentLevel = 3;
        else if (levelId.Contains("4")) currentLevel = 4;

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