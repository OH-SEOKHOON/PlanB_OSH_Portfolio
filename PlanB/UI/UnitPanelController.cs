using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Unity.VisualScripting;

public class UnitPanelController : MonoBehaviour
{
    [SerializeField] private Button deployButton;
    [SerializeField] private Sprite instImage;
    [SerializeField] private Sprite arrImage;
    
    [SerializeField] private SpawnController _spawnController;
    [SerializeField]List<GameObject> MinionPrefabs = new List<GameObject>();
    private UIminionCell currentSelectedMinionCell = null;
    
    [SerializeField] private RectTransform battleStartButton;
    private RectTransform _rectTransform;
    
    public GameObject crosshair;
    
    [SerializeField] private GameObject _timetext;
    private TimerDisplay _timerDisplay;
    private RectTransform _timerRectTransform;
    
    [SerializeField] private GameObject _pausepanel;
    [NonSerialized] public bool _isPaused = false;
    
    
    //게임 결과 패널
    [SerializeField]private GameObject _winPanel;
    [SerializeField]private GameObject _losePanel;
    [SerializeField]private GameObject _backPanel;
    private bool _isGameOver = false;
    
    private void Start()
    {
        _rectTransform = GetComponent<RectTransform>();
        _timerDisplay = _timetext.GetComponent<TimerDisplay>();
        _timerRectTransform = _timetext.GetComponent<RectTransform>();
    }

    public void OnDeployButtonClicked()
    {
        if (_spawnController._deployMouseType == DeployMouseType.Inst)
        {
            deployButton.image.sprite = arrImage;
            _spawnController._deployMouseType = DeployMouseType.Arr;
            
            // 기존 선택된 셀이 있을 경우 강조 해제
            if (currentSelectedMinionCell != null)
            {
                currentSelectedMinionCell.SetHighlighted(false); // 이전 셀 강조 해제
                currentSelectedMinionCell = null; //선택 해제
            }
            
        }
        else if (_spawnController._deployMouseType == DeployMouseType.Arr)
        {
            deployButton.image.sprite = instImage;
            _spawnController._deployMouseType = DeployMouseType.Inst;
        }
    }

    public void SetMinionsPrefabs(UIminionCell cell)
    {
        if (_spawnController._deployMouseType == DeployMouseType.Arr)
        {
            deployButton.image.sprite = instImage;
            _spawnController._deployMouseType = DeployMouseType.Inst;
        }
        
        // 기존 선택된 셀이 있을 경우 강조 해제
        if (currentSelectedMinionCell != null)
        {
            currentSelectedMinionCell.SetHighlighted(false); // 이전 셀 강조 해제
        }
        
        if (currentSelectedMinionCell == cell)
        {
            // 이미 선택된 인덱스를 다시 눌렀으므로 해제
            Debug.Log("선택 해제됨");
            currentSelectedMinionCell = null;
            _spawnController.currentspwanMinion = null;
            return;
        }
        
        // 새로운 셀 선택
        cell.SetHighlighted(true); // 새로 선택된 셀 강조

        _spawnController.currentspwanMinion = MinionPrefabs[cell.Index];
        
        // 새로운 선택된 셀 저장 
        _spawnController.currentCell = cell;
        currentSelectedMinionCell = cell; 
        
    }

    public void BattleStart()
    {
        if (_spawnController._minions.Count > 0)
        {
            _rectTransform.DOAnchorPos(new Vector2(0, -450f), 0.5f).SetEase(Ease.OutCubic);
            battleStartButton.DOAnchorPos(new Vector2(-120, 50), 0.5f).SetEase(Ease.OutCubic);
            battleStartButton.gameObject.SetActive(false);
            _timerRectTransform.DOAnchorPos(new Vector2(0, -25), 0.5f).SetEase(Ease.OutCubic);
            _timerDisplay.StartTimer();
            _spawnController.OnBattlePhase();
        }
        else
        {
            Debug.Log("부대가 없습니다!");
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && !_isGameOver)
        {
            OnPause(_isPaused);
        }
    }

    private void OnPause(bool isPaused)
    {
        if (isPaused)
        {
            if (_spawnController._cameraController.CurrentCameraType == CameraType.Deploy)
            {
                foreach (var minion in _spawnController._minions)
                {
                    var m = minion.GetComponent<Minion>();
                    m.SetPhase(GamePhase.Deploy);
                }
            }
            else
            {
                foreach (var minion in _spawnController._minions)
                {
                    var m = minion.GetComponent<Minion>();
                    m.SetPhase(GamePhase.Battle);
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                }
            }
            
            if (_spawnController._cameraController.CurrentCameraType == CameraType.Follow)
            {
                _spawnController._cameraController.targetUnit.SetPhase(GamePhase.Follow);
            }
            _pausepanel.SetActive(false);
            _isPaused = false;
            Time.timeScale = 1f;
        }
        else
        {
            foreach (var minion in _spawnController._minions)
            {
                var m = minion.GetComponent<Minion>();
                m.SetPhase(GamePhase.Endgame);
            }
            _pausepanel.SetActive(true);
            _isPaused = true;
            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    public void EndGame(Team team, int killcount, int cost, int score)
    {
        _isGameOver = true;
        _spawnController.OnBattleEnd();
        _timerDisplay.StopTimer();
        var battleTime = _timerDisplay.GetFormattedTime();
        _backPanel.SetActive(true);

        switch (team)
        {
            case Team.Player:
                var wpc = _winPanel.GetComponent<WinpanelController>();
                wpc.OpenPanel();
                wpc.killText.text = killcount.ToString();
                wpc.timeText.text = battleTime;
                wpc.costText.text = cost.ToString();
                wpc.scoreText.text = score.ToString();
                break;
            case Team.Enemy:
                var lpc = _losePanel.GetComponent<LosepanelController>();
                lpc.OpenPanel();
                lpc.killText.text = killcount.ToString();
                lpc.timeText.text = battleTime;
                lpc.costText.text = cost.ToString();
                lpc.scoreText.text = score.ToString();
                break;
        }
    }
}
