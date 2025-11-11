using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GalleryManager : MonoBehaviour
{
    [Header("Gallery UI")]
    public GameObject galleryPanel;
    public GameObject mainMenuPanel;
    public Transform galleryGrid;
    public GameObject imagePreviewPrefab;

    [Header("Gallery Settings")]
    public Sprite[] coloringSprites; // Спрайты всех рисунков

    void Start()
    {
        UpdateGallery();
    }

    public void ShowGallery()
    {
        mainMenuPanel.SetActive(false);
        galleryPanel.SetActive(true);
        UpdateGallery();
    }

    public void BackToMenu()
    {
        galleryPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }

    void UpdateGallery()
    {
        // Очищаем старые превью
        foreach (Transform child in galleryGrid)
        {
            Destroy(child.gameObject);
        }

        int currentSlot = PlayerPrefs.GetInt("CurrentSaveSlot", 0);

        // Создаем превью для каждого рисунка
        for (int i = 0; i < coloringSprites.Length; i++)
        {
            string levelId = $"Coloring_Level_{i + 1}";
            bool isUnlocked = PlayerPrefs.GetInt($"Slot_{currentSlot}_Gallery_{levelId}", 0) == 1;

            CreateImagePreview(coloringSprites[i], isUnlocked, levelId);
        }
    }

    void CreateImagePreview(Sprite sprite, bool isUnlocked, string levelId)
    {
        GameObject preview = Instantiate(imagePreviewPrefab, galleryGrid);
        Image previewImage = preview.GetComponent<Image>();
        Image lockOverlay = preview.transform.Find("LockOverlay")?.GetComponent<Image>();

        if (previewImage != null)
        {
            previewImage.sprite = sprite;

            if (!isUnlocked)
            {
                previewImage.color = Color.black; // Черно-белое для заблокированных
                if (lockOverlay != null) lockOverlay.gameObject.SetActive(true);
            }
            else
            {
                previewImage.color = Color.white; // Полноцветное для разблокированных
                if (lockOverlay != null) lockOverlay.gameObject.SetActive(false);
            }
        }

        // Добавляем обработчик клика
        Button button = preview.GetComponent<Button>();
        if (button != null && isUnlocked)
        {
            button.onClick.AddListener(() => ShowFullImage(sprite));
        }
    }

    void ShowFullImage(Sprite sprite)
    {
        // Реализуй полноэкранный просмотр изображения
        Debug.Log($"Показываем полноэкранно: {sprite.name}");
    }
}
