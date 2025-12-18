using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopManager : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI pointsText; 
    public Button buyButton;           
    public TextMeshProUGUI buttonText; 
    
    [Header("Settings")]
    public int skinCost = 200;

    [Header("VFX")]
    public GameObject coinEffectPrefab;
    public Transform canvasTransform; 
    
    private void OnEnable()
    {
        UpdateShopUI();
    }

    public void BuySkin()
    {
        if (GameManager.Instance == null) return;
        if (GameManager.Instance.HasMapSkin) return;

        if (GameManager.Instance.TrySpendPoints(skinCost))
        {
            GameManager.Instance.UnlockMapSkin();
            UpdateShopUI(); 
            
            if (coinEffectPrefab != null && canvasTransform != null)
            {
                GameObject effect = Instantiate(coinEffectPrefab, canvasTransform);
                effect.transform.position = buyButton.transform.position;
            }
            if (AudioManager.Instance != null) 
                AudioManager.Instance.PlaySFX(AudioManager.Instance.coinSound);
        }
    }

    private void UpdateShopUI()
    {
        if (GameManager.Instance == null) return;

        if (pointsText != null)
        {
            pointsText.text = "Twoje punkty: " + GameManager.Instance.TotalPoints;
        }

        if (GameManager.Instance.HasMapSkin)
        {
            buyButton.interactable = false; 
            buttonText.text = "POSIADANE";
        }
        else
        {
            if (GameManager.Instance.TotalPoints >= skinCost)
            {
                buyButton.interactable = true; 
                buttonText.text = $"KUP ({skinCost} pkt)";
            }
            else
            {
                buyButton.interactable = false; 
                buttonText.text = $"KUP ({skinCost} pkt)";
            }
        }
    }
}