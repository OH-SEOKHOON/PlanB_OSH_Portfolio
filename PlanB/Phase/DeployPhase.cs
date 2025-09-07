using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DeployPhase : IGamePhase
{
    //참조변수
    private Minion _minion;
    
    //드래그 관련 변수들
    public bool draggable;     //드래그 중인지를 나타내는 불값
    private Vector3 startPos;   // 초기위치
    
    public void Enter(Minion minion)
    {
        _minion = minion;
        startPos = _minion.transform.position;
    }

    public void Update()
    {
        if (_minion._spawnController._deployMouseType == DeployMouseType.Arr)
            MinionDrag();
    }
    
    void MinionDrag()
    {
        // 사용자가 마우스 왼쪽버튼을 눌렀을 때 true를 반환해 if문 내 코드 실행
        if(Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject()){
            
            // Raycast를 생성해 부딪힌 오브젝트 반환
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            
            if (Physics.Raycast(ray, out hit)) {
                // 부딪힌 오브젝트가 이 스크립트가 붙어있는 오브젝트일 경우
                if (hit.transform == _minion.transform) {
                    draggable = true;
                    startPos = _minion.transform.position;
                    _minion.SetColor(Color.cyan);
                    _minion.OnSelect();
                }
            }
        }

        // 사용자가 마우스 왼쪽버튼을 누른 상태에서 손가락을 떼었을 때 true를 반환해 if문 내 코드 실행
        if(Input.GetMouseButtonUp(0)){
            // 드래그상태 해제
            draggable = false;
            
            CapsuleCollider capsule = _minion.mycollider;
            Vector3 point1 = capsule.transform.position + capsule.center + Vector3.up * (capsule.height / 2 - capsule.radius);
            Vector3 point2 = capsule.transform.position + capsule.center + Vector3.down * (capsule.height / 2 - capsule.radius);

            Collider[] overlapping = Physics.OverlapCapsule(
                point1,
                point2,
                capsule.radius,
                _minion.playerLayer
            );
            
            bool overlappedWithOthers = false;
            
            foreach (var col in overlapping)
            {
                if (col != _minion.mycollider && col.gameObject != _minion.gameObject)
                {
                    overlappedWithOthers = true;
                    break;
                }
            }

            if (overlappedWithOthers)
            {
                _minion.transform.position = startPos; // 이전 위치로 되돌리기
            }
            else
            {
                startPos = _minion.transform.position;
            }
            
            _minion.SetColor(Color.green);
        }

        // 현재 드래그중인 상태일 경우 if문 내 코드 실행
        if (draggable)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            Vector3 targetPosition;

            if (Physics.Raycast(ray, out hit, 200f, _minion.groundLayer))
            {
                BoxCollider box = _minion._spawnBox;
                Bounds bounds = box.bounds;

                if (bounds.Contains(hit.point))
                {
                    targetPosition = hit.point;
                    _minion.SetColor(Color.cyan);
                }
                else
                {
                    Vector3 closestPoint = box.ClosestPoint(hit.point);
                    targetPosition = closestPoint;
                    _minion.SetColor(Color.red);
                }

                _minion.transform.position = targetPosition;
            }
        }
    }

    public void Exit()
    {
        _minion = null;
    }
}
