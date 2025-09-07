using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowType : ICameraType
{
    //참조변수
    private CameraController _cameraController;
    private Camera _camera;
    
    private float rotationSpeed = 2.0f;
    private float distance = 5f;
    private Transform _target;
    private Vector3 targetPosition;
    private float _azimuthAngle;
    private float _polarAngle = 45f;
    private LayerMask obstacleLayerMask = LayerMask.GetMask("Ground");
    
    private Transform _cameraTransform;
    
    public void Enter(CameraController cameraController)
    {
        _cameraController = cameraController;
        _camera = Camera.main;
        _cameraTransform = _camera.transform;
        
        if (_cameraController.targetUnit == null)
            _cameraController.SetCameraType(CameraType.Battle);
        
        _cameraController.targetUnit.UnSelect();
        _cameraController.targetUnit.SetPhase(GamePhase.Follow);
        
        _target = _cameraController.targetUnit.transform;
        targetPosition = _target.position + new Vector3(0, 1f, 0);
        
        _cameraController._unitPanelController.crosshair.SetActive(false);
    }

    public void Update()
    {
        if (!_target)
        {
            _cameraController.SetCameraType(CameraType.Battle);
            return;
        }
        
        _target = _cameraController.targetUnit.transform;
        targetPosition = _target.position + new Vector3(0, 1f, 0);
        
        if (!_cameraController._unitPanelController._isPaused)
        {
            HandleCameraFollow();
            if (Input.GetKeyDown(KeyCode.L))
                _cameraController.SetCameraType(CameraType.Battle);
        }
    }
    
    private void HandleCameraFollow()
    {
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");
        _azimuthAngle += mouseX * rotationSpeed;
        _polarAngle -= mouseY * rotationSpeed;
        _polarAngle = Mathf.Clamp(_polarAngle, 5f, 80f);
        
        // 벽감지 처리 메서드 호출
        var currentDistance = AdjustCameraDistance();
        
        var cartesianPosition = GetCameraPosition(currentDistance, _polarAngle, _azimuthAngle);
        var cameraPosition = _target.position - cartesianPosition;
        
        _camera.transform.position = cameraPosition;
        _camera.transform.LookAt(targetPosition);
        
        _cameraController.lastCameraPosition = _cameraTransform.position;
        _cameraController.lastCameraRotation = _cameraTransform.rotation;
    }
    
    Vector3 GetCameraPosition(float r, float polarAngle, float azimuthAngle)
    {
        float b = r * Mathf.Cos(polarAngle * Mathf.Deg2Rad);
        float z = b * Mathf.Cos(azimuthAngle * Mathf.Deg2Rad);
        float y = r * Mathf.Sin(polarAngle * Mathf.Deg2Rad) * -1;
        float x = b * Mathf.Sin(azimuthAngle * Mathf.Deg2Rad);
        
        return new Vector3(x, y, z);
    }
    
    // 카메라와 타겟 사이에 장애물이 있을 때 카메라와 타겟간의 거리를 조절하는 함수
    private float AdjustCameraDistance()
    {
        //일단 초기값은 기본 거리
        var currentDistance = distance;
        
        // 타겟에서 카메라 방향을 구함
        Vector3 direction = GetCameraPosition(1f, _polarAngle, _azimuthAngle).normalized;
        RaycastHit hit;

        // 타겟에서 카메라 예정 위치까지 레이케이스 발사
        if (Physics.Raycast(_target.position, -direction, out hit, 
                distance, obstacleLayerMask))
        {
            //충돌 시, 카메라 거리를 hit 지점- 0.3f으로 조정
            float offset = 0.3f;
            currentDistance = hit.distance - offset;
            //최소거리0.5f 미만으로는 안내려가게함
            currentDistance = Mathf.Max(currentDistance, 0.5f);
        }
        //반환
        return currentDistance;
    }

    public void Exit()
    {
        _cameraController.targetUnit.OnSelect();
        _cameraController.targetUnit.SetPhase(GamePhase.Battle);
        _cameraController = null;
        _camera = null;
        _target = null;
    }
}
