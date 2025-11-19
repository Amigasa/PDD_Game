using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class ColoringController : MonoBehaviour
{
    [Header("Основные элементы")]
    public TMP_Text questionText;
    public Button[] answerButtons;
    public GameObject victoryPanel;
    public GameObject defeatPanel;
    public TMP_Text victoryText;

    [Header("Части рисунка")]
    public GameObject[] correctParts;    // Правильные части (изначально невидимы)
    public GameObject[] wrongParts;      // Неправильные части (изначально невидимы)
    public Image baseDrawing;           // Базовый контур (всегда виден)

    [Header("Настройки")]
    public int totalQuestions = 4;
    public int maxErrors = 2;

    private int currentQuestion = 0;
    private int correctAnswer;
    private bool isGameActive = true;
    private int errorCount = 0;
    private int correctAnswers = 0;

    void Start()
    {
        InitializeDrawing();
        GenerateQuestion();
    }

    void InitializeDrawing()
    {
        // Скрываем все части рисунка изначально
        foreach (GameObject part in correctParts)
        {
            if (part != null) part.SetActive(false);
        }
        foreach (GameObject part in wrongParts)
        {
            if (part != null) part.SetActive(false);
        }

        // Базовый контур всегда виден
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
            // Правильный ответ - показываем правильную часть
            correctAnswers++;
            ShowPart(currentQuestion, true);
        }
        else
        {
            // Неправильный ответ - показываем неправильную часть
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
                // Показываем правильную часть
                if (correctParts[partIndex] != null)
                {
                    correctParts[partIndex].SetActive(true);
                    StartCoroutine(AnimateAppearance(correctParts[partIndex]));
                }
                // Гарантируем что неправильная часть скрыта
                if (wrongParts[partIndex] != null)
                    wrongParts[partIndex].SetActive(false);
            }
            else
            {
                // Показываем неправильную часть
                if (wrongParts[partIndex] != null)
                {
                    wrongParts[partIndex].SetActive(true);
                    StartCoroutine(AnimateAppearance(wrongParts[partIndex]));
                }
                // Гарантируем что правильная часть скрыта
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
            // Анимация появления (fade in)
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

        int levelNumber = GetCurrentLevelNumber();

        // Сохраняем прогресс
        SaveLevelProgress(levelNumber, isPerfect);

        if (isPerfect)
        {
            // Идеальное прохождение - разблокируем рисунок в галерее
            UnlockGalleryImage(levelNumber);
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

        int levelNumber = GetCurrentLevelNumber();

        // Сохраняем обычный прогресс (без галереи)
        SaveLevelProgress(levelNumber, false);

        TMP_Text defeatText = defeatPanel.GetComponentInChildren<TMP_Text>();
        if (defeatText != null)
        {
            defeatText.text = errorCount >= maxErrors ?
                "Слишком много ошибок!\nПопробуй еще раз!" :
                $"Почти получилось!\nПравильных ответов: {correctAnswers}/{totalQuestions}";
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

    void SaveLevelProgress(int levelNumber, bool isPerfect)
    {
        int currentSlot = PlayerPrefs.GetInt("CurrentSaveSlot", 0);

        // Сохраняем что уровень пройден
        string completedKey = $"Slot_{currentSlot}_Coloring_Level_{levelNumber}_completed";
        PlayerPrefs.SetInt(completedKey, 1);

        // Сохраняем количество звезд (3 звезды за идеальное прохождение, 1 за обычное)
        int stars = isPerfect ? 3 : (correctAnswers >= totalQuestions * 0.7f ? 2 : 1);
        string starsKey = $"Slot_{currentSlot}_Coloring_Level_{levelNumber}_stars";
        PlayerPrefs.SetInt(starsKey, stars);

        // Сохраняем идеальное прохождение для галереи
        if (isPerfect)
        {
            string perfectKey = $"Slot_{currentSlot}_Coloring_Level_{levelNumber}_perfect";
            PlayerPrefs.SetInt(perfectKey, 1);
        }

        PlayerPrefs.Save();

        Debug.Log($"Сохранен прогресс: Уровень {levelNumber}, Звезд: {stars}, Идеально: {isPerfect}");
    }

    void UnlockGalleryImage(int levelNumber)
    {
        int currentSlot = PlayerPrefs.GetInt("CurrentSaveSlot", 0);
        string galleryKey = $"Slot_{currentSlot}_Gallery_Level_{levelNumber}";
        PlayerPrefs.SetInt(galleryKey, 1);
        PlayerPrefs.Save();

        Debug.Log($"Рисунок уровня {levelNumber} разблокирован в галерее слота {currentSlot}!");
    }

    // Кнопки UI
    public void RestartLevel()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex
        );
    }

    public void BackToMenu()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(2);
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
                UnityEngine.SceneManagement.SceneManager.LoadScene(nextSceneName);
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