using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class ProfileUI : MonoBehaviour
{
    [Header("UI References")]
    public TMP_InputField nameInputField;
    public Transform profileListContent;
    public GameObject profileButtonPrefab; 
    public GameObject profilePanel; 
    public TextMeshProUGUI currentProfileText; 

    private void Start()
    {
        if (GameManager.Instance.GetActiveProfile() == null)
        {
            var profiles = GameManager.Instance.GetAllProfiles();
            if (profiles.Count > 0)
            {
                GameManager.Instance.SelectProfile(profiles[0].playerName);
            }
        }

        RefreshProfileList();
        UpdateCurrentProfileText();
        
        if (GameManager.Instance.GetActiveProfile() == null)
        {
            profilePanel.SetActive(true);
        }
        else
        {
            profilePanel.SetActive(false);
        }
    }

    public void OnCreateProfileButton()
    {
        string name = nameInputField.text;
        GameManager.Instance.CreateProfile(name);
        nameInputField.text = "";
        RefreshProfileList();
        UpdateCurrentProfileText();
    }

    public void OnSelectProfile(string name)
    {
        GameManager.Instance.SelectProfile(name);
        UpdateCurrentProfileText();
        profilePanel.SetActive(false);
    }

    public void ToggleProfilePanel()
    {
        profilePanel.SetActive(!profilePanel.activeSelf);
        if(profilePanel.activeSelf) RefreshProfileList();
    }

    private void RefreshProfileList()
    {
        foreach (Transform child in profileListContent) Destroy(child.gameObject);

        List<PlayerProfile> profiles = GameManager.Instance.GetAllProfiles();
        foreach (var p in profiles)
        {
            GameObject rowObj = Instantiate(profileButtonPrefab, profileListContent);
            
            Transform selectBtnTr = rowObj.transform.Find("SelectButton");
            if (selectBtnTr != null)
            {
                Button selectBtn = selectBtnTr.GetComponent<Button>();
                TextMeshProUGUI btnText = selectBtnTr.GetComponentInChildren<TextMeshProUGUI>();
                
                if (btnText) btnText.text = p.playerName + $" (Lvl {p.level})";
                
                string profileName = p.playerName; 
                selectBtn.onClick.AddListener(() => OnSelectProfile(profileName));
            }

            Transform deleteBtnTr = rowObj.transform.Find("DeleteButton");
            if (deleteBtnTr != null)
            {
                Button deleteBtn = deleteBtnTr.GetComponent<Button>();
                string profileName = p.playerName;
                
                deleteBtn.onClick.AddListener(() => {
                    GameManager.Instance.DeleteProfile(profileName);
                    RefreshProfileList();       
                    UpdateCurrentProfileText(); 
                });
            }
        }
    }
    
    private void UpdateCurrentProfileText()
    {
        var profile = GameManager.Instance.GetActiveProfile();
        if (profile != null && currentProfileText != null)
        {
            currentProfileText.text = "Gracz: " + profile.playerName;
        }
        else if (currentProfileText != null)
        {
            currentProfileText.text = "Brak profilu";
        }
    }
}