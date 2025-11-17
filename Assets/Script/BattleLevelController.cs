using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

[System.Serializable]
public class LevelProgress
{
    public int starsEarned;
    public bool isCompleted;
}

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
    public GameObject[] victoryStars;
    public TMP_Text victoryText;

    [Header("Game Settings")]
    public int totalQuestions = 5;
    public string levelId = "Battle_Level_1";

    private int currentQuestion = 0;
    private int correctAnswer;
    private bool isGameActive = true;
    private int starsEarned = 0;

    void Start()
    {
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
            enemyHealth--;
            StartCoroutine(AttackEnemy());
        }
        else
        {
            playerHealth--;
            StartCoroutine(AttackPlayer());
        }

        UpdateHearts();
        currentQuestion++;

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

        if (currentQuestion < totalQuestions)
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
        Vector3 originalPos = player.transform.position;
        player.transform.position = originalPos + new Vector3(0.5f, 0, 0);
        yield return new WaitForSeconds(0.3f);
        enemy.GetComponent<SpriteRenderer>().color = Color.red;
        yield return new WaitForSeconds(0.2f);
        player.transform.position = originalPos;
        enemy.GetComponent<SpriteRenderer>().color = Color.white;
    }

    IEnumerator AttackPlayer()
    {
        Vector3 originalPos = enemy.transform.position;
        enemy.transform.position = originalPos + new Vector3(-0.5f, 0, 0);
        yield return new WaitForSeconds(0.3f);
        player.GetComponent<SpriteRenderer>().color = Color.red;
        yield return new WaitForSeconds(0.2f);
        enemy.transform.position = originalPos;
        player.GetComponent<SpriteRenderer>().color = Color.white;
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
            if (i < starsEarned)
            {
                victoryStars[i].SetActive(true);
                victoryStars[i].transform.localScale = Vector3.zero;

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
        SaveLevelProgress(levelId, starsEarned);
        StartCoroutine(AnimateStars());

        if (victoryText != null)
            victoryText.text = $"Победа!\nЗвезд: {starsEarned}/3";

        victoryPanel.SetActive(true);
    }

    void SaveLevelProgress(string levelId, int stars)
    {
        int currentSlot = SimpleSlotManager.GetCurrentSlot();
        PlayerPrefs.SetInt($"Slot_{currentSlot}_{levelId}_stars", stars);
        PlayerPrefs.SetInt($"Slot_{currentSlot}_{levelId}_completed", 1);
        PlayerPrefs.Save();
    }

    int CalculateStars()
    {
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
        starsEarned = 1;
        SaveLevelProgress(levelId, starsEarned);
        StartCoroutine(AnimateStars());

        if (victoryText != null)
            victoryText.text = $"Ничья!\nЗвезд: {starsEarned}/3";

        victoryPanel.SetActive(true);
    }

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
}
