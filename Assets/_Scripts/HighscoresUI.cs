using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class HighscoresUI : MonoBehaviour
{
    public Transform highscoreContent;
    public GameObject highscoreRowPrefab; 
    public GameObject highscorePanel;

    public void ShowHighscores()
    {
        highscorePanel.SetActive(true);
        RefreshHighscores();
    }

    public void HideHighscores()
    {
        highscorePanel.SetActive(false);
    }

    private void RefreshHighscores()
    {
        foreach (Transform child in highscoreContent) Destroy(child.gameObject);

        List<PlayerProfile> scores = GameManager.Instance.GetHighScores();
        
        int count = 0;
        foreach (var p in scores)
        {
            if (count >= 10) break;
            
            GameObject row = Instantiate(highscoreRowPrefab, highscoreContent);
            
            TextMeshProUGUI[] texts = row.GetComponentsInChildren<TextMeshProUGUI>();
            
            if (texts.Length >= 2)
            {
                texts[0].text = (count + 1) + ". " + p.playerName;
                texts[1].text = p.highScore.ToString() + " pkt";
            }
            else if (texts.Length == 1)
            {
                texts[0].text = $"{count + 1}. {p.playerName} - {p.highScore} pkt";
            }
            
            count++;
        }
    }
}