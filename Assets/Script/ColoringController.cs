using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Linq;

public class ColoringController : MonoBehaviour
{
    [Header("Основные элементы")]
    public TMP_Text questionText;
    public Button[] answerButtons;
    public GameObject victoryPanel;
    public GameObject defeatPanel;
    public TMP_Text victoryText;
    public TMP_Text difficultyText;
    public TMP_Text levelProgressText;

    [Header("Части рисунка")]
    public GameObject[] correctParts;
    public GameObject[] wrongParts;
    public Image baseDrawing;

    [Header("Настройки")]
    public int maxErrors = 2;
    public string levelId = "Coloring_Level_1";

    [Header("Система сложности")]
    public ProgressiveLevelManager progressiveLevelManager;

    private int currentQuestion = 0;
    private int correctAnswer;
    private bool isGameActive = true;
    private int errorCount = 0;
    private int correctAnswers = 0;
    private int currentDifficultyLevel = 0;
    private AdaptiveDifficulty adaptiveDifficulty = new AdaptiveDifficulty();
    private DifficultySettings currentDifficultySettings;

    void Start()
    {
        if (string.IsNullOrEmpty(levelId))
        {
            levelId = "Coloring_Level_1";
        }

        // Получаем настройки сложности из ProgressiveLevelManager
        currentDifficultySettings = progressiveLevelManager.GetDifficultyForLevel(levelId);

        // Определяем текущий уровень сложности на основе levelId
        currentDifficultyLevel = progressiveLevelManager.ExtractLevelNumber(levelId) - 1;

        // Воспроизводим музыку для режима раскраски
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayColoringGameMusic();
        }

        InitializeDrawing();
        UpdateDifficultyDisplay();
        GenerateQuestion();

        Debug.Log($"Загружен уровень раскраски: {levelId}, Уровень сложности: {currentDifficultyLevel + 1}");
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

    void UpdateDifficultyDisplay()
    {
        if (difficultyText != null)
        {
            difficultyText.text = $"Уровень {currentDifficultyLevel + 1}";
            difficultyText.color = GetDifficultyColor(currentDifficultyLevel);
        }

        if (levelProgressText != null)
        {
            levelProgressText.text = $"Вопрос: {currentQuestion + 1}/{currentDifficultySettings.totalQuestions}";
        }
    }

    Color GetDifficultyColor(int level)
    {
        switch (level)
        {
            case 0: case 1: return Color.green;
            case 2: case 3: return Color.yellow;
            case 4: case 5: return new Color(1f, 0.5f, 0f); // оранжевый
            case 6: return Color.red;
            case 7: return Color.magenta;
            default: return Color.white;
        }
    }

    void GenerateQuestion()
    {
        if (!isGameActive) return;

        int number1, number2;

        // Используем адаптивную сложность: иногда показываем слабые числа
        int[] weakNumbers = adaptiveDifficulty.GetWeakNumbers();
        bool useWeakNumber = weakNumbers.Length > 0 && Random.Range(0f, 1f) > 0.7f;

        if (useWeakNumber && weakNumbers.Length > 0)
        {
            // Используем одно из слабых чисел
            number1 = weakNumbers[Random.Range(0, weakNumbers.Length)];
            number2 = Random.Range(currentDifficultySettings.minNumber, currentDifficultySettings.maxNumber + 1);
        }
        else
        {
            // Обычная генерация чисел
            number1 = Random.Range(currentDifficultySettings.minNumber, currentDifficultySettings.maxNumber + 1);
            number2 = Random.Range(currentDifficultySettings.minNumber, currentDifficultySettings.maxNumber + 1);
        }

        correctAnswer = number1 * number2;
        questionText.text = $"{number1} × {number2} = ?";

        GenerateAnswers();
    }

    void GenerateAnswers()
    {
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
                answers[i] = GenerateWrongAnswer(answers, i);
            }

            answerButtons[i].GetComponentInChildren<TMP_Text>().text = answers[i].ToString();

            int index = i;
            answerButtons[i].onClick.RemoveAllListeners();
            answerButtons[i].onClick.AddListener(() => OnAnswerSelected(index, answers[index]));
        }
    }

    int GenerateWrongAnswer(int[] existingAnswers, int currentIndex)
    {
        int wrongAnswer;
        int attempts = 0;

        do
        {
            if (currentDifficultySettings.useTrickyAnswers && attempts < 3)
            {
                // Генерируем "хитрые" неправильные ответы
                wrongAnswer = GenerateTrickyAnswer();
            }
            else
            {
                // Обычные неправильные ответы
                int variation = Random.Range(-currentDifficultySettings.wrongAnswerRange,
                                           currentDifficultySettings.wrongAnswerRange + 1);
                wrongAnswer = correctAnswer + variation;
            }

            attempts++;

            if (attempts > 20)
            {
                wrongAnswer = correctAnswer + (currentIndex + 1) * (currentIndex % 2 == 0 ? 1 : -1);
                break;
            }

        } while (wrongAnswer == correctAnswer || wrongAnswer <= 0 ||
                 System.Array.Exists(existingAnswers, x => x == wrongAnswer));

        return wrongAnswer;
    }

    int GenerateTrickyAnswer()
    {
        // Генерируем "хитрые" ответы, которые могут сбить с толку
        int trickyType = Random.Range(0, 3);

        switch (trickyType)
        {
            case 0: // Ответ от соседнего примера
                int neighbor1 = Random.Range(currentDifficultySettings.minNumber, currentDifficultySettings.maxNumber + 1);
                int neighbor2 = Random.Range(currentDifficultySettings.minNumber, currentDifficultySettings.maxNumber + 1);
                return neighbor1 * neighbor2;

            case 1: // Перепутанные цифры (например, 24 вместо 42)
                string correctStr = correctAnswer.ToString();
                if (correctStr.Length == 2)
                {
                    char[] chars = correctStr.ToCharArray();
                    System.Array.Reverse(chars);
                    return int.Parse(new string(chars));
                }
                break;

            case 2: // Сложение вместо умножения
                int a = correctAnswer / 10;
                int b = correctAnswer % 10;
                if (a > 0 && b > 0) return a + b;
                break;
        }

        // Fallback - обычный неправильный ответ
        return correctAnswer + Random.Range(-currentDifficultySettings.wrongAnswerRange,
                                          currentDifficultySettings.wrongAnswerRange + 1);
    }

    public void OnAnswerSelected(int buttonIndex, int selectedAnswer)
    {
        if (!isGameActive) return;

        if (selectedAnswer == correctAnswer)
        {
            // Звуки правильного ответа
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX("ColoringCorrectAnswer");
                AudioManager.Instance.PlaySFX("ColorAppear");
            }

            correctAnswers++;
            ShowPart(currentQuestion, true);
        }
        else
        {
            // Звуки неправильного ответа
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX("ColoringWrongAnswer");
            }

            errorCount++;

            // Записываем ошибку в адаптивную систему
            string[] numbers = questionText.text.Split('×');
            if (numbers.Length == 2)
            {
                int num1 = int.Parse(numbers[0].Trim());
                int num2 = int.Parse(numbers[1].Split('=')[0].Trim());
                adaptiveDifficulty.RecordMistake(num1, num2);
            }

            ShowPart(currentQuestion, false);
        }

        currentQuestion++;

        if (errorCount >= maxErrors)
        {
            Defeat();
        }
        else if (currentQuestion >= currentDifficultySettings.totalQuestions)
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

        SaveLevelProgress(isPerfect);

        // Звуки победы
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayVictoryMusic();
            AudioManager.Instance.PlaySFX("DrawingComplete");

            // Дополнительный звук для идеального результата
            if (isPerfect)
            {
                AudioManager.Instance.PlaySFX("StarAppear");
            }
        }

        bool isHighLevel = currentDifficultyLevel >= 5;
        bool isExpertLevel = currentDifficultyLevel >= 7;

        if (isPerfect)
        {
            UnlockGalleryImage();

            victoryText.text = "Идеально! \nРисунок добавлен в галерею!";
        }
        else
        {
            string adaptiveInfo = "";
            int[] weakNumbers = adaptiveDifficulty.GetWeakNumbers();

            victoryText.text = $"Неплохо! Правильных ответов: {correctAnswers}/{currentDifficultySettings.totalQuestions}{adaptiveInfo}";
        }

        victoryPanel.SetActive(true);
    }

    void Defeat()
    {
        isGameActive = false;

        SaveLevelProgress(false);

        // Звуки поражения
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayDefeatMusic();
        }

        TMP_Text defeatText = defeatPanel.GetComponentInChildren<TMP_Text>();
        if (defeatText != null)
        {
            string adaptiveInfo = "";
            int[] weakNumbers = adaptiveDifficulty.GetWeakNumbers();

            defeatText.text = errorCount >= maxErrors ?
                $"Слишком много ошибок!\nПопробуй еще раз!{adaptiveInfo}" :
                $"Почти получилось!\nПравильных ответов: {correctAnswers}/{currentDifficultySettings.totalQuestions}{adaptiveInfo}";
        }

        defeatPanel.SetActive(true);
    }

    void SaveLevelProgress(bool isPerfect)
    {
        int currentSlot = PlayerPrefs.GetInt("CurrentSaveSlot", 0);

        // Сохраняем что уровень пройден
        string completedKey = $"Slot_{currentSlot}_{levelId}_completed";
        PlayerPrefs.SetInt(completedKey, 1);

        // Расчет звезд
        int baseStars = isPerfect ? 3 : (correctAnswers >= currentDifficultySettings.totalQuestions * 0.7f ? 2 : 1);

        // Бонус за высокий уровень сложности
        int difficultyBonus = (currentDifficultyLevel >= 5 && correctAnswers >= currentDifficultySettings.totalQuestions * 0.8f) ? 1 : 0;
        int totalStars = Mathf.Min(3, baseStars + difficultyBonus);

        string starsKey = $"Slot_{currentSlot}_{levelId}_stars";
        PlayerPrefs.SetInt(starsKey, totalStars);

        PlayerPrefs.Save();

        Debug.Log($"Сохранен прогресс: {levelId}, Звезд: {totalStars}, Уровень: {currentDifficultyLevel + 1}, Идеально: {isPerfect}");
    }

    void UnlockGalleryImage()
    {
        int currentSlot = PlayerPrefs.GetInt("CurrentSaveSlot", 0);

        int levelNumber = progressiveLevelManager.ExtractLevelNumber(levelId);

        string galleryKey = $"Slot_{currentSlot}_Gallery_Level_{levelNumber}";
        PlayerPrefs.SetInt(galleryKey, 1);
        PlayerPrefs.Save();

        Debug.Log($"Рисунок уровня {levelNumber} разблокирован в галерее слота {currentSlot}!");
    }

    // Кнопки UI
    public void RestartLevel()
    {
        // Звук кнопки перезапуска
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonClick();
        }
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void BackToMenu()
    {
        // Звук кнопки возврата
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonClick();
        }
        SceneManager.LoadScene(2);
    }

    public void LoadNextLevel()
    {
        // Звук кнопки следующего уровня
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonClick();
        }

        int currentLevel = progressiveLevelManager.ExtractLevelNumber(levelId);
        int nextLevel = currentLevel + 1;

        if (nextLevel <= 4) // У вас 4 уровня раскраски
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

    // Дополнительные методы для управления звуками
    public void PlayButtonSound()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonClick();
        }
    }

    public void PlayHoverSound()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonHover();
        }
    }
}