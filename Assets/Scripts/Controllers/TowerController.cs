using System.Collections.Generic;
using UnityEngine;

public class TowerController : MonoBehaviour
{
    public static TowerController Instance;

    public List<LevelPool> LevelPools = new List<LevelPool>();
    public float FloorHeight = 15;
    public int CurrentFloor;
    [Header("Needed to Work")]
    public Transform _player;
    Transform _tower;
    float _baseHeight;
    int _lastLevel = 0, _newLevel;

    GameObject[] _spawnedLevels;

    void Awake()
    {
        if(Instance == null) Instance = this;
        else Destroy(this);
        _baseHeight = _player.transform.position.y;
    }

    private void Start()
    {
        _tower = new GameObject("Tower").transform;
        _spawnedLevels = new GameObject[LevelPools.Count];
        NewLevel();
    }

    void Update()
    {
        _newLevel = Mathf.Clamp(Mathf.FloorToInt((_player.position.y + 1 - _baseHeight) / FloorHeight), 0, 100) + 1;
        CurrentFloor = _newLevel;

        if(_lastLevel != _newLevel && _lastLevel < _newLevel) NewLevel();

        if(_spawnedLevels[_newLevel + 1] == null || LevelPools[_newLevel + 1].IsFixed) return;

        Destroy(_spawnedLevels[_newLevel + 1]);
        _spawnedLevels[_newLevel + 1] = null;
        _lastLevel = _newLevel;
    }

    void NewLevel()
    {
        _lastLevel = _newLevel;
        if(_spawnedLevels[_newLevel] != null) return;
        _spawnedLevels[_newLevel] = Instantiate(LevelPools[_newLevel].GetLevel(), new Vector3(0, 4 + (FloorHeight * _newLevel)), Quaternion.identity, _tower);
        if(Random.Range(1, 3) == 2) _spawnedLevels[_newLevel].transform.localScale = new Vector3(-1, 1, 1);
    }
}