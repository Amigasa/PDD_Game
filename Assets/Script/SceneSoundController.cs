using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSoundController : MonoBehaviour
{
    void Start()
    {
        PlaySceneMusic();
    }

    void PlaySceneMusic()
    {
        if (AudioManager.Instance == null) return;

        string sceneName = SceneManager.GetActiveScene().name;

        switch (sceneName)
        {
            case "MainMenu":
                AudioManager.Instance.PlayMainMenuMusic();
                break;
            case "Gallery":
                AudioManager.Instance.PlayGalleryMusic();
                break;
            case "Tutorial":
                AudioManager.Instance.PlayTutorialMusic();
                break;
            case "BattleMap":
                AudioManager.Instance.PlayBattleMapMusic();
                break;
            case "ColoringMap":
                AudioManager.Instance.PlayColoringMapMusic();
                break;
            case "BattleLevel":
            case "BattleLevel1":
            case "BattleLevel2":
            case "BattleLevel3":
            case "BattleLevel4":
            case "BattleLevel5":
            case "BattleLevel6":
            case "BattleLevel7":
            case "BattleLevel8":
                AudioManager.Instance.PlayBattleGameMusic();
                break;
            case "ColoringLevel":
            case "ColoringLevel1":
            case "ColoringLevel2":
            case "ColoringLevel3":
            case "ColoringLevel4":
            case "ColoringLevel5":
            case "ColoringLevel6":
            case "ColoringLevel7":
            case "ColoringLevel8":
                AudioManager.Instance.PlayColoringGameMusic();
                break;
        }
    }
}
