using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CameraType
{ None, Deploy, Battle, Follow, Endgame }

public class CameraController : MonoBehaviour
{   
    //팔로우 타겟 설정
    [NonSerialized] public Minion targetUnit;
    
    //상태(타입) 변수
    private DeployType _deployType;
    private BattleType _battleType;
    private FollowType _followType;
    private EndgameType _endgameType;
    
    public CameraType CurrentCameraType { get; private set; }
    private Dictionary<CameraType, ICameraType> _cameraTypes;
    
    [NonSerialized] public Vector3 lastCameraPosition;
    [NonSerialized] public Quaternion lastCameraRotation;
    
    public UnitPanelController _unitPanelController;
    
    [NonSerialized] public BoxCollider _cameraBound;
    
    private void Start()
    {
        _deployType = new DeployType();
        _battleType = new BattleType();
        _followType = new FollowType();
        _endgameType = new EndgameType();
        
        _cameraTypes = new Dictionary<CameraType, ICameraType>
        {
            { CameraType.Deploy, _deployType },
            { CameraType.Battle, _battleType },
            { CameraType.Follow, _followType },
            { CameraType.Endgame, _endgameType }
        };
        
        SetCameraType(CameraType.Deploy);
    }

    public void InitCamera(MapData mapData)
    {
        _cameraBound = Instantiate(mapData._cameraBounds);

        transform.position = mapData._cameraPoint.worldPosition;
        transform.rotation = Quaternion.Euler(mapData._cameraPoint.rotation);
        
        lastCameraPosition = transform.position;
        lastCameraRotation = transform.rotation;
    }
    
    public void SetCameraType(CameraType cameraType)
    {
        if (CurrentCameraType != CameraType.None)
        {
            _cameraTypes[CurrentCameraType].Exit();
        }
        CurrentCameraType = cameraType;
        
        _cameraTypes[CurrentCameraType].Enter(this);
    }
    
    private void Update()
    {
        if (CurrentCameraType != CameraType.None)
        {
            _cameraTypes[CurrentCameraType].Update();
        }
    }
}
