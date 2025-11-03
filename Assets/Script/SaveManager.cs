using UnityEngine;

[System.Serializable]
public class LevelProgress
{
    public int starsEarned;
    public bool isCompleted;
}

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SaveLevelProgress(string levelId, int stars)
    {
        PlayerPrefs.SetInt(levelId + "_stars", stars);
        PlayerPrefs.SetInt(levelId + "_completed", 1);
        PlayerPrefs.Save();

        Debug.Log($"Сохранен прогресс: Уровень {levelId}, Звезд: {stars}");
    }

    public LevelProgress LoadLevelProgress(string levelId)
    {
        LevelProgress progress = new LevelProgress();
        progress.starsEarned = PlayerPrefs.GetInt(levelId + "_stars", 0);
        progress.isCompleted = PlayerPrefs.GetInt(levelId + "_completed", 0) == 1;
        return progress;
    }
}
