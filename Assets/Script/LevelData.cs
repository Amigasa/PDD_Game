using UnityEngine;

[CreateAssetMenu(fileName = "LevelData", menuName = "Game/Level Data")]
public class LevelData : ScriptableObject
{
    public QuestionData[] questions;
}

[System.Serializable]
public class QuestionData
{
    [TextArea(2, 5)]
    public string questionText;
    public string[] answers;
    public int correctAnswerIndex; // 0, 1, 2 или 3
}