using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MapData", menuName = "Datas/MapData")]
public class MapData : ScriptableObject
{
    public GameObject _mapPrefab;
    public GameObject _noneDeployable;
    public BoxCollider _spawnBox;
    public BoxCollider _cameraBounds;
    public List<EUnit> _enemies;
    public CameraPoint _cameraPoint;
}

[System.Serializable]
public class EUnit
{
    public GameObject enemiePrefab;
    public Vector3 worldPosition;
    public Vector3 rotation;
}

[System.Serializable]
public class CameraPoint
{
    public Vector3 worldPosition;
    public Vector3 rotation;
}