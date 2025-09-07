using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.EventSystems;

public enum GamePhase
{ None, Deploy, Battle, Follow, Endgame }

public class Minion : MonoBehaviour, ISelectSubject<GameObject>
{

    //드래그관련
    [NonSerialized] public CapsuleCollider mycollider;
    public LayerMask groundLayer;
    public LayerMask playerLayer;
    [NonSerialized] public BoxCollider _spawnBox;
    
    //외곽선 쉐이더
    //private Outline _outline;
    
    // 상태(페이즈)변수
    private DeployPhase _deployPhase;
    private BattlePhase _battlePhase;
    private FollowPhase _followPhase;
    private EndgamePhase _endgamePhase;
    
    public GamePhase CurrentPhase { get; private set; }
    private Dictionary<GamePhase, IGamePhase> _gamePhases;
    
    //옵저버 관련(스폰 컨트롤러)
    public SpawnController _spawnController;
    
    private void Awake()
    {
        mycollider = GetComponent<CapsuleCollider>();
        //_outline = GetComponentInChildren<Outline>();
    }

    private void Start()
    {
        SetColor(Color.green);
        UnSelect();
        
        _deployPhase = new DeployPhase();
        _battlePhase = new BattlePhase();
        _followPhase = new FollowPhase();
        _endgamePhase = new EndgamePhase();

        _gamePhases = new Dictionary<GamePhase, IGamePhase>
        {
            { GamePhase.Deploy, _deployPhase },
            { GamePhase.Battle, _battlePhase },
            { GamePhase.Follow, _followPhase },
            { GamePhase.Endgame, _endgamePhase }
        };
        
        SetPhase(GamePhase.Deploy);
    }

    private void Update()
    {
        if (CurrentPhase != GamePhase.None)
        {
            _gamePhases[CurrentPhase].Update();
        }
    }
    public void SetPhase(GamePhase newPhase)
    {
        if (CurrentPhase != GamePhase.None)
        {
            _gamePhases[CurrentPhase].Exit();
        }
        CurrentPhase = newPhase;
        _gamePhases[CurrentPhase].Enter(this);
    }
    
    public void SetColor(Color color)
    {
        //_outline.OutlineColor = color;
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (CurrentPhase == GamePhase.Deploy)
        {
            if (other.CompareTag("Player"))
            {
                if (_deployPhase.draggable)
                    SetColor(Color.red);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (CurrentPhase == GamePhase.Deploy)
        {
            if (other.CompareTag("Player"))
            {
                if (_deployPhase.draggable)
                    SetColor(Color.cyan);
            }
        }
    }

    public void OnSelect()
    {
        //_outline.OutlineWidth = 10f;
        OnSelected(this);
    }

    public void UnSelect()
    {
        //_outline.OutlineWidth = 0f;
    }
    
    # region 선택 옵저버
    public void OnSelected(Minion minion)
    {
        _spawnController.OnMinionSelecte(this);
    }
    
    # endregion
}
