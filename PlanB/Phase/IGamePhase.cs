using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IGamePhase
{
    void Enter(Minion minion);
    void Update();
    void Exit();
}
