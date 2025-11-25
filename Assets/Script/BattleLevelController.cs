using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

[System.Serializable]
public class LevelProgress
{
    public int starsEarned;
    public bool isCompleted;
}

public class BattleLevelController : MonoBehaviour
{
    [Header("Player & Enemy")]
    public Image playerImage;
    public Image enemyImage;
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
    public GameObject[] victoryStars;
    public TMP_Text victoryText;

    [Header("Timer UI - Slider")]
    public TMP_Text timerText;
    public Slider timerSlider;
    public Image timerFillImage;

    [Header("Bonus Effects")]
    public TMP_Text bonusText;

    [Header("Game Settings")]
    public string levelId = "Battle_Level_1";

    [Header("Progressive Difficulty")]
    public ProgressiveLevelManager difficultyManager;

    private int currentQuestion = 0;
    private int correctAnswer;
    private bool isGameActive = true;
    private int starsEarned = 0;
    private SmartQuestionGenerator questionGenerator;
    private AdaptiveDifficulty adaptiveDifficulty;
    private float questionStartTime;
    private int correctCombo = 0;
    private float timeRemaining;
    private bool isTimerRunning = false;
    private int totalQuestions = 5;

    // ИСПРАВЛЕНИЕ 3: Защита от множественных нажатий
    private bool isProcessingAnswer = false;
    private Coroutine currentAttackCoroutine;

    void Start()
    {
        // Динамическое определение levelId
        levelId = GetCurrentLevelId();

        // Инициализация системы сложности
        questionGenerator = new SmartQuestionGenerator();
        adaptiveDifficulty = new AdaptiveDifficulty();

        // Находим ProgressiveLevelManager если не установлен в Inspector
        if (difficultyManager == null)
            difficultyManager = FindObjectOfType<ProgressiveLevelManager>();

        // Воспроизводим музыку для битвы
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayBattleGameMusic();
        }

        GenerateQuestion();
        UpdateHearts();

