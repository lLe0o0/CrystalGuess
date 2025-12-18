using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class MainMenu : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI totalPointsText;
    [SerializeField] private GameObject shopPanel; 
    
    private int displayedTotalPoints = 0; 

    private void OnEnable()
    {
        GameManager.OnProfileChanged += UpdatePointsText;
        UpdatePointsText();
    }

    private void OnDisable()
    {
        GameManager.OnProfileChanged -= UpdatePointsText;
    }

    private void Start()
    {
        UpdatePointsText();
        if(shopPanel != null) shopPanel.SetActive(false);
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayMusic(AudioManager.Instance.menuMusic);
        }
    }

    public void StartGame()
    {
        SceneManager.LoadScene("Game");
    }

    public void OpenShop()
    {
        if (shopPanel != null) shopPanel.SetActive(true);
    }
    
    public void CloseShop()
    {
        if(shopPanel != null)
        {
            shopPanel.SetActive(false);
            UpdatePointsText(); 
        }
    }

    public void OnResetProgressButton()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ResetPlayerProgress();
            UpdatePointsText(); 
        }
    }

    public void PlayClickSound()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonSound();
        }
    }

    private void UpdatePointsText()
    {
        if (GameManager.Instance != null && totalPointsText != null)
        {
            int newTotalPoints = GameManager.Instance.TotalPoints;
            StartCoroutine(AnimateNumber(totalPointsText, "PUNKTY: ", displayedTotalPoints, newTotalPoints, 1.0f));
            displayedTotalPoints = newTotalPoints;
        }
        else if (totalPointsText != null)
        {
            totalPointsText.text = "PUNKTY: -";
        }
    }
    
    private IEnumerator AnimateNumber(TextMeshProUGUI textElement, string prefix, int startValue, int endValue, float duration)
    {
        float time = 0;
        while (time < duration)
        {
            int currentValue = (int)Mathf.Lerp(startValue, endValue, time / duration);
            if (textElement != null)
                textElement.text = prefix + currentValue.ToString();
            time += Time.deltaTime;
            yield return null; 
        }
        if (textElement != null)
            textElement.text = prefix + endValue.ToString();
    }
}