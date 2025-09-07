using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICameraType
{
    void Enter(CameraController cameraController);
    void Update();
    void Exit();
}
