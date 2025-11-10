using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SimpleSaveManager : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject MenuPanel;
    public GameObject SavePanel;
    public GameObject RejimiPanel;

    [Header("Save Slots")]
    public SaveSlotUI[] saveSlots;

    void Start()
    {
        LoadAllSaveData();
    }

    // Кнопка "Начать игру" в главном меню
    public void ShowSaveSelection()
    {
        MenuPanel.SetActive(false);
        SavePanel.SetActive(true);
        LoadAllSaveData();
    }

    // Кнопка "Назад"
    public void HideSaveSelection()
    {
        SavePanel.SetActive(false);
        MenuPanel.SetActive(true);
    }

    void LoadAllSaveData()
    {
        for (int i = 0; i < saveSlots.Length; i++)
        {
            UpdateSlotDisplay(i);
        }
    }

    void UpdateSlotDisplay(int slotIndex)
    {
        bool hasSave = PlayerPrefs.GetInt($"SaveSlot_{slotIndex}_Exists", 0) == 1;

        if (hasSave)
        {
            // Есть сохранение - показываем прогресс
            string saveName = PlayerPrefs.GetString($"SaveSlot_{slotIndex}_Name", $"Сохранение {slotIndex + 1}");
            int levelsCompleted = CountCompletedLevels(slotIndex);
            int totalStars = CountTotalStars(slotIndex);

            saveSlots[slotIndex].slotText.text = saveName;
            saveSlots[slotIndex].progressText.text = $"Уровней: {levelsCompleted}/8";
            saveSlots[slotIndex].deleteButton.SetActive(true);
        }
        else
        {
            // Нет сохранения - показываем "Новое сохранение"
            saveSlots[slotIndex].slotText.text = $"Сохранение {slotIndex + 1}";
            saveSlots[slotIndex].progressText.text = "Новое сохранение";
            saveSlots[slotIndex].deleteButton.SetActive(false);
        }
    }

    int CountCompletedLevels(int slotIndex)
    {
        int completed = 0;
        for (int i = 1; i <= 17; i++)
        {
            if (PlayerPrefs.GetInt($"SaveSlot_{slotIndex}_Level_{i}_Completed", 0) == 1)
                completed++;
        }
        return completed;
    }

    int CountTotalStars(int slotIndex)
    {
        int stars = 0;
        for (int i = 1; i <= 17; i++)
        {
            stars += PlayerPrefs.GetInt($"SaveSlot_{slotIndex}_Level_{i}_Stars", 0);
        }
        return stars;
    }

    // Нажатие на слот сохранения
    public void OnSaveSlotClicked(int slotIndex)
    {
        bool hasSave = PlayerPrefs.GetInt($"SaveSlot_{slotIndex}_Exists", 0) == 1;

        if (!hasSave)
        {
            // Создаем новое сохранение
            PlayerPrefs.SetInt($"SaveSlot_{slotIndex}_Exists", 1);
            PlayerPrefs.SetString($"SaveSlot_{slotIndex}_Name", $"Сохранение {slotIndex + 1}");
            PlayerPrefs.Save();
        }

        // Устанавливаем текущий слот и переходим к игре
        PlayerPrefs.SetInt("CurrentSaveSlot", slotIndex);
        PlayerPrefs.Save();

        SavePanel.SetActive(false);//Отключается панель главного меню
        RejimiPanel.SetActive(true);//Включаются режимы
    }

    // Удаление сохранения
    public void OnDeleteSaveClicked(int slotIndex)
    {
        // Удаляем все данные этого слота
        for (int i = 1; i <= 17; i++)
        {
            PlayerPrefs.DeleteKey($"SaveSlot_{slotIndex}_Level_{i}_Completed");
            PlayerPrefs.DeleteKey($"SaveSlot_{slotIndex}_Level_{i}_Stars");
        }
        PlayerPrefs.DeleteKey($"SaveSlot_{slotIndex}_Exists");
        PlayerPrefs.DeleteKey($"SaveSlot_{slotIndex}_Name");
        PlayerPrefs.Save();

        // Обновляем отображение
        UpdateSlotDisplay(slotIndex);
    }
}

[System.Serializable]
public class SaveSlotUI
{
    public Button slotButton;
    public TMP_Text slotText;
    public TMP_Text progressText;
    public GameObject deleteButton;
}