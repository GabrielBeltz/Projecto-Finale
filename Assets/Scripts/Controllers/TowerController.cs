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
    float _baseHeight;
    int _lastLevel = 0, _newLevel;

    List<GameObject> _spawnedLevels;

    void Awake()
    {
        if(Instance == null) Instance = this;
        else Destroy(this);
        _baseHeight = _player.transform.position.y;
    }

    private void Start()
    {
         _spawnedLevels = new List<GameObject>();
        NewLevel();
    }

    void Update()
    {
        _newLevel = Mathf.FloorToInt((_player.position.y - _baseHeight) / FloorHeight);
        CurrentFloor = _newLevel;
        _newLevel = Mathf.Clamp(_newLevel, 0, 100) + 1;
        if(_lastLevel != _newLevel && _lastLevel < _newLevel)
        {
            NewLevel();
            _lastLevel = _newLevel;
        }
        
        if(_spawnedLevels.Count > _newLevel + 1) 
        {
            Destroy(_spawnedLevels[_newLevel + 1]);
            _spawnedLevels.RemoveAt(_newLevel + 1);
            _lastLevel--;
        } 
    }

    void NewLevel()
    {
        _spawnedLevels.Add(Instantiate(LevelPools[_newLevel].GetLevel(), new Vector3(0, 4 + (FloorHeight * _newLevel)), Quaternion.identity, null));
        if(Random.Range(1, 3) == 2) _spawnedLevels[_spawnedLevels.Count - 1].transform.localScale = new Vector2(-1, 1);
    }
}