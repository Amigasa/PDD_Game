using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class KnopkiMenu : MonoBehaviour
{
    [SerializeField] private GameObject MenuPanel;//Панель главного меню
    [SerializeField] private GameObject RejimiPanel;//Панель режимов

    void Update()
    {

    }
    public void Play()//Публичная функция для кнопки Играть
    {
        MenuPanel.SetActive(false);//Отключается панель главного меню
        RejimiPanel.SetActive(true);//Включаются режимы
    }
    public void Ex()//Кнопка назад для выхода в меню
    {
        RejimiPanel.SetActive(false);//Отключается панель режимов
        MenuPanel.SetActive(true);//Включается главное меню
    }
    public void Exit()//Кнопка выхода в главном меню
    {
        Application.Quit();//Полностью выход из игры
    }
    public void Man()//Для кнопки битва
    {
        SceneManager.LoadScene(1);//Запускается сцена игры
    }
    public void Woman()//Для кнопки разукрашка
    {
        SceneManager.LoadScene(2);//Запускается сцена игры
    }
    public void ExMenu()//Для кнопки выхода в меню
    {
        SceneManager.LoadScene(0);//Запускается сцена меню
    }
}
