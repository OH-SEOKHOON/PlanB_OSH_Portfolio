using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BattlePhase : IGamePhase
{
    //참조변수
    private Minion _minion;
    
    public void Enter(Minion minion)
    {
        _minion = minion;
    }
    
    public void Update()
    {
        if (_minion._spawnController._deployMouseType == DeployMouseType.Battle)
            SelectMinion();
    }

    void SelectMinion()
    {
        // 사용자가 마우스 왼쪽버튼을 눌렀을 때 true를 반환해 if문 내 코드 실행
        if(Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            float sphereRadius = 0.01f; // 반경 설정

            if (Physics.SphereCast(ray, sphereRadius, out hit)) // SphereCast로 확대된 범위
            {
                if (hit.transform == _minion.transform)
                {
                    _minion.OnSelect();
                }
            }
        }
    }
    
    public void Exit()
    {
        _minion = null;
    }
}
