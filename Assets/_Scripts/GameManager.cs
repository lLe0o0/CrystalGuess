using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

public class GameManager : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private GameSettings gameSettings;

    [Header("Active Game Properties")]
    public int ActiveCodeLength { get; private set; }
    public int ActiveNumberOfColors { get; private set; }
    public int ActiveMaxGuesses { get; private set; }

    public int CurrentLevel 
    { 
        get { return activeProfile != null ? activeProfile.level : 1; } 
        private set { if(activeProfile != null) activeProfile.level = value; }
    }
    
    public int TotalPoints 
    { 
        get { return activeProfile != null ? activeProfile.totalPoints : 0; } 
        private set { if(activeProfile != null) activeProfile.totalPoints = value; }
    }
    
    public bool HasMapSkin 
    { 
        get { return activeProfile != null ? activeProfile.hasMapSkin : false; } 
        private set { if(activeProfile != null) activeProfile.hasMapSkin = value; }
    }
    
    public bool TutorialSeen 
    {
        get { return activeProfile != null ? activeProfile.tutorialSeen : false; }
        set { if (activeProfile != null) { activeProfile.tutorialSeen = value; SaveGame(); } }
    }

    public int LastPointsWon { get; private set; }
    public float RemainingTime { get; private set; }
    public GameState CurrentState { get; private set; }
    
    public static event Action<GameState> OnGameStateChanged;
    public static event Action OnProfileChanged;
    public static event Action<List<int>, List<HintState>> OnGuessChecked;
    
    private const string SAVE_KEY = "Mastermind_SaveData_V1";
    private SaveData saveData;
    private PlayerProfile activeProfile;

    private int currentDifficultyTier;
    private int currentGuessCount = 0;
    private bool justLeveledUp = false; 
    private List<int> secretCode;
    
    public static GameManager Instance { get; private set; }

    public enum GameState { Start, Tutorial, GeneratingCode, PlayerTurn, CheckingGuess, Win, Loss }
    public enum HintState { None, ColorOnly, Perfect }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
            LoadGame();
        }
        else Destroy(gameObject);
    }

    private void Update()
    {
        switch (CurrentState)
        {
            case GameState.Start: ChangeState(GameState.GeneratingCode); break;
            case GameState.GeneratingCode: GenerateSecretCode(); ChangeState(GameState.PlayerTurn); break;
            case GameState.PlayerTurn:
                if (RemainingTime > 0)
                {
                    RemainingTime -= Time.deltaTime;
                    if (RemainingTime <= 0)
                    {
                        RemainingTime = 0;
                        ChangeState(GameState.Loss);
                    }
                }
                break;
        }
    }
    
    private void OnDisable() { SceneManager.sceneLoaded -= OnSceneLoaded; }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Game") StartCoroutine(StartGameDelay());
    }

    private IEnumerator StartGameDelay()
    {
        yield return new WaitForEndOfFrame();
        StartNewGame();
    }

    public void CreateProfile(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return;
        if (saveData.profiles.Any(p => p.playerName == name)) return;

        PlayerProfile newProfile = new PlayerProfile(name);
        saveData.profiles.Add(newProfile);
        SelectProfile(name);
    }

    public void SelectProfile(string name)
    {
        activeProfile = saveData.profiles.Find(p => p.playerName == name);
        if (activeProfile != null)
        {
            saveData.lastActiveProfileName = name;
            SaveGame();
            OnProfileChanged?.Invoke();
        }
    }

    public void DeleteProfile(string name)
    {
        PlayerProfile profileToRemove = saveData.profiles.Find(p => p.playerName == name);
        
        if (profileToRemove != null)
        {
            saveData.profiles.Remove(profileToRemove);

            if (activeProfile == profileToRemove)
            {
                activeProfile = null;
                saveData.lastActiveProfileName = "";
                OnProfileChanged?.Invoke();
            }
            SaveGame();
        }
    }

    public List<PlayerProfile> GetAllProfiles() => saveData.profiles;
    public PlayerProfile GetActiveProfile() => activeProfile;
    public List<PlayerProfile> GetHighScores() => saveData.profiles.OrderByDescending(p => p.highScore).ToList();

    private void SaveGame()
    {
        string json = JsonUtility.ToJson(saveData);
        PlayerPrefs.SetString(SAVE_KEY, json);
        PlayerPrefs.Save();
    }

    private void LoadGame()
    {
        if (PlayerPrefs.HasKey(SAVE_KEY))
        {
            string json = PlayerPrefs.GetString(SAVE_KEY);
            saveData = JsonUtility.FromJson<SaveData>(json);
        }
        else
        {
            saveData = new SaveData();
        }

        if (!string.IsNullOrEmpty(saveData.lastActiveProfileName))
        {
            activeProfile = saveData.profiles.Find(p => p.playerName == saveData.lastActiveProfileName);
        }
    }

    public void StartNewGame()
    {
        if (activeProfile == null) CreateProfile("Player");

        currentGuessCount = 0;
        CalculateDifficulty();

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayMusic(AudioManager.Instance.gameMusic);
        }

        if (!TutorialSeen) ChangeState(GameState.Tutorial);
        else ChangeState(GameState.GeneratingCode);
    }
    
    public void CloseTutorial()
    {
        TutorialSeen = true; 
        ChangeState(GameState.GeneratingCode);
    }

    public void RestartGame() { StartNewGame(); }

    public void ChangeState(GameState newState)
    {
        if (CurrentState == newState) return;
        CurrentState = newState;
        OnGameStateChanged?.Invoke(newState);
    }

    private void CalculateDifficulty()
    {
        currentDifficultyTier = (CurrentLevel - 1) / 5; 
        ActiveCodeLength = gameSettings.codeLength;
        ActiveNumberOfColors = gameSettings.numberOfColors;
        ActiveMaxGuesses = gameSettings.maxGuesses;

        ActiveNumberOfColors = Mathf.Min(gameSettings.numberOfColors + currentDifficultyTier, 6); 
        if (currentDifficultyTier >= 2) ActiveCodeLength = Mathf.Min(gameSettings.codeLength + 1, 6);
        ActiveMaxGuesses = Mathf.Max(gameSettings.maxGuesses - currentDifficultyTier, 6); 
        
        RemainingTime = gameSettings.baseTime;
        if (RemainingTime <= 0) RemainingTime = 60;
    }

    private void GenerateSecretCode()
    {
        secretCode = new List<int>();
        for (int i = 0; i < ActiveCodeLength; i++)
        {
            secretCode.Add(UnityEngine.Random.Range(0, ActiveNumberOfColors));
        }
    }

    public void ProcessPlayerGuess(List<int> guess)
    {
        ChangeState(GameState.CheckingGuess);
        
        List<HintState> hints = CheckGuessPositional(guess);
        currentGuessCount++;
        OnGuessChecked?.Invoke(guess, hints);
        
        bool isWin = true;
        foreach(var h in hints) if(h != HintState.Perfect) isWin = false;

        if (isWin)
        {
            int timeMultiplier = 1 + (currentDifficultyTier * 4);
            int timeBonus = Mathf.RoundToInt(RemainingTime * timeMultiplier);
            
            LastPointsWon = 100 + (currentDifficultyTier * 50) + timeBonus;
            
            if(activeProfile != null) 
            {
                activeProfile.totalPoints += LastPointsWon;
                if (activeProfile.totalPoints > activeProfile.highScore)
                {
                    activeProfile.highScore = activeProfile.totalPoints;
                }
            }

            CurrentLevel++;
            justLeveledUp = true; 
            
            SaveGame(); 
            ChangeState(GameState.Win);
        }
        else if (currentGuessCount >= ActiveMaxGuesses)
        {
            ChangeState(GameState.Loss);
        }
        else
        {
            ChangeState(GameState.PlayerTurn);
        }
    }

    private List<HintState> CheckGuessPositional(List<int> playerGuess)
    {
        List<HintState> hints = new List<HintState>();
        for(int i=0; i<ActiveCodeLength; i++) hints.Add(HintState.None);

        List<int> tempSecret = new List<int>(secretCode);
        List<int> tempGuess = new List<int>(playerGuess);

        for (int i = 0; i < ActiveCodeLength; i++)
        {
            if (tempGuess[i] == tempSecret[i])
            {
                hints[i] = HintState.Perfect;
                tempSecret[i] = -1;
                tempGuess[i] = -2;
            }
        }
        for (int i = 0; i < ActiveCodeLength; i++)
        {
            if (hints[i] == HintState.Perfect) continue;
            int foundIndex = tempSecret.IndexOf(tempGuess[i]);
            if (foundIndex != -1)
            {
                hints[i] = HintState.ColorOnly;
                tempSecret[foundIndex] = -1;
            }
        }
        return hints;
    }

    public bool TrySpendPoints(int cost)
    {
        if (TotalPoints >= cost) {
            TotalPoints -= cost; 
            SaveGame();
            return true;
        } 
        return false;
    }
    
    public void UnlockMapSkin() 
    { 
        HasMapSkin = true; 
        SaveGame();
    }

    public bool CheckAndResetLevelUpFlag()
    {
        if (justLeveledUp)
        {
            justLeveledUp = false; 
            return true; 
        }
        return false; 
    }
    
    public void ResetPlayerProgress() 
    {
        if(activeProfile != null)
        {
            activeProfile.level = 1;
            activeProfile.totalPoints = 0;
            activeProfile.hasMapSkin = false;
            activeProfile.tutorialSeen = false;
            SaveGame();
            OnProfileChanged?.Invoke();
        }
    }
}