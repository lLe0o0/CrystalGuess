using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class UIManager : MonoBehaviour
{
    [Header("Kontenery")]
    public Transform historyContainer; 
    public GameObject guessRowPrefab;  
    public GuessRow activeInputRow;    

    [Header("UI Gry")]
    public List<Button> colorButtons;
    public GameObject winScreen;
    public GameObject lossScreen;
    public GameObject levelUpBanner; 
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI totalPointsText;
    public TextMeshProUGUI pointsWonText;
    public GameObject tutorialPanel;        
    public TextMeshProUGUI timerText; 
    
    [Header("VFX")]
    public GameObject confettiEffectPrefab; 
    public GameObject pegAppearEffectPrefab; 
    public GameObject lossEffectPrefab; 
    public Transform canvasTransform; 

    [Header("Scenery")]
    public GameObject mapSkinBackground; 
    public Camera mainCamera;            

    [Header("Dane")]
    public List<Sprite> pegSprites; 

    private List<int> currentGuess = new List<int>();
    private int displayedTotalPoints = 0; 
    private int currentPegIndex = 0;

    private void OnEnable() {
        GameManager.OnGuessChecked += AddHistoryRow;
        GameManager.OnGameStateChanged += HandleState;
    }
    private void OnDisable() {
        GameManager.OnGuessChecked -= AddHistoryRow;
        GameManager.OnGameStateChanged -= HandleState;
    }

    private void Update()
    {
        if (GameManager.Instance != null && timerText != null)
        {
            float time = GameManager.Instance.RemainingTime;
            if (time < 0) time = 0;

            timerText.text = Mathf.CeilToInt(time).ToString();
            
            if (time <= 10f && time > 0) 
                timerText.color = Color.red;
            else 
                timerText.color = Color.white;
        }
    }

    public void OnColorSelected(int colorIndex)
    {
        if (currentPegIndex < GameManager.Instance.ActiveCodeLength)
        {
            Sprite selectedSprite = pegSprites[colorIndex];

            activeInputRow.SetPegSprite(currentPegIndex, selectedSprite);
            activeInputRow.AnimatePegPop(currentPegIndex);

            if (AudioManager.Instance != null) 
            AudioManager.Instance.PlaySFX(AudioManager.Instance.popSound); 
            
            if (pegAppearEffectPrefab != null && activeInputRow.guessPegs.Count > currentPegIndex)
            {
                Transform pegTransform = activeInputRow.guessPegs[currentPegIndex].transform;
                GameObject effect = Instantiate(pegAppearEffectPrefab, canvasTransform);
                effect.transform.position = pegTransform.position;
            }

            currentGuess.Add(colorIndex);
            currentPegIndex++;
        }
    }

    // --- NOWA METODA UNDO ---
    public void OnUndoButton()
    {
        if (currentPegIndex > 0)
        {
            currentPegIndex--;
            if (currentGuess.Count > currentPegIndex)
            {
                currentGuess.RemoveAt(currentPegIndex);
            }
            activeInputRow.ClearSinglePeg(currentPegIndex);
        }
    }
    // ------------------------

    public void OnSubmitGuess()
    {
        if (currentGuess.Count == GameManager.Instance.ActiveCodeLength)
            GameManager.Instance.ProcessPlayerGuess(currentGuess);
    }

    private void AddHistoryRow(List<int> guess, List<GameManager.HintState> hints)
    {
        GameObject newObj = Instantiate(guessRowPrefab, historyContainer);
        GuessRow newRow = newObj.GetComponent<GuessRow>(); 
        
        newRow.UpdateSlotCount(GameManager.Instance.ActiveCodeLength);
        newRow.SetupHistoryRow(guess, hints, pegSprites);

        activeInputRow.ResetRow();
        currentGuess.Clear();
        currentPegIndex = 0;
    }

    private void HandleState(GameManager.GameState state)
    {
        if (GameManager.Instance != null && mapSkinBackground != null)
        {
            mapSkinBackground.SetActive(GameManager.Instance.HasMapSkin);
        }

        if (tutorialPanel != null)
        {
            tutorialPanel.SetActive(state == GameManager.GameState.Tutorial);
        }

        if (state == GameManager.GameState.Win)
        {
            ShowScreenAnimated(winScreen); 
            lossScreen.SetActive(false);

            if (AudioManager.Instance != null) 
                AudioManager.Instance.PlaySFX(AudioManager.Instance.winSound);
            
            if (confettiEffectPrefab != null && canvasTransform != null)
            {
                GameObject effect = Instantiate(confettiEffectPrefab, canvasTransform);
                effect.transform.localPosition = Vector3.zero;
            }
        }
        else if (state == GameManager.GameState.Loss)
        {
            ShowScreenAnimated(lossScreen); 
            winScreen.SetActive(false);

            if (AudioManager.Instance != null) 
                AudioManager.Instance.PlaySFX(AudioManager.Instance.lossSound);
            
            if (lossEffectPrefab != null && canvasTransform != null)
            {
                GameObject effect = Instantiate(lossEffectPrefab, canvasTransform);
                effect.transform.localPosition = Vector3.zero;
            }
        }
        else
        {
            winScreen.SetActive(false);
            lossScreen.SetActive(false);
        }
        
        activeInputRow.gameObject.SetActive(state != GameManager.GameState.Win && state != GameManager.GameState.Loss);

        if (state == GameManager.GameState.GeneratingCode)
        {
            if (GameManager.Instance.CheckAndResetLevelUpFlag())
            {
                if (levelUpBanner != null) StartCoroutine(ShowAndHideBanner(levelUpBanner));
            }
            
            foreach(Transform child in historyContainer) Destroy(child.gameObject);
            
            activeInputRow.UpdateSlotCount(GameManager.Instance.ActiveCodeLength);
            activeInputRow.ResetRow();
            currentGuess.Clear();
            currentPegIndex = 0;
            
            if(levelText) levelText.text = "POZIOM: " + GameManager.Instance.CurrentLevel;
            if (totalPointsText != null)
            {
                int newTotalPoints = GameManager.Instance.TotalPoints;
                StartCoroutine(AnimateNumber(totalPointsText, "PUNKTY: ", displayedTotalPoints, newTotalPoints, 1.5f));
                displayedTotalPoints = newTotalPoints; 
            }
            
            UpdateButtons();
        }
        else if (state == GameManager.GameState.Win)
        {
            int pointsWon = GameManager.Instance.LastPointsWon;
            if (pointsWonText != null)
            {
                StartCoroutine(AnimateNumber(pointsWonText, "+", 0, pointsWon, 1.0f));
            }
        }
    }

    private void UpdateButtons()
    {
        for(int i=0; i<colorButtons.Count; i++)
            colorButtons[i].gameObject.SetActive(i < GameManager.Instance.ActiveNumberOfColors);
    }

    public void OnRestart() => GameManager.Instance?.RestartGame();
    public void OnMenu() => SceneManager.LoadScene("MainMenu");
    
    public void OnTutorialButton()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.CloseTutorial();
        }
    }
   
    private void ShowScreenAnimated(GameObject screen)
    {
        screen.transform.localScale = Vector3.zero;
        screen.SetActive(true);
        StartCoroutine(AnimateScreenPop(screen.transform));
    }

    private IEnumerator AnimateScreenPop(Transform target)
    {
        float duration = 0.35f; 
        float time = 0;
        Vector3 startScale = Vector3.zero;
        Vector3 endScale = Vector3.one;

        while (time < duration)
        {
            float t = time / duration;
            float s = 1.70158f;
            float scale = (t = t - 1) * t * ((s + 1) * t + s) + 1;

            target.localScale = Vector3.LerpUnclamped(startScale, endScale, scale);
            time += Time.deltaTime;
            yield return null; 
        }
        target.localScale = endScale; 
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
    
    private IEnumerator ShowAndHideBanner(GameObject banner)
    {
        ShowScreenAnimated(banner);
        yield return new WaitForSeconds(2.5f); 
        StartCoroutine(AnimateScreenFadeOut(banner.transform));
    }
    
    private IEnumerator AnimateScreenFadeOut(Transform target)
    {
        float duration = 0.3f; 
        float time = 0;
        Vector3 startScale = target.localScale; 
        Vector3 endScale = Vector3.zero; 

        while (time < duration)
        {
            target.localScale = Vector3.Lerp(startScale, endScale, time / duration);
            time += Time.deltaTime;
            yield return null; 
        }
        target.localScale = endScale;
        target.gameObject.SetActive(false); 
    }
}