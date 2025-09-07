using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndgameType : ICameraType
{
    private CameraController _cameraController;
    public void Enter(CameraController cameraController)
    {
        _cameraController = cameraController;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void Update()
    {
        
    }

    public void Exit()
    {
        _cameraController = null;
    }
}
