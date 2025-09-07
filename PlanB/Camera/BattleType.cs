using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleType : ICameraType
{
    //참조변수
    private CameraController _cameraController;
    private Camera _camera;
    private CharacterController _characterController;
    private Bounds bounds;
    
    private float walkingSpeed = 10f;
    private float runningSpeed = 20f;
    private bool isRunning;
    private float lookSpeed = 2.0f;
    
    Vector3 moveDirection = Vector3.zero;
    
    
    float rotationX = 0;
    float rotationY = 0;
    private Transform _cameraTransform;
    
    public void Enter(CameraController cameraController)
    {
        _cameraController = cameraController;
        _camera = Camera.main;
        _cameraTransform = _camera.transform;
        _characterController = _camera.GetComponent<CharacterController>();
        
        // 현재 카메라 위치 회전값 불러오기
        _cameraTransform.position = _cameraController.lastCameraPosition;
        _cameraTransform.rotation = _cameraController.lastCameraRotation;
        
        // 회전값 동기화
        Vector3 euler = _cameraTransform.rotation.eulerAngles;
        rotationX = (euler.x > 180) ? euler.x - 360f : euler.x;
        rotationY = euler.y;
        
        _cameraController._unitPanelController.crosshair.SetActive(true);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        BoxCollider boundsCollider = _cameraController._cameraBound;
        bounds = boundsCollider.bounds;
    }

    public void Update()
    {
        if (!_cameraController._unitPanelController._isPaused)
        {
            RotateCamera();
            MoveCamera();
            if (Input.GetKeyDown(KeyCode.L))
                _cameraController.SetCameraType(CameraType.Follow);
        }
    }

    private void MoveCamera()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        
        Vector3 direction = _cameraTransform.right * horizontal + _cameraTransform.forward * vertical;
        
        if (Input.GetKey(KeyCode.Space))
        {
            direction += _cameraTransform.up;
        }
        
        if (Input.GetKey(KeyCode.LeftControl))
        {
            direction -= _cameraTransform.up;
        }
        
        isRunning = Input.GetKey(KeyCode.LeftShift);
        moveDirection = direction * (isRunning ? runningSpeed : walkingSpeed);
        
        Vector3 beforeMovePos = _cameraTransform.position;
        
        _characterController.Move(moveDirection * Time.deltaTime);
        
        if (!bounds.Contains(_cameraTransform.position))
        {
            // 바운드 밖이면 이동 무효화
            _cameraTransform.position = beforeMovePos;
        }
        
        _cameraController.lastCameraPosition = _cameraTransform.position;
    }

    private void RotateCamera()
    {
        float mouseX = Input.GetAxis("Mouse X") * lookSpeed;
        float mouseY = Input.GetAxis("Mouse Y") * lookSpeed;

        rotationY += mouseX;
        rotationX -= mouseY;
        rotationX = Mathf.Clamp(rotationX, -90f, 90f);
        
        _cameraTransform.rotation = Quaternion.Euler(rotationX, rotationY, 0f);
        
        _cameraController.lastCameraPosition = _cameraTransform.position;
        _cameraController.lastCameraRotation = _cameraTransform.rotation;
    }
    
    public void Exit()
    {
        _cameraController = null;
        _camera = null;
        _cameraTransform = null;
        _characterController = null;
    }
}
