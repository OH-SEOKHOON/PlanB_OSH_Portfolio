using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPhase : IGamePhase
{
    //참조변수
    private Minion _minion;
    
    public void Enter(Minion minion)
    {
        _minion = minion;
    }
    
    public void Update()
    {
        
    }
    
    public void Exit()
    {
        _minion = null;
    }


}
