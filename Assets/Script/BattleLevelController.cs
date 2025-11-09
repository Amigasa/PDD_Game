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

    [Header("Stars Display")]
    public GameObject[] victoryStars; // 3 звезды на панели победы
    public TMP_Text victoryText;

    [Header("Game Settings")]
    public int totalQuestions = 5;
    public string levelId = "Battle_Level_1"; // Уникальный ID уровня

    private int currentQuestion = 0;
    private int correctAnswer;
    private bool isGameActive = true;
    private int starsEarned = 0;

    void Start()
    {
        // Загружаем ID текущего уровня
        if (string.IsNullOrEmpty(levelId))
        {
            levelId = PlayerPrefs.GetString("CurrentLevel", "Battle_Level_1");
        }
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
    IEnumerator AnimateStars()
    {
        for (int i = 0; i < victoryStars.Length; i++)
        {
            if (i < starsEarned)
            {
                victoryStars[i].SetActive(true);
                victoryStars[i].transform.localScale = Vector3.zero;

                // Анимация увеличения
                float duration = 0.5f;
                float elapsed = 0f;
                while (elapsed < duration)
                {
                    victoryStars[i].transform.localScale = Vector3.one * (elapsed / duration);
                    elapsed += Time.deltaTime;
                    yield return null;
                }

                victoryStars[i].transform.localScale = Vector3.one;
            }
            yield return new WaitForSeconds(0.2f);
        }
    }

    void Victory()
    {
        isGameActive = false;
        starsEarned = CalculateStars();

        // Показываем звезды на панели победы
        StartCoroutine(AnimateStars());

        victoryPanel.SetActive(true);
        // Сохраняем прогресс
        SaveManager.Instance.SaveLevelProgress("Battle_Level_1", CalculateStars());
    }

    int CalculateStars()
    {
        // Победил и осталась 1 жизнь - 1 звезда
        // Победил и осталось 2 жизни - 2 звезды  
        // Победил и осталось 3 жизни - 3 звезды
        return playerHealth;
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