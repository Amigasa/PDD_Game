using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class BattleLevelController : MonoBehaviour
{
    [Header("Player & Enemy")]
    public GameObject player;
    public GameObject enemy;
    public int playerHealth = 3;
    public int enemyHealth = 3;

    [Header("UI Elements")]
    public TMP_Text questionText;
    public Button[] answerButtons;
    public GameObject[] playerHearts;
    public GameObject[] enemyHearts;
    public GameObject victoryPanel;
    public GameObject defeatPanel;

    [Header("Game Settings")]
    public int totalQuestions = 5;

    private int currentQuestion = 0;
    private int correctAnswer;
    private bool isGameActive = true;

    void Start()
    {
        GenerateQuestion();
        UpdateHearts();
    }

    void GenerateQuestion()
    {
        if (!isGameActive) return;

        // Генерируем пример умножения
        int number1 = Random.Range(2, 10); // от 2 до 9
        int number2 = Random.Range(2, 10);
        correctAnswer = number1 * number2;

        questionText.text = $"{number1} × {number2} = ?";

        // Создаем массив ответов
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
                // Генерируем неправильные ответы
                int wrongAnswer;
                do
                {
                    int offset = Random.Range(1, 4);
                    wrongAnswer = correctAnswer + (Random.Range(0, 2) == 0 ? offset : -offset);
                } while (wrongAnswer == correctAnswer || wrongAnswer <= 0 || wrongAnswer > 81);

                answers[i] = wrongAnswer;
            }

            // Обновляем текст кнопок
            answerButtons[i].GetComponentInChildren<TMP_Text>().text = answers[i].ToString();

            // Убираем старые обработчики и добавляем новые
            int index = i; // важно для замыкания
            answerButtons[i].onClick.RemoveAllListeners();
            answerButtons[i].onClick.AddListener(() => OnAnswerSelected(index, answers[index]));
        }
    }

    public void OnAnswerSelected(int buttonIndex, int selectedAnswer)
    {
        if (!isGameActive) return;

        if (selectedAnswer == correctAnswer)
        {
            // Правильный ответ - атакуем монстра
            enemyHealth--;
            StartCoroutine(AttackEnemy());
        }
        else
        {
            // Неправильный ответ - получаем урон
            playerHealth--;
            StartCoroutine(AttackPlayer());
        }

        UpdateHearts();
        currentQuestion++;

        // Проверяем условия победы/поражения
        if (enemyHealth <= 0)
        {
            Victory();
            return;
        }
        else if (playerHealth <= 0)
        {
            Defeat();
            return;
        }

        // Генерируем следующий вопрос
        if (currentQuestion < totalQuestions)
        {
            GenerateQuestion();
        }
        else
        {
            // Если вопросы закончились, но здоровье еще есть
            if (playerHealth > enemyHealth)
                Victory();
            else if (enemyHealth > playerHealth)
                Defeat();
            else
                Draw(); // Ничья
        }
    }

    IEnumerator AttackEnemy()
    {
        // Анимация атаки игрока
        Vector3 originalPos = player.transform.position;
        player.transform.position = originalPos + new Vector3(0.5f, 0, 0);

        yield return new WaitForSeconds(0.3f);

        // Анимация получения урона монстром
        enemy.GetComponent<SpriteRenderer>().color = Color.red;

        yield return new WaitForSeconds(0.2f);

        player.transform.position = originalPos;
        enemy.GetComponent<SpriteRenderer>().color = Color.white;
    }

    IEnumerator AttackPlayer()
    {
        // Анимация атаки монстра
        Vector3 originalPos = enemy.transform.position;
        enemy.transform.position = originalPos + new Vector3(-0.5f, 0, 0);

        yield return new WaitForSeconds(0.3f);

        // Анимация получения урона игроком
        player.GetComponent<SpriteRenderer>().color = Color.red;

        yield return new WaitForSeconds(0.2f);

        enemy.transform.position = originalPos;
        player.GetComponent<SpriteRenderer>().color = Color.white;
    }

    void UpdateHearts()
    {
        // Обновляем сердца игрока
        for (int i = 0; i < playerHearts.Length; i++)
        {
            playerHearts[i].SetActive(i < playerHealth);
        }

        // Обновляем сердца монстра
        for (int i = 0; i < enemyHearts.Length; i++)
        {
            enemyHearts[i].SetActive(i < enemyHealth);
        }
    }

    void Victory()
    {
        isGameActive = false;
        victoryPanel.SetActive(true);
        // Сохраняем прогресс
        SaveManager.Instance.SaveLevelProgress("Battle_Level_1", CalculateStars());
    }

    void Defeat()
    {
        isGameActive = false;
        defeatPanel.SetActive(true);
    }

    void Draw()
    {
        isGameActive = false;
        // Можно сделать отдельную панель для ничьи или считать победой
        victoryPanel.SetActive(true);
        SaveManager.Instance.SaveLevelProgress("Battle_Level_1", 1); // 1 звезда за ничью
    }

    int CalculateStars()
    {
        if (playerHealth == 3) return 3;
        if (playerHealth == 2) return 2;
        return 1;
    }

    // Кнопки для UI
    public void RestartLevel()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
    }

    public void BackToMenu()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(0); // Главное меню
    }
}