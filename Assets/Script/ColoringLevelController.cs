using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class ColoringLevelController : MonoBehaviour
{
    [Header("Game Elements")]
    public TMP_Text questionText;
    public Button[] answerButtons;
    public Image coloringImage; // Черно-белое изображение для раскраски
    public Image referenceImage; // Цветное изображение-образец (можно скрыть)

    [Header("Level Settings")]
    public string levelId = "Coloring_Level_1";
    public int totalQuestions = 8;
    public int partsToColor = 8; // Количество частей для раскраски

    [Header("UI Elements")]
    public GameObject victoryPanel;
    public TMP_Text victoryText;

    private int currentQuestion = 0;
    private int correctAnswer;
    private bool isGameActive = true;
    private int correctAnswers = 0;
    private int currentPartIndex = 0;

    // Маски для частей изображения (должны быть настроены в редакторе)
    [Header("Image Parts")]
    public Image[] imageParts;

    void Start()
    {
        if (string.IsNullOrEmpty(levelId))
        {
            levelId = PlayerPrefs.GetString("CurrentLevel", "Coloring_Level_1");
        }

        // Изначально делаем все части черно-белыми
        InitializeImageParts();
        GenerateQuestion();
    }

    void InitializeImageParts()
    {
        foreach (Image part in imageParts)
        {
            if (part != null)
            {
                part.color = Color.white; // Белый = не раскрашено
            }
        }
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
                    int offset = Random.Range(1, 4);
                    wrongAnswer = correctAnswer + (Random.Range(0, 2) == 0 ? offset : -offset);
                } while (wrongAnswer == correctAnswer || wrongAnswer <= 0 || wrongAnswer > 81);

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
            // Правильный ответ - раскрашиваем правильно
            correctAnswers++;
            ColorImagePart(true);
        }
        else
        {
            // Неправильный ответ - раскрашиваем неправильно
            ColorImagePart(false);
        }

        currentQuestion++;
        currentPartIndex++;

        if (currentQuestion >= totalQuestions || currentPartIndex >= partsToColor)
        {
            EndLevel();
        }
        else
        {
            GenerateQuestion();
        }
    }

    void ColorImagePart(bool isCorrect)
    {
        if (currentPartIndex < imageParts.Length && imageParts[currentPartIndex] != null)
        {
            Color newColor;

            if (isCorrect)
            {
                // Правильный цвет (берем из палитры или предустановленных цветов)
                newColor = GetCorrectColorForPart(currentPartIndex);
            }
            else
            {
                // Неправильный цвет (случайный неподходящий)
                newColor = GetWrongColorForPart(currentPartIndex);
            }

            // Плавная анимация раскрашивания
            StartCoroutine(AnimateColorChange(imageParts[currentPartIndex], newColor));
        }
    }

    Color GetCorrectColorForPart(int partIndex)
    {
        // Предустановленные правильные цвета для каждой части
        Color[] correctColors = {
            new Color(1f, 0.5f, 0.5f), // Красный
            new Color(0.5f, 1f, 0.5f), // Зеленый
            new Color(0.5f, 0.5f, 1f), // Синий
            new Color(1f, 1f, 0.5f),   // Желтый
            new Color(1f, 0.5f, 1f),   // Фиолетовый
            new Color(0.5f, 1f, 1f),   // Бирюзовый
            new Color(1f, 0.8f, 0.5f), // Оранжевый
            new Color(0.8f, 0.8f, 0.8f) // Серый
        };

        return correctColors[partIndex % correctColors.Length];
    }

    Color GetWrongColorForPart(int partIndex)
    {
        Color correctColor = GetCorrectColorForPart(partIndex);
        Color wrongColor;

        // Генерируем явно неправильный цвет
        do
        {
            wrongColor = new Color(
                Random.Range(0f, 1f),
                Random.Range(0f, 1f),
                Random.Range(0f, 1f)
            );
        } while (ColorDistance(correctColor, wrongColor) < 0.5f); // Убедимся что цвета сильно отличаются

        return wrongColor;
    }

    float ColorDistance(Color a, Color b)
    {
        return Mathf.Abs(a.r - b.r) + Mathf.Abs(a.g - b.g) + Mathf.Abs(a.b - b.b);
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

    void EndLevel()
    {
        isGameActive = false;

        // Проверяем, достаточно ли правильных ответов для добавления в галерею
        bool isPicturePerfect = correctAnswers >= totalQuestions * 0.8f; // 80% правильных ответов

        if (isPicturePerfect)
        {
            SaveToGallery();
            victoryText.text = "Отлично! Рисунок добавлен в галерею!";
        }
        else
        {
            victoryText.text = "Хорошая попытка! Попробуй еще раз!";
        }

        victoryPanel.SetActive(true);

        // Сохраняем прогресс уровня
        SaveLevelProgress();
    }

    void SaveLevelProgress()
    {
        int currentSlot = PlayerPrefs.GetInt("CurrentSaveSlot", 0);
        int stars = correctAnswers >= totalQuestions ? 3 : (correctAnswers >= totalQuestions * 0.7f ? 2 : 1);

        PlayerPrefs.SetInt($"Slot_{currentSlot}_{levelId}_completed", 1);
        PlayerPrefs.SetInt($"Slot_{currentSlot}_{levelId}_stars", stars);
        PlayerPrefs.Save();
    }

    void SaveToGallery()
    {
        int currentSlot = PlayerPrefs.GetInt("CurrentSaveSlot", 0);

        // Сохраняем что этот уровень завершен идеально для галереи
        PlayerPrefs.SetInt($"Slot_{currentSlot}_Gallery_{levelId}", 1);
        PlayerPrefs.Save();

        Debug.Log($"Рисунок {levelId} добавлен в галерею слота {currentSlot}");
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
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }
}