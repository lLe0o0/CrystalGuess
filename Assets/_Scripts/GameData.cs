using System.Collections.Generic;

[System.Serializable]
public class PlayerProfile
{
    public string playerName;
    public int level = 1;
    public int totalPoints = 0;
    public int highScore = 0;
    public bool hasMapSkin = false;
    public bool tutorialSeen = false;

    public PlayerProfile(string name)
    {
        playerName = name;
        level = 1;
        totalPoints = 0;
        highScore = 0;
        hasMapSkin = false;
        tutorialSeen = false;
    }
}

[System.Serializable]
public class SaveData
{
    public List<PlayerProfile> profiles = new List<PlayerProfile>();
    public string lastActiveProfileName = "";
}