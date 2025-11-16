using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class ColoringController : MonoBehaviour
{
    [Header("Основные элементы")]
    public TMP_Text questionText;
    public Button[] answerButtons;
    public Image[] imageParts; // Part1, Part2, Part3, Part4
    public GameObject victoryPanel;
    public GameObject defeatPanel; // ДОБАВИЛИ ПАНЕЛЬ ПРОИГРЫША

    [Header("Настройки")]
    public int totalQuestions = 4;
    public int maxErrors = 2; // Максимум ошибок

    private int currentQuestion = 0;
    private int correctAnswer;
    private bool isGameActive = true;
    private int errorCount = 0; // Счетчик ошибок

    void Start()
    {
        GenerateQuestion();
    }

    void GenerateQuestion()
    {
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
            // Правильно - красим в хороший цвет
            ColorImagePart(currentQuestion, true);
            currentQuestion++;
        }
        else
        {
            // Неправильно - красим в плохой цвет и считаем ошибку
            ColorImagePart(currentQuestion, false);
            errorCount++;
            currentQuestion++;
        }

        // Проверяем условия окончания игры
        if (errorCount >= maxErrors)
        {
            // Слишком много ошибок - проигрыш
            Defeat();
        }
        else if (currentQuestion >= totalQuestions)
        {
            // Все вопросы закончились - проверяем победу
            if (errorCount == 0)
            {
                Victory();
            }
            else
            {
                Defeat();
            }
        }
        else
        {
            GenerateQuestion();
        }
    }

    void ColorImagePart(int partIndex, bool isCorrect)
    {
        if (partIndex < imageParts.Length)
        {
            Color color = isCorrect ? GetGoodColor(partIndex) : GetBadColor();
            imageParts[partIndex].color = color;
        }
    }

    Color GetGoodColor(int partIndex)
    {
        // Хорошие цвета для каждой части
        Color[] goodColors = {
            new Color(0.4f, 0.7f, 1f),    // Голубой (небо)
            new Color(0.3f, 0.8f, 0.3f),  // Зеленый (трава)
            new Color(1f, 0.8f, 0.3f),    // Желтый (солнце)
            new Color(0.9f, 0.6f, 0.3f)   // Коричневый (дерево)
        };

        return goodColors[partIndex];
    }

    Color GetBadColor()
    {
        // Случайный плохой цвет (яркий и некрасивый)
        return new Color(
            Random.Range(0.8f, 1f),   // Яркий красный
            Random.Range(0f, 0.2f),   // Минимум зеленого
            Random.Range(0.8f, 1f)    // Яркий синий
        );
    }

    void Victory()
    {
        isGameActive = false;
        victoryPanel.SetActive(true);

        // Сохраняем в галерею
        SaveToGallery();
    }

    void Defeat()
    {
        isGameActive = false;
        defeatPanel.SetActive(true);

        // Обновляем текст в зависимости от количества ошибок
        TMP_Text defeatText = defeatPanel.GetComponentInChildren<TMP_Text>();
        if (defeatText != null)
        {
            if (errorCount >= maxErrors)
            {
                defeatText.text = "Слишком много ошибок!\nПопробуй еще раз!";
            }
            else
            {
                defeatText.text = "Почти получилось!\nПопробуй еще раз!";
            }
        }
    }

    void SaveToGallery()
    {
        int currentSlot = PlayerPrefs.GetInt("CurrentSaveSlot", 0);
        string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        PlayerPrefs.SetInt($"Slot_{currentSlot}_Gallery_{sceneName}", 1);
        PlayerPrefs.Save();
        Debug.Log("Рисунок добавлен в галерею!");
    }

    // Кнопки
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