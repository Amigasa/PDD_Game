using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GalleryManager : MonoBehaviour
{
    [Header("Галерея")]
    public GameObject galleryPanel;
    public GameObject mainMenuPanel;
    public Button[] galleryButtons; // 4 кнопки с изображениями
    public GameObject[] lockIcons; // 4 иконки замков
    public TMP_Text emptyText; // "Пока нет рисунков"

    [Header("Спрайты рисунков")]
    public Sprite[] drawingSprites; // 4 спрайта рисунков

    void Start()
    {
        UpdateGallery();
    }

    public void BackToModeSelection()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }

    void UpdateGallery()
    {
        int currentSlot = PlayerPrefs.GetInt("CurrentSaveSlot", 0);
        bool hasAnyDrawing = false;

        // Проверяем каждый рисунок
        for (int i = 0; i < galleryButtons.Length; i++)
        {
            string levelKey = $"Slot_{currentSlot}_Gallery_Coloring_Level_{i + 1}";
            bool isUnlocked = PlayerPrefs.GetInt(levelKey, 0) == 1;

            if (isUnlocked)
            {
                // Рисунок разблокирован
                galleryButtons[i].GetComponent<Image>().sprite = drawingSprites[i];
                galleryButtons[i].GetComponent<Image>().color = Color.white;
                lockIcons[i].SetActive(false);
                hasAnyDrawing = true;
            }
            else
            {
                // Рисунок заблокирован
                galleryButtons[i].GetComponent<Image>().sprite = drawingSprites[i];
                galleryButtons[i].GetComponent<Image>().color = Color.black; // Черно-белый
                lockIcons[i].SetActive(true);
            }
        }

        // Показываем текст если нет ни одного рисунка
        if (emptyText != null)
        {
            emptyText.gameObject.SetActive(!hasAnyDrawing);
        }
    }

    // Просмотр рисунка (просто выводим в консоль)
    public void ViewDrawing(int drawingIndex)
    {
        int currentSlot = PlayerPrefs.GetInt("CurrentSaveSlot", 0);
        string levelKey = $"Slot_{currentSlot}_Gallery_Coloring_Level_{drawingIndex + 1}";

        if (PlayerPrefs.GetInt(levelKey, 0) == 1)
        {
            Debug.Log($"Просматриваем рисунок {drawingIndex + 1}");
            // Здесь можно добавить полноэкранный просмотр
        }
        else
        {
            Debug.Log("Этот рисунок еще не разблокирован!");
        }
    }

}