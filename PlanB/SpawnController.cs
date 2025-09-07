using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
#if UNITY_EDITOR
using UnityEditor.TextCore;
#endif
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UIElements;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public enum DeployMouseType
{ None, Inst, Arr, Battle }

public class SpawnController : MonoBehaviour, ISelectObserver<GameObject>
{
    //유닛 정보
    [NonSerialized]public List<GameObject> _minions = new List<GameObject>();
    [NonSerialized]public List<GameObject> _enemies = new List<GameObject>();
    
    //배치 정보
    [NonSerialized]public DeployMouseType _deployMouseType;         //현재 마우스 타입
    [NonSerialized]public GameObject currentspwanMinion;            //생성할 유닛 정보(inst시 사용)
    [NonSerialized]public UIminionCell currentCell;                 //생성할 유닛 UI 셀 (남은 소환가능 수 사용)
    private Minion currentSelectedMinion;                           //선택 중인 유닛 정보(arr및 배틀 페이즈시 사용)
    public Vector3 boxHalfExtents = new Vector3(0.1f, 1f, 0.1f);    // Box Ray의 크기
    [SerializeField] LayerMask groundLayer;                         //감지될 땅 레이어
    private float spawnTimer = 0.1f;                                //연속 스폰 방지용 시간
    private float lastSpawnTimer = 0f;                              //연속 스폰 방지용 타이머
    
    //배치 영역 정보
    [SerializeField] private string _mapname;
    [NonSerialized] public MapData _mapData;
    [NonSerialized] public BoxCollider _spawnBox;  //배치 영역(콜라이더)
    private GameObject _noneDeployable;
    
    
    //카메라 정보
    [NonSerialized] public CameraController _cameraController;
    private Camera _mainCamera;
    
    private void Awake()
    {
        // Addressables 시스템 초기화 (최초 1회만 필요함)
        Addressables.InitializeAsync().WaitForCompletion();

        // 실제 MapData 동기 로드
        var handle = Addressables.LoadAssetAsync<MapData>(_mapname);
        _mapData = handle.WaitForCompletion();

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            Debug.Log("MapData 로드 성공: " + _mapData.name);
        }
        else
        {
            Debug.LogError("MapData 로드 실패");
        }
        
        _mainCamera = Camera.main;
        if (_mainCamera != null)
            _cameraController = _mainCamera.GetComponent<CameraController>();
    }
    
    void Start()
    {
        _deployMouseType = DeployMouseType.Inst;
        Initmap();
    }

    void Initmap()
    {
        Instantiate(_mapData._mapPrefab);

        _spawnBox = Instantiate(_mapData._spawnBox);
        
        _noneDeployable = Instantiate(_mapData._noneDeployable);
        
        foreach (var enemy in _mapData._enemies)
        {
            var temp = Instantiate(enemy.enemiePrefab, enemy.worldPosition, Quaternion.Euler(enemy.rotation) );
            _enemies.Add(temp);
        }
        
        _cameraController.InitCamera(_mapData);
    }
    
    void Update()
    {
        if (_deployMouseType == DeployMouseType.Inst && currentspwanMinion != null)
        {
            lastSpawnTimer += Time.deltaTime;
            CheckSpawnPoint();
        }
    }

    void CheckSpawnPoint()
    {
        if ((Input.GetMouseButtonDown(0) || Input.GetMouseButton(0)) && !EventSystem.current.IsPointerOverGameObject())
        {
            Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 200f, groundLayer))
            {
                if (!_spawnBox.bounds.Contains(hit.point)) return;

                Vector3 direction = hit.normal;
                Vector3 origin = hit.point + direction * 0.5f;
                Vector3 spawnPos = hit.point;

                Collider[] overlaps = Physics.OverlapBox(spawnPos, boxHalfExtents, Quaternion.identity);

                bool blocked = false;
                foreach (var col in overlaps)
                {
                    if (col.CompareTag("Player")) // 필요시 태그로 필터링
                    {
                        blocked = true;
                        break;
                    }
                }

                if (!blocked && lastSpawnTimer >= spawnTimer)
                {
                    SpawnUnit(spawnPos);
                    lastSpawnTimer = 0f;
                }
            }
        }
    }
    void SpawnUnit(Vector3 spawnPos)
    {
        if (currentCell.IsSpawned())
        {
            var instance = Instantiate(currentspwanMinion, spawnPos, Quaternion.identity); // 오브젝트 생성
            _minions.Add(instance);

            // Minion 컴포넌트 가져오기
            var minion = instance.GetComponent<Minion>();
            
            if (minion != null)
            {
                minion._spawnBox = _spawnBox;
                minion._spawnController = this;
            }
        }
    }

    
    #region 선택 옵저버
    public void OnMinionSelecte(Minion minion)
    {
        // 기존 선택 해제
        if (currentSelectedMinion != null && currentSelectedMinion != minion)
        {
            currentSelectedMinion.UnSelect();
        }
        
        currentSelectedMinion = minion;
        _cameraController.targetUnit = minion;
    }

    #endregion

    public void OnBattlePhase()
    {
        _noneDeployable.SetActive(false);
        foreach (var minion in _minions)
        {
            var _minion = minion.GetComponent<Minion>();
            _minion.SetPhase(GamePhase.Battle);
        }
        
        // 기존 선택 해제
        if (currentSelectedMinion != null)
        {
            currentSelectedMinion.UnSelect();
        }
        _deployMouseType = DeployMouseType.Battle;
        _cameraController.SetCameraType(CameraType.Battle);
    }

    public void OnBattleEnd()
    {
        foreach (var minion in _minions)
        {
            var _minion = minion.GetComponent<Minion>();
            _minion.SetPhase(GamePhase.Endgame);
        }
        
        _cameraController.SetCameraType(CameraType.Endgame);
    }
}