        Debug.Log($"Загружен уровень: {levelId}");
    }

    void Update()
    {
        if (isGameActive && isTimerRunning)
        {
            UpdateTimer();
        }
    }

    void UpdateTimer()
    {
        // ИСПРАВЛЕНИЕ 4: Защита таймера
        if (!isGameActive) return;

        if (timeRemaining > 0)
        {
            timeRemaining -= Time.deltaTime;

            // Обновляем текст
            if (timerText != null)
                timerText.text = Mathf.CeilToInt(timeRemaining).ToString();

            // Обновляем слайдер
            if (timerSlider != null)
            {
                float totalTime = GetTimeLimitForCurrentLevel();
                float fillAmount = timeRemaining / totalTime;
                timerSlider.value = fillAmount;

                // Меняем цвет в зависимости от времени
                if (timerFillImage != null)
                {
                    if (fillAmount < 0.3f)
                        timerFillImage.color = Color.red;
                    else if (fillAmount < 0.6f)
                        timerFillImage.color = Color.yellow;
                    else
                        timerFillImage.color = Color.green;
                }
            }
        }
        else if (isTimerRunning) // ИСПРАВЛЕНИЕ 4: Защита от повторных вызовов
        {
            TimeOut();
        }
    }

    void TimeOut()
    {
        // ИСПРАВЛЕНИЕ 3: Защита от множественных обработок
        if (!isGameActive || isProcessingAnswer) return;

        isTimerRunning = false;
        correctCombo = 0;
        playerHealth--;

        // ИСПРАВЛЕНИЕ 1: Защищенный парсинг
        if (TryParseQuestion(questionText.text, out int num1, out int num2))
        {
            adaptiveDifficulty.RecordMistake(num1, num2);
        }

        // Звук при потере времени
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX("BattleWrongAnswer");
            AudioManager.Instance.PlaySFX("PlayerDamage");
        }

        UpdateHearts();
        currentQuestion++;

        ShowBonusEffect("Время вышло! -1 здоровье");

        // ИСПРАВЛЕНИЕ 2: Задержка для стабильности
        StartCoroutine(CheckGameEndAfterDelay());
    }

    // ИСПРАВЛЕНИЕ 1: Защищенный метод парсинга
    private bool TryParseQuestion(string questionText, out int num1, out int num2)
    {
        num1 = 0;
        num2 = 0;

        try
        {
            string[] parts = questionText.Split('×');
            if (parts.Length != 2) return false;

            string[] secondPart = parts[1].Split('=');
            if (secondPart.Length == 0) return false;

            if (!int.TryParse(parts[0].Trim(), out num1)) return false;
            if (!int.TryParse(secondPart[0].Trim(), out num2)) return false;

            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Ошибка парсинга вопроса: {e.Message}");
            return false;
        }
    }

    IEnumerator CheckGameEndAfterDelay()
    {
        yield return new WaitForSeconds(0.3f);

        if (playerHealth <= 0)
        {
            Defeat();
        }
        else if (currentQuestion < totalQuestions)
        {
            GenerateQuestion();
        }
        else
        {
            if (playerHealth > enemyHealth)
                Victory();
            else if (enemyHealth > playerHealth)
                Defeat();
            else
                Draw();
        }
    }

    float GetTimeLimitForCurrentLevel()
    {
        DifficultySettings difficulty = difficultyManager.GetDifficultyForLevel(levelId);
        return difficulty.timeBonusThreshold * 2f;
    }

    string GetCurrentLevelId()
    {
        string sceneName = SceneManager.GetActiveScene().name;

        if (sceneName.Contains("1")) return "Battle_Level_1";
        else if (sceneName.Contains("2")) return "Battle_Level_2";
        else if (sceneName.Contains("3")) return "Battle_Level_3";
        else if (sceneName.Contains("4")) return "Battle_Level_4";
        else if (sceneName.Contains("5")) return "Battle_Level_5";
        else if (sceneName.Contains("6")) return "Battle_Level_6";
        else if (sceneName.Contains("7")) return "Battle_Level_7";
        else if (sceneName.Contains("8")) return "Battle_Level_8";
        else return "Battle_Level_1";
    }

    void GenerateQuestion()
    {
        if (!isGameActive) return;

        // Получаем настройки сложности для текущего уровня
        DifficultySettings difficulty = difficultyManager.GetDifficultyForLevel(levelId);
        totalQuestions = difficulty.totalQuestions;
        int[] weakNumbers = adaptiveDifficulty.GetWeakNumbers();

        // Генерируем вопрос
        QuestionData question = questionGenerator.GenerateQuestion(difficulty, weakNumbers);

        questionText.text = $"{question.number1} × {question.number2} = ?";
        correctAnswer = question.correctAnswer;
        questionStartTime = Time.time;

        // Запускаем таймер
        timeRemaining = GetTimeLimitForCurrentLevel();
        isTimerRunning = true;

        // Сбрасываем слайдер и цвет
        if (timerSlider != null)
            timerSlider.value = 1f;

        if (timerFillImage != null)
            timerFillImage.color = Color.green;

        if (timerText != null)
            timerText.color = Color.white;

        // Создаем массив ответов
        int[] answers = new int[4];
        int correctIndex = Random.Range(0, 4);
        answers[correctIndex] = correctAnswer;

        // Заполняем неправильные ответы
        int wrongIndex = 0;
        for (int i = 0; i < 4; i++)
        {
            if (i != correctIndex)
            {
                answers[i] = question.wrongAnswers[wrongIndex];
                wrongIndex++;
            }

            answerButtons[i].GetComponentInChildren<TMP_Text>().text = answers[i].ToString();

            int index = i;
            answerButtons[i].onClick.RemoveAllListeners();
            answerButtons[i].onClick.AddListener(() => OnAnswerSelected(index, answers[index]));
        }
    }

    public void OnAnswerSelected(int buttonIndex, int selectedAnswer)
    {
        // ИСПРАВЛЕНИЕ 3: Защита от множественных нажатий
        if (!isGameActive || !isTimerRunning || isProcessingAnswer) return;

        isProcessingAnswer = true;
        isTimerRunning = false;

        // ИСПРАВЛЕНИЕ 2: Останавливаем предыдущие корутины
        if (currentAttackCoroutine != null)
        {
            StopCoroutine(currentAttackCoroutine);
        }

        // Получаем настройки для определения порога бонуса
        DifficultySettings difficulty = difficultyManager.GetDifficultyForLevel(levelId);
        float reactionTime = Time.time - questionStartTime;
        bool isFast = reactionTime < difficulty.timeBonusThreshold;

        if (selectedAnswer == correctAnswer)
        {
            // Правильный ответ
            correctCombo++;
            enemyHealth--;

            // Звуки правильного ответа
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX("BattleCorrectAnswer");
                AudioManager.Instance.PlaySFX("PlayerAttack");
                AudioManager.Instance.PlaySFX("EnemyDamage");

                if (isFast)
                {
                    AudioManager.Instance.PlaySFX("StarAppear");
                }
            }

            if (isFast)
            {
                ShowBonusEffect($"Быстро! ({reactionTime:F1}с)");
            }

            currentAttackCoroutine = StartCoroutine(AttackEnemy());
        }
        else
        {
            // Неправильный ответ
            correctCombo = 0;
            playerHealth--;

            // Звуки неправильного ответа
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX("BattleWrongAnswer");
                AudioManager.Instance.PlaySFX("EnemyAttack");
                AudioManager.Instance.PlaySFX("PlayerDamage");
            }

            // ИСПРАВЛЕНИЕ 1: Защищенный парсинг
            if (TryParseQuestion(questionText.text, out int num1, out int num2))
            {
                adaptiveDifficulty.RecordMistake(num1, num2);
            }

            currentAttackCoroutine = StartCoroutine(AttackPlayer());
        }

        UpdateHearts();
        currentQuestion++;

        // ИСПРАВЛЕНИЕ 2: Задержка перед проверкой конца игры
        StartCoroutine(CheckGameEndAfterAttack());
    }

    IEnumerator CheckGameEndAfterAttack()
    {
        // Ждем завершения анимации атаки
        yield return new WaitForSeconds(0.8f);

        // Разблокируем ввод
        isProcessingAnswer = false;

        if (enemyHealth <= 0)
        {
            Victory();
        }
        else if (playerHealth <= 0)
        {
            Defeat();
        }
        else if (currentQuestion < totalQuestions)
        {
            GenerateQuestion();
        }
        else
        {
            if (playerHealth > enemyHealth)
                Victory();
            else if (enemyHealth > playerHealth)
                Defeat();
            else
                Draw();
        }
    }

    IEnumerator AttackEnemy()
    {
        // ИСПРАВЛЕНИЕ 2: Защита в корутинах
        if (!isGameActive) yield break;

        Vector2 originalPos = playerImage.rectTransform.anchoredPosition;
        playerImage.rectTransform.anchoredPosition = originalPos + new Vector2(50f, 0);

        yield return new WaitForSeconds(0.3f);

        if (!isGameActive) yield break;
        enemyImage.color = Color.red;

        yield return new WaitForSeconds(0.2f);

        if (!isGameActive) yield break;
        playerImage.rectTransform.anchoredPosition = originalPos;
        enemyImage.color = Color.white;
    }

    IEnumerator AttackPlayer()
    {
        // ИСПРАВЛЕНИЕ 2: Защита в корутинах
        if (!isGameActive) yield break;

        Vector2 originalPos = enemyImage.rectTransform.anchoredPosition;
        enemyImage.rectTransform.anchoredPosition = originalPos + new Vector2(-50f, 0);

        yield return new WaitForSeconds(0.3f);

        if (!isGameActive) yield break;
        playerImage.color = Color.red;

        yield return new WaitForSeconds(0.2f);

        if (!isGameActive) yield break;
        enemyImage.rectTransform.anchoredPosition = originalPos;
        playerImage.color = Color.white;
    }

    void UpdateHearts()
    {
        for (int i = 0; i < playerHearts.Length; i++)
            playerHearts[i].SetActive(i < playerHealth);
        for (int i = 0; i < enemyHearts.Length; i++)
            enemyHearts[i].SetActive(i < enemyHealth);
    }

    IEnumerator AnimateStars()
    {
        for (int i = 0; i < victoryStars.Length; i++)
        {
            if (i < starsEarned && isGameActive) // ИСПРАВЛЕНИЕ 2: Защита
            {
                victoryStars[i].SetActive(true);
                victoryStars[i].transform.localScale = Vector3.zero;

                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlaySFX("StarAppear");
                }

                float duration = 0.5f;
                float elapsed = 0f;
                while (elapsed < duration && isGameActive) // ИСПРАВЛЕНИЕ 2: Защита
                {
                    victoryStars[i].transform.localScale = Vector3.one * (elapsed / duration);
                    elapsed += Time.deltaTime;
                    yield return null;
                }

                if (isGameActive) // ИСПРАВЛЕНИЕ 2: Защита
                {
                    victoryStars[i].transform.localScale = Vector3.one;
                }
            }
            yield return new WaitForSeconds(0.2f);
        }

        if (AudioManager.Instance != null && isGameActive)
        {
            AudioManager.Instance.PlaySFX("LevelComplete");
        }
    }

    void Victory()
    {
        if (!isGameActive) return; // ИСПРАВЛЕНИЕ 2: Защита от повторных вызовов

        isGameActive = false;
        isTimerRunning = false;
        starsEarned = CalculateStars();
        SaveLevelProgress(levelId, starsEarned);

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayVictoryMusic();
        }

        StartCoroutine(AnimateStars());

        if (victoryText != null)
            victoryText.text = $"Победа!\nЗвезд: {starsEarned}/3";

        victoryPanel.SetActive(true);
    }

    void SaveLevelProgress(string levelId, int stars)
    {
        try
        {
            int currentSlot = SimpleSlotManager.GetCurrentSlot();
            PlayerPrefs.SetInt($"Slot_{currentSlot}_{levelId}_stars", stars);
            PlayerPrefs.SetInt($"Slot_{currentSlot}_{levelId}_completed", 1);
            PlayerPrefs.Save();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Ошибка сохранения: {e.Message}");
        }
    }

    int CalculateStars()
    {
        return playerHealth;
    }

    void Defeat()
    {
        if (!isGameActive) return; // ИСПРАВЛЕНИЕ 2: Защита от повторных вызовов

        isGameActive = false;
        isTimerRunning = false;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayDefeatMusic();
        }

        defeatPanel.SetActive(true);
    }

    void Draw()
    {
        if (!isGameActive) return; // ИСПРАВЛЕНИЕ 2: Защита от повторных вызовов

        isGameActive = false;
        isTimerRunning = false;
        starsEarned = 1;
        SaveLevelProgress(levelId, starsEarned);

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayDefeatMusic();
        }

        StartCoroutine(AnimateStars());

        if (victoryText != null)
            victoryText.text = $"Ничья!\nЗвезд: {starsEarned}/3";

        victoryPanel.SetActive(true);
    }

    void ShowBonusEffect(string message)
    {
        if (!isGameActive) return; // ИСПРАВЛЕНИЕ 2: Защита

        Debug.Log(message);

        if (bonusText != null)
        {
            bonusText.text = message;
            bonusText.gameObject.SetActive(true);
            StartCoroutine(HideBonusText());
        }
    }

    IEnumerator HideBonusText()
    {
        yield return new WaitForSeconds(2f);
        if (bonusText != null && isGameActive) // ИСПРАВЛЕНИЕ 2: Защита
            bonusText.gameObject.SetActive(false);
    }

    public void RestartLevel()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonClick();
        }

        // ИСПРАВЛЕНИЕ 2: Очистка корутин
        StopAllCoroutines();

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void BackToMenu()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonClick();
        }

        // ИСПРАВЛЕНИЕ 2: Очистка корутин
        StopAllCoroutines();

        SceneManager.LoadScene(1);
    }

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

    void OnDestroy()
    {
        // ИСПРАВЛЕНИЕ 2: Очистка при выгрузке
        StopAllCoroutines();
    }
}