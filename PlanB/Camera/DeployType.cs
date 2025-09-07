using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class DeployType : ICameraType
{
    //참조변수
    private CameraController _cameraController;
    private Camera _camera;
    private Transform _cameraTransform;
    private Bounds bounds;
    
    //이동 속도
    private float walkingSpeed = 20f;
    private float runningSpeed = 40f;
    private bool isRunning;
    
    private float zoomSpeed = 10f;
    private float minFOV = 15f;
    private float maxFOV = 60f;
    
    private float fixedY; // 고정할 Y 좌표 저장용
    
    
    public void Enter(CameraController cameraController)
    {
        _cameraController = cameraController;
        _camera = Camera.main;
        _cameraTransform = _camera.transform;
        
        _cameraTransform.position = _cameraController.lastCameraPosition;
        _cameraTransform.rotation = _cameraController.lastCameraRotation;
        
        BoxCollider boundsCollider = _cameraController._cameraBound;
        bounds = boundsCollider.bounds;
        
        fixedY = _cameraController.transform.position.y;
    }

    public void Update()
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveZ = Input.GetAxisRaw("Vertical");
        isRunning = Input.GetKey(KeyCode.LeftShift);

        // 이동 방향은 카메라 기준의 오른쪽 및 전방 방향 (단, Y는 무시)
        Vector3 right = _cameraTransform.right;
        Vector3 forward = _cameraTransform.forward;
        right.y = 0;
        forward.y = 0;
        right.Normalize();
        forward.Normalize();

        Vector3 moveDir = (right * moveX + forward * moveZ).normalized;
        Vector3 movement = moveDir * ((isRunning ? runningSpeed : walkingSpeed) * Time.deltaTime);

        Vector3 newPosition = _cameraController.transform.position + movement;

        // Y 위치 고정
        newPosition.y = fixedY;

        // 박스 콜라이더 바운드 내로 제한
        newPosition.x = Mathf.Clamp(newPosition.x, bounds.min.x, bounds.max.x);
        newPosition.z = Mathf.Clamp(newPosition.z, bounds.min.z, bounds.max.z);

        _cameraController.transform.position = newPosition;

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        _camera.fieldOfView -= scroll * zoomSpeed;
        _camera.fieldOfView = Mathf.Clamp(_camera.fieldOfView, minFOV, maxFOV);

        _cameraController.lastCameraPosition = _cameraTransform.position;
        _cameraController.lastCameraRotation = _cameraTransform.rotation;
    }

    public void Exit()
    {
        _camera.DOFieldOfView(60f, 0.1f).SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                _cameraController = null;
                _camera = null;
            });
    }
}
