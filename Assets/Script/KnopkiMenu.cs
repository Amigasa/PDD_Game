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
    public void Man_1_lvl()//Для кнопки 1 лвл битва
    {
        SceneManager.LoadScene(3);//Запускается сцена игры
    }
    public void Man_2_lvl()//Для кнопки 2 лвл битва
    {
        SceneManager.LoadScene(4);//Запускается сцена игры
    }
    public void Woman_1_lvl()//Для кнопки 1 лвл разукрашка
    {
        SceneManager.LoadScene(5);//Запускается сцена игры
    }
    public void Woman_2_lvl()//Для кнопки 2 лвл разукрашка
    {
        SceneManager.LoadScene(6);//Запускается сцена игры
    }
    public void OnRestartButton()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex
        );
    }
}
