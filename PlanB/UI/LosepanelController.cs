using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class LosepanelController : MonoBehaviour
{
    private RectTransform _rectTransform;

    public TextMeshProUGUI killText;
    public TextMeshProUGUI timeText;
    public TextMeshProUGUI costText;
    public TextMeshProUGUI scoreText;
    
    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
    }

    public void OpenPanel()
    {
        this.gameObject.SetActive(true);
        _rectTransform.DOScale(0f,0f);
        _rectTransform.DOAnchorPos(new Vector2(0, -40f), 0f);
        _rectTransform.DOScale(0.4f,0.3f).SetEase(Ease.InCubic);
    }
    
    public void OnClickCountinueButton()
    {
        UIManager.LoadScene("StartScene");
    }
}
