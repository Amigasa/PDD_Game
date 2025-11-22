using UnityEngine;

[System.Serializable]
public class DifficultySettings
{
    public int minNumber = 2;
    public int maxNumber = 10;
    public int wrongAnswerRange = 5;
    public int totalQuestions = 5;
    public bool useTrickyAnswers = false;
    public float timeBonusThreshold = 4f; // Секунд для бонуса
}

public class ProgressiveLevelManager : MonoBehaviour
{
    [Header("Настройки сложности для 8 уровней")]
    public DifficultySettings[] difficultyLevels = new DifficultySettings[]
    {
        // Уровень 1: Знакомство
        new DifficultySettings {
            minNumber = 2,
            maxNumber = 5,
            wrongAnswerRange = 2,
            totalQuestions = 5,
            useTrickyAnswers = false,
            timeBonusThreshold = 5f
        },
        
        // Уровень 2: Основы
        new DifficultySettings {
            minNumber = 2,
            maxNumber = 6,
            wrongAnswerRange = 3,
            totalQuestions = 6,
            useTrickyAnswers = false,
            timeBonusThreshold = 4.5f
        },
        
        // Уровень 3: Развитие
        new DifficultySettings {
            minNumber = 3,
            maxNumber = 7,
            wrongAnswerRange = 3,
            totalQuestions = 6,
            useTrickyAnswers = true,
            timeBonusThreshold = 4f
        },
        
        // Уровень 4: Уверенность
        new DifficultySettings {
            minNumber = 4,
            maxNumber = 8,
            wrongAnswerRange = 4,
            totalQuestions = 7,
            useTrickyAnswers = true,
            timeBonusThreshold = 3.5f
        },
        
        // Уровень 5: Сложность
        new DifficultySettings {
            minNumber = 5,
            maxNumber = 9,
            wrongAnswerRange = 4,
            totalQuestions = 7,
            useTrickyAnswers = true,
            timeBonusThreshold = 3f
        },
        
        // Уровень 6: Мастерство
        new DifficultySettings {
            minNumber = 6,
            maxNumber = 9,
            wrongAnswerRange = 5,
            totalQuestions = 8,
            useTrickyAnswers = true,
            timeBonusThreshold = 2.5f
        },
        
        // Уровень 7: Эксперт
        new DifficultySettings {
            minNumber = 7,
            maxNumber = 10,
            wrongAnswerRange = 5,
            totalQuestions = 8,
            useTrickyAnswers = true,
            timeBonusThreshold = 2f
        },
        
        // Уровень 8: Грандмастер
        new DifficultySettings {
            minNumber = 8,
            maxNumber = 10,
            wrongAnswerRange = 6,
            totalQuestions = 10,
            useTrickyAnswers = true,
            timeBonusThreshold = 1.5f
        }
    };

    public DifficultySettings GetDifficultyForLevel(string levelId)
    {
        int levelNumber = ExtractLevelNumber(levelId);
        int difficultyIndex = Mathf.Clamp(levelNumber - 1, 0, difficultyLevels.Length - 1);
        return difficultyLevels[difficultyIndex];
    }

    int ExtractLevelNumber(string levelId)
    {
        foreach (char c in levelId)
        {
            if (char.IsDigit(c))
            {
                return int.Parse(c.ToString());
            }
        }
        return 1;
    }
}
