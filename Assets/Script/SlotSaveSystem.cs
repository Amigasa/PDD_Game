using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class SlotSaveSystem : MonoBehaviour
{
    public static SlotSaveSystem Instance;

    [Header("Save Slots UI")]
    public SaveSlotUI[] saveSlots;

    void Awake()
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

    // ========== ОБЩИЕ МЕТОДЫ СОХРАНЕНИЯ ==========

    public static void SaveLevelProgress(string levelId, int stars)
    {
        int currentSlot = PlayerPrefs.GetInt("CurrentSaveSlot", 0);
        PlayerPrefs.SetInt($"Slot_{currentSlot}_{levelId}_Stars", stars);
        PlayerPrefs.SetInt($"Slot_{currentSlot}_{levelId}_Completed", 1);
        PlayerPrefs.Save();
    }

    public static LevelProgress LoadLevelProgress(string levelId)
    {
        int currentSlot = PlayerPrefs.GetInt("CurrentSaveSlot", 0);
        LevelProgress progress = new LevelProgress();
        progress.starsEarned = PlayerPrefs.GetInt($"Slot_{currentSlot}_{levelId}_Stars", 0);
        progress.isCompleted = PlayerPrefs.GetInt($"Slot_{currentSlot}_{levelId}_Completed", 0) == 1;
        return progress;
    }

    public static int GetCurrentSlot()
    {
        return PlayerPrefs.GetInt("CurrentSaveSlot", 0);
    }

    // ========== УПРАВЛЕНИЕ СЛОТАМИ ==========

    public void CreateNewSlot(int slotIndex)
    {
        // Помечаем слот как существующий
        PlayerPrefs.SetInt($"Slot_{slotIndex}_Exists", 1);
        PlayerPrefs.SetString($"Slot_{slotIndex}_Name", $"Сохранение {slotIndex + 1}");

        // Очищаем весь прогресс уровня для этого слота
        ClearSlotProgress(slotIndex);

        PlayerPrefs.Save();
        UpdateSlotDisplay(slotIndex);
    }

    public void DeleteSlot(int slotIndex)
    {
        // Полностью очищаем все данные слота
        ClearSlotProgress(slotIndex);
        PlayerPrefs.DeleteKey($"Slot_{slotIndex}_Exists");
        PlayerPrefs.DeleteKey($"Slot_{slotIndex}_Name");
        PlayerPrefs.Save();

        UpdateSlotDisplay(slotIndex);
    }

    void ClearSlotProgress(int slotIndex)
    {
        // Очищаем прогресс всех уровней для этого слота
        for (int i = 1; i <= 17; i++)
        {
            PlayerPrefs.DeleteKey($"Slot_{slotIndex}_Battle_Level_{i}_Stars");
            PlayerPrefs.DeleteKey($"Slot_{slotIndex}_Battle_Level_{i}_Completed");
        }
    }

    public void SelectSlot(int slotIndex)
    {
        PlayerPrefs.SetInt("CurrentSaveSlot", slotIndex);
        PlayerPrefs.Save();
    }

    // ========== ОБНОВЛЕНИЕ UI ==========

    public void UpdateAllSlotsDisplay()
    {
        for (int i = 0; i < saveSlots.Length; i++)
        {
            UpdateSlotDisplay(i);
        }
    }

    void UpdateSlotDisplay(int slotIndex)
    {
        bool slotExists = PlayerPrefs.GetInt($"Slot_{slotIndex}_Exists", 0) == 1;
        SaveSlotUI slotUI = saveSlots[slotIndex];

        if (slotExists)
        {
            // Показываем прогресс сохранения
            string slotName = PlayerPrefs.GetString($"Slot_{slotIndex}_Name", $"Сохранение {slotIndex + 1}");
            int completedLevels = CountCompletedLevels(slotIndex);
            int totalStars = CountTotalStars(slotIndex);

            slotUI.slotNameText.text = slotName;
            slotUI.progressText.text = $"Уровней: {completedLevels}/17\nЗвезд: {totalStars}/51";
            slotUI.deleteButton.SetActive(true);

            // Визуальное выделение выбранного слота
            bool isCurrentSlot = GetCurrentSlot() == slotIndex;
            slotUI.backgroundImage.color = isCurrentSlot ? Color.green : Color.white;
        }
        else
        {
            // Показываем пустой слот
            slotUI.slotNameText.text = $"Сохранение {slotIndex + 1}";
            slotUI.progressText.text = "Новое сохранение";
            slotUI.deleteButton.SetActive(false);
            slotUI.backgroundImage.color = Color.gray;
        }
    }

    int CountCompletedLevels(int slotIndex)
    {
        int count = 0;
        for (int i = 1; i <= 17; i++)
        {
            if (PlayerPrefs.GetInt($"Slot_{slotIndex}_Battle_Level_{i}_Completed", 0) == 1)
                count++;
        }
        return count;
    }

    int CountTotalStars(int slotIndex)
    {
        int total = 0;
        for (int i = 1; i <= 17; i++)
        {
            total += PlayerPrefs.GetInt($"Slot_{slotIndex}_Battle_Level_{i}_Stars", 0);
        }
        return total;
    }
}

[System.Serializable]
public class SaveSlotUI
{
    public Button slotButton;
    public Image backgroundImage;
    public TMP_Text slotNameText;
    public TMP_Text progressText;
    public GameObject deleteButton;
}
