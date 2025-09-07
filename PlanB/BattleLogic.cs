using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleLogic
{
    private UnitPanelController _unitPanelController;
    private List<GameObject> _minions;
    private List<GameObject> _enemies;

    // 생성자
    public BattleLogic(UnitPanelController unitPanelController, List<GameObject> minions, List<GameObject> enemies)
    {
        _minions = minions;
        _enemies = enemies;
    }

    // todo: 배틀 로직 구현
}