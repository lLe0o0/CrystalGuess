using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

public class GuessRow : MonoBehaviour
{
    [Header("Elementy UI")]
    public List<Image> guessPegs;   
    public List<Image> resultPegs;  

    [Header("Sprite'y Wyników")]
    public Sprite perfectSprite;    
    public Sprite colorOnlySprite;  
    public Sprite noMatchSprite;    

    public void SetPegSprite(int index, Sprite pegSprite)
    {
        if(index < guessPegs.Count)
        {
            guessPegs[index].sprite = pegSprite;
            guessPegs[index].color = Color.white; 
        }
    }

    public void ResetRow()
    {
        foreach(var img in guessPegs)
        {
            img.sprite = null; 
            img.color = new Color(1, 1, 1, 0); 
        }
        foreach(var img in resultPegs) 
        {
            img.gameObject.SetActive(false);
        }
    }

    public void ClearSinglePeg(int index)
    {
        if (index >= 0 && index < guessPegs.Count)
        {
            guessPegs[index].sprite = null; 
            guessPegs[index].color = new Color(1, 1, 1, 0);
        }
    }

    public void SetupHistoryRow(List<int> guessIndices, List<GameManager.HintState> hints, List<Sprite> spritePalette)
    {
        for (int i = 0; i < guessPegs.Count; i++)
        {
            if (i < guessIndices.Count)
            {
                guessPegs[i].sprite = spritePalette[guessIndices[i]];
                guessPegs[i].color = Color.white; 
            }
        }

        for (int i = 0; i < resultPegs.Count; i++)
        {
            if (i < hints.Count)
            {
                resultPegs[i].gameObject.SetActive(true); 
                resultPegs[i].color = Color.white; 
                
                switch (hints[i])
                {
                    case GameManager.HintState.Perfect:
                        resultPegs[i].sprite = perfectSprite;
                        break;
                    case GameManager.HintState.ColorOnly:
                        resultPegs[i].sprite = colorOnlySprite;
                        break;
                    default: 
                        resultPegs[i].sprite = noMatchSprite;
                        break;
                }
            }
        }
    }

    public void UpdateSlotCount(int activeCount)
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform slot = transform.GetChild(i);
            slot.gameObject.SetActive(i < activeCount);
        }
    }

    public void AnimatePegPop(int index)
    {
        if (index < guessPegs.Count && guessPegs[index] != null)
        {
            StartCoroutine(PopCoroutine(guessPegs[index].transform));
        }
    }

    private IEnumerator PopCoroutine(Transform target)
    {
        float duration = 0.1f;
        float time = 0;
        Vector3 startScale = Vector3.one;
        Vector3 targetScale = Vector3.one * 1.3f; 

        while (time < duration)
        {
            target.localScale = Vector3.Lerp(startScale, targetScale, time / duration);
            time += Time.deltaTime;
            yield return null;
        }
        target.localScale = targetScale;

        time = 0;
        while (time < duration)
        {
            target.localScale = Vector3.Lerp(targetScale, startScale, time / duration);
            time += Time.deltaTime;
            yield return null;
        }
        target.localScale = startScale;
    }
}