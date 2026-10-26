using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class LevelSelectionManager : MonoBehaviour
{
    [System.Serializable]
    public class LevelInfo
    {
        public int sceneBuildIndex;
        public Button levelButton;
        public Image buttonImage;
        public TMP_Text levelText;

        [Header("Спрайты кнопки")]
        public Sprite grayButtonSprite;
        public Sprite redButtonSprite;
        public Sprite redPressedSprite;
        public Sprite greenButtonSprite;
    }

    [Header("Level Settings")]
    public LevelInfo[] levels;

    [Header("Звуки")]
    public AudioSource audioSource;
    public AudioClip buttonPressSound;
    public AudioClip buttonGreenSound;

    void Start()
    {
        RefreshAllLevels();

        int justCompleted = PlayerPrefs.GetInt("JustCompletedLevel", -1);

        if (justCompleted > 0)
        {
            int levelIndex = justCompleted - 1;
            if (levelIndex >= 0 && levelIndex < levels.Length)
            {
                levels[levelIndex].buttonImage.sprite = levels[levelIndex].redPressedSprite;
                StartCoroutine(AnimateButtonPress(levelIndex));
            }

            PlayerPrefs.DeleteKey("JustCompletedLevel");
            PlayerPrefs.Save();
        }
    }

    public void RefreshAllLevels()
    {
        for (int i = 0; i < levels.Length; i++)
        {
            UpdateLevelDisplay(i);
        }
    }

    void UpdateLevelDisplay(int levelIndex)
    {
        LevelInfo level = levels[levelIndex];
        int currentSlot = PlayerPrefs.GetInt("CurrentSaveSlot", 0);

        bool completed = PlayerPrefs.GetInt($"Slot_{currentSlot}_Level_{levelIndex + 1}_completed", 0) == 1;
        bool unlocked = IsLevelUnlocked(levelIndex);

        if (level.levelText != null)
            level.levelText.text = (levelIndex + 1).ToString();

        if (level.buttonImage != null)
        {
            if (!unlocked)
                level.buttonImage.sprite = level.grayButtonSprite;
            else if (completed)
                level.buttonImage.sprite = level.greenButtonSprite;
            else
                level.buttonImage.sprite = level.redButtonSprite;
        }

        if (level.levelButton != null)
            level.levelButton.interactable = unlocked;
    }

    IEnumerator AnimateButtonPress(int levelIndex)
    {
        LevelInfo level = levels[levelIndex];
        if (level.buttonImage == null) yield break;

        if (audioSource != null && buttonPressSound != null)
            audioSource.PlayOneShot(buttonPressSound);

        yield return new WaitForSeconds(0.4f);

        level.buttonImage.sprite = level.greenButtonSprite;
        if (audioSource != null && buttonGreenSound != null)
            audioSource.PlayOneShot(buttonGreenSound);
    }

    bool IsLevelUnlocked(int levelIndex)
    {
        if (levelIndex == 0) return true;
        int currentSlot = PlayerPrefs.GetInt("CurrentSaveSlot", 0);
        return PlayerPrefs.GetInt($"Slot_{currentSlot}_Level_{levelIndex}_completed", 0) == 1;
    }

    public void LoadLevel(int levelIndex)
    {
        if (levelIndex < 0 || levelIndex >= levels.Length) return;
        if (!IsLevelUnlocked(levelIndex)) return;

        StartCoroutine(LoadLevelWithAnimation(levelIndex));
    }

    IEnumerator LoadLevelWithAnimation(int levelIndex)
    {
        LevelInfo level = levels[levelIndex];

        if (level.buttonImage != null)
            level.buttonImage.sprite = level.redPressedSprite;

        if (audioSource != null && buttonPressSound != null)
            audioSource.PlayOneShot(buttonPressSound);

        yield return new WaitForSeconds(0.3f);

        PlayerPrefs.SetInt("CurrentLevel", levelIndex + 1);
        PlayerPrefs.Save();

        SceneManager.LoadScene(levels[levelIndex].sceneBuildIndex);
    }

    public void BackToMenu()
    {
        SceneManager.LoadScene(0);
    }
}