using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

[System.Serializable]
public class Dialogue
{
    public string speakerName;
    [TextArea(3, 10)]
    public string sentence;
    public bool isCharacter1Speaking; // true - первый персонаж, false - второй
}

public class TutorialManager : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI dialogueText;
    public TextMeshProUGUI speakerNameText;
    public Image character1Image;
    public Image character2Image;
    public Button backButton;

    [Header("Dialogue Settings")]
    public Dialogue[] dialogues;
    public float typingSpeed = 0.05f;

    private int currentDialogueIndex = 0;
    private bool isTyping = false;
    private Coroutine typingCoroutine;

    [Header("Character Colors")]
    public Color activeColor = Color.white;
    public Color inactiveColor = new Color(0.7f, 0.7f, 0.7f, 1f);

    void Start()
    {
        backButton.onClick.AddListener(BackToMainMenu);

        // Ќачинаем первый диалог
        ShowDialogue(currentDialogueIndex);
    }

    void ShowDialogue(int index)
    {
        if (index >= dialogues.Length)
        {
            // ƒиалоги закончились, возвращаем в меню
            BackToMainMenu();
            return;
        }

        Dialogue currentDialogue = dialogues[index];

        // ”станавливаем им€ говор€щего
        speakerNameText.text = currentDialogue.speakerName;

        // ѕодсвечиваем активного персонажа
        if (currentDialogue.isCharacter1Speaking)
        {
            character1Image.color = activeColor;
            character2Image.color = inactiveColor;
        }
        else
        {
            character1Image.color = inactiveColor;
            character2Image.color = activeColor;
        }

        // «апускаем анимацию печати текста
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        typingCoroutine = StartCoroutine(TypeSentence(currentDialogue.sentence));
    }

    IEnumerator TypeSentence(string sentence)
    {
        isTyping = true;
        dialogueText.text = "";

        foreach (char letter in sentence.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
    }

    void ContinueDialogue()
    {
        if (isTyping)
        {
            // ≈сли текст еще печатаетс€, завершить печать
            if (typingCoroutine != null)
                StopCoroutine(typingCoroutine);

            dialogueText.text = dialogues[currentDialogueIndex].sentence;
            isTyping = false;
            return;
        }

        currentDialogueIndex++;
        ShowDialogue(currentDialogueIndex);
    }

    void BackToMainMenu()
    {
        SceneManager.LoadScene(0); // «амените на им€ вашей сцены главного меню
    }

    void Update()
    {
        // ƒобавл€ем возможность продолжать по нажатию пробела или клику мыши
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
        {
            ContinueDialogue();
        }
    }
}
