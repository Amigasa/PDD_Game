using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class KnopkiMenu : MonoBehaviour
{
    [SerializeField] private GameObject SavePanel;
    [SerializeField] private GameObject MenuPanel;
    [SerializeField] private GameObject RejimiPanel;
    [SerializeField] private GameObject galleryPanel;

    // Добавь эти методы:
    public void ShowGallery()
    {
        MenuPanel.SetActive(false);
        galleryPanel.SetActive(true);
    }

    public void BackFromGallery()
    {
        galleryPanel.SetActive(false);
        MenuPanel.SetActive(true);
    }
    void Update()
    {

    }
    public void Ex()//Кнопка назад для выхода в меню
    {
        RejimiPanel.SetActive(false);
        SavePanel.SetActive(true);
    }
    public void Exit()//Кнопка выхода в главном меню
    {
        Application.Quit();//Полностью выход из игры
    }
    
}
