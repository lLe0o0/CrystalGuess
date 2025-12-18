using UnityEngine;

[CreateAssetMenu(fileName = "GameSettings", menuName = "Mastermind/Game Settings")]
public class GameSettings : ScriptableObject
{
    [Header("Game Configuration")]
    public int codeLength = 4;
    public int numberOfColors = 2;
    public int maxGuesses = 2;

    [Header("Time Settings")]
    public float baseTime = 60.0f;
}