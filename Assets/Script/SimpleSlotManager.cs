using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SimpleSlotManager : MonoBehaviour
{
    [System.Serializable]
    public class SaveSlotUI
    {
        public Button slotButton;
        public TMP_Text slotNameText;
        public TMP_Text progressText;
        public GameObject deleteButton;
    }

    [Header("UI References")]
    public SaveSlotUI[] saveSlots;
    public GameObject mainMenuPanel;
    public GameObject saveSelectionPanel;

    void Start()
    {
        UpdateAllSlotsDisplay();
    }

    // Показывает меню выбора слотов
    public void ShowSaveSelection()
    {
        mainMenuPanel.SetActive(false);
        saveSelectionPanel.SetActive(true);
        UpdateAllSlotsDisplay();
    }

    // Возврат в главное меню
    public void BackToMainMenu()
    {
        saveSelectionPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }

    // Выбор слота сохранения
    public void SelectSlot(int slotIndex)
    {
        bool slotExists = PlayerPrefs.GetInt($"Slot_{slotIndex}_Exists", 0) == 1;

        if (!slotExists)
        {
            CreateNewSlot(slotIndex);
        }

        PlayerPrefs.SetInt("CurrentSaveSlot", slotIndex);
        PlayerPrefs.Save();

        // Переходим к карте уровней
        saveSelectionPanel.SetActive(false);
        UnityEngine.SceneManagement.SceneManager.LoadScene("LevelMap"); // или панель
    }

    // Удаление слота
    public void DeleteSlot(int slotIndex)
    {
        // Удаляем прогресс уровней
        for (int i = 1; i <= 5; i++) // 5 уровней
        {
            PlayerPrefs.DeleteKey($"Slot_{slotIndex}_Level_{i}_Completed");
            PlayerPrefs.DeleteKey($"Slot_{slotIndex}_Level_{i}_Correct");
            PlayerPrefs.DeleteKey($"Slot_{slotIndex}_Level_{i}_Wrong");
        }

        // Удаляем достижения
        for (int i = 1; i <= 5; i++)
        {
            PlayerPrefs.DeleteKey($"Slot_{slotIndex}_Achievement_Level_{i}");
        }

        PlayerPrefs.DeleteKey($"Slot_{slotIndex}_Exists");
        PlayerPrefs.DeleteKey($"Slot_{slotIndex}_Name");
        PlayerPrefs.Save();

        UpdateAllSlotsDisplay();
    }

    // Создание нового слота
    void CreateNewSlot(int slotIndex)
    {
        PlayerPrefs.SetInt($"Slot_{slotIndex}_Exists", 1);
        PlayerPrefs.SetString($"Slot_{slotIndex}_Name", $"Сохранение {slotIndex + 1}");
        PlayerPrefs.Save();
    }

    // Обновление отображения всех слотов
    void UpdateAllSlotsDisplay()
    {
        for (int i = 0; i < saveSlots.Length; i++)
        {
            UpdateSlotDisplay(i);
        }
    }

    // Обновление отображения конкретного слота
    void UpdateSlotDisplay(int slotIndex)
    {
        bool slotExists = PlayerPrefs.GetInt($"Slot_{slotIndex}_Exists", 0) == 1;
        SaveSlotUI slotUI = saveSlots[slotIndex];

        if (slotExists)
        {
            string slotName = PlayerPrefs.GetString($"Slot_{slotIndex}_Name", $"Сохранение {slotIndex + 1}");
            int completedLevels = CountCompletedLevels(slotIndex);

            slotUI.slotNameText.text = slotName;
            slotUI.progressText.text = $"Уровней: {completedLevels}/5";
            slotUI.deleteButton.SetActive(true);
        }
        else
        {
            slotUI.slotNameText.text = $"Сохранение {slotIndex + 1}";
            slotUI.progressText.text = "Новое сохранение";
            slotUI.deleteButton.SetActive(false);
        }
    }

    // Подсчет пройденных уровней
    int CountCompletedLevels(int slotIndex)
    {
        int count = 0;
        for (int i = 1; i <= 5; i++)
        {
            if (PlayerPrefs.GetInt($"Slot_{slotIndex}_Level_{i}_Completed", 0) == 1)
                count++;
        }
        return count;
    }

    // Получить текущий выбранный слот
    public static int GetCurrentSlot()
    {
        return PlayerPrefs.GetInt("CurrentSaveSlot", 0);
    }
}