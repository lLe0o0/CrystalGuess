using UnityEngine;
using UnityEngine.UI;

public class ButtonSound : MonoBehaviour
{
    private void Start()
    {
        GetComponent<Button>().onClick.AddListener(() => 
        {
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlayButtonSound();
        });
    }
}