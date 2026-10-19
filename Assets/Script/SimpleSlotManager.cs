using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

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

    [Header("Level Settings")]
    public int totalLevels = 5;
    public int totalAchievements = 5;

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
            // Создаем новое сохранение
            CreateNewSlot(slotIndex);
        }

        PlayerPrefs.SetInt("CurrentSaveSlot", slotIndex);
        PlayerPrefs.Save();

        Debug.Log($"Выбран слот: {slotIndex}");

        // Переходим СРАЗУ на карту уровней
        SceneManager.LoadScene("Man"); // Убедись, что сцена так называется в Build Settings
    }

    // Удаление слота
    public void DeleteSlot(int slotIndex)
    {
        // Удаляем все данные слотов (уровни и достижения)
        for (int i = 1; i <= totalLevels; i++)
        {
            PlayerPrefs.DeleteKey($"Slot_{slotIndex}_Level_{i}_stars");
            PlayerPrefs.DeleteKey($"Slot_{slotIndex}_Level_{i}_completed");
            PlayerPrefs.DeleteKey($"Slot_{slotIndex}_Level_{i}_Correct");
            PlayerPrefs.DeleteKey($"Slot_{slotIndex}_Level_{i}_Wrong");
        }

        for (int i = 1; i <= totalAchievements; i++)
        {
            PlayerPrefs.DeleteKey($"Slot_{slotIndex}_Achievement_Level_{i}");
        }

        PlayerPrefs.DeleteKey($"Slot_{slotIndex}_Exists");
        PlayerPrefs.DeleteKey($"Slot_{slotIndex}_Name");
        PlayerPrefs.Save();

        UpdateAllSlotsDisplay();
        Debug.Log($"Слот {slotIndex} полностью очищен");
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
            int completedAchievements = CountAchievements(slotIndex);

            slotUI.slotNameText.text = slotName;
            slotUI.progressText.text = $"Уровней: {completedLevels}/{totalLevels}\nДостижений: {completedAchievements}/{totalAchievements}";

            if (slotUI.deleteButton != null)
                slotUI.deleteButton.SetActive(true);
        }
        else
        {
            slotUI.slotNameText.text = $"Сохранение {slotIndex + 1}";
            slotUI.progressText.text = "Новое сохранение";

            if (slotUI.deleteButton != null)
                slotUI.deleteButton.SetActive(false);
        }
    }

    // Подсчет пройденных уровней
    int CountCompletedLevels(int slotIndex)
    {
        int count = 0;
        for (int i = 1; i <= totalLevels; i++)
        {
            if (PlayerPrefs.GetInt($"Slot_{slotIndex}_Level_{i}_completed", 0) == 1)
                count++;
        }
        return count;
    }

    // Подсчет полученных достижений
    int CountAchievements(int slotIndex)
    {
        int count = 0;
        for (int i = 1; i <= totalAchievements; i++)
        {
            if (PlayerPrefs.GetInt($"Slot_{slotIndex}_Achievement_Level_{i}", 0) == 1)
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