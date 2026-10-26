using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class LevelController : MonoBehaviour
{
    [Header("UI Элементы")]
    public TMP_Text questionText;
    public TMP_Text progressText;

    [Header("Кнопки и их индикаторы")]
    public Button[] answerButtons;
    public TMP_Text[] answerTexts;
    public GameObject[] greenChecks;
    public GameObject[] redCrosses;

    [Header("Панель результатов")]
    public GameObject resultPanel;
    public TMP_Text resultText;
    public Button nextButton;

    [Header("Данные уровня")]
    public LevelData levelData;

    [Header("Настройки")]
    public int maxErrors = 3;

    private int currentQuestionIndex = 0;
    private int correctCount = 0;
    private int wrongCount = 0;
    private bool isAnswered = false;

    void Start()
    {
        if (resultPanel != null) resultPanel.SetActive(false);
        ShowQuestion();
    }

    void ShowQuestion()
    {
        if (currentQuestionIndex >= levelData.questions.Length)
        {
            EndLevel(true);
            return;
        }

        isAnswered = false;
        QuestionData q = levelData.questions[currentQuestionIndex];

        questionText.text = q.questionText;

        List<string> shuffledAnswers = new List<string>(q.answers);
        string correctAnswerText = q.answers[q.correctAnswerIndex];

        for (int i = 0; i < shuffledAnswers.Count; i++)
        {
            string temp = shuffledAnswers[i];
            int randomIndex = Random.Range(i, shuffledAnswers.Count);
            shuffledAnswers[i] = shuffledAnswers[randomIndex];
            shuffledAnswers[randomIndex] = temp;
        }

        int newCorrectIndex = shuffledAnswers.IndexOf(correctAnswerText);

        for (int i = 0; i < answerButtons.Length; i++)
        {
            if (i < greenChecks.Length && greenChecks[i] != null) greenChecks[i].SetActive(false);
            if (i < redCrosses.Length && redCrosses[i] != null) redCrosses[i].SetActive(false);

            if (i < shuffledAnswers.Count)
            {
                answerButtons[i].gameObject.SetActive(true);
                answerTexts[i].text = shuffledAnswers[i];

                int index = i;
                answerButtons[i].onClick.RemoveAllListeners();
                answerButtons[i].onClick.AddListener(() => OnAnswerSelected(index, newCorrectIndex));
            }
            else
            {
                answerButtons[i].gameObject.SetActive(false);
            }
        }

        if (progressText != null)
            progressText.text = $"Вопрос {currentQuestionIndex + 1}/{levelData.questions.Length}";
    }

    public void OnAnswerSelected(int index, int correctIndex)
    {
        if (isAnswered) return;
        isAnswered = true;

        if (index == correctIndex)
        {
            correctCount++;
            if (greenChecks.Length > index && greenChecks[index] != null)
                greenChecks[index].SetActive(true);

            if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX("Correct");
        }
        else
        {
            wrongCount++;
            if (redCrosses.Length > index && redCrosses[index] != null)
                redCrosses[index].SetActive(true);

            if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX("Wrong");
        }

        StartCoroutine(NextQuestionDelay());
    }

    IEnumerator NextQuestionDelay()
    {
        yield return new WaitForSeconds(1.5f);

        currentQuestionIndex++;

        if (wrongCount >= maxErrors)
        {
            EndLevel(false);
        }
        else
        {
            ShowQuestion();
        }
    }

    void EndLevel(bool isVictory)
    {
        int levelId = PlayerPrefs.GetInt("CurrentLevel", 1);
        int currentSlot = PlayerPrefs.GetInt("CurrentSaveSlot", 0);

        if (isVictory)
        {
            PlayerPrefs.SetInt($"Slot_{currentSlot}_Level_{levelId}_completed", 1);
            PlayerPrefs.SetInt($"Slot_{currentSlot}_Level_{levelId}_Correct", correctCount);
            PlayerPrefs.SetInt($"Slot_{currentSlot}_Level_{levelId}_Wrong", wrongCount);
            PlayerPrefs.SetInt("JustCompletedLevel", levelId);
            PlayerPrefs.Save();

            StartCoroutine(WaitAndGoToMap());
        }
        else
        {
            if (resultPanel != null)
            {
                resultPanel.SetActive(true);
                if (resultText != null)
                    resultText.text = $"Уровень не пройден!\nОшибок: {wrongCount}";
            }
        }
    }

    IEnumerator WaitAndGoToMap()
    {
        yield return new WaitForSeconds(0.4f);
        SceneManager.LoadScene("LevelMap");
    }

    public void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void BackToMap()
    {
        SceneManager.LoadScene("LevelMap");
    }
}