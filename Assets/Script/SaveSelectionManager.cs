using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveSelectionManager : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject mainMenuPanel;
    public GameObject saveSelectionPanel;

    void Start()
    {
        SlotSaveSystem.Instance.UpdateAllSlotsDisplay();
    }

    public void ShowSaveSelection()
    {
        mainMenuPanel.SetActive(false);
        saveSelectionPanel.SetActive(true);
        SlotSaveSystem.Instance.UpdateAllSlotsDisplay();
    }

    public void BackToMainMenu()
    {
        saveSelectionPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }

    public void OnSlotSelected(int slotIndex)
    {
        bool slotExists = PlayerPrefs.GetInt($"Slot_{slotIndex}_Exists", 0) == 1;

        if (!slotExists)
        {
            // Создаем новый слот
            SlotSaveSystem.Instance.CreateNewSlot(slotIndex);
        }

        // Выбираем этот слот
        SlotSaveSystem.Instance.SelectSlot(slotIndex);

        // Обновляем отображение
        SlotSaveSystem.Instance.UpdateAllSlotsDisplay();

        // Переходим к выбору режима
        saveSelectionPanel.SetActive(false);
        // Здесь загружаем сцену выбора режима или уровней
        SceneManager.LoadScene("ModeSelection");
    }

    public void OnDeleteSlot(int slotIndex)
    {
        SlotSaveSystem.Instance.DeleteSlot(slotIndex);
    }
}
