using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TowerController : MonoBehaviour
{
    public static TowerController Instance;

    public List<LevelPool> LevelPools = new List<LevelPool>();
    public float FloorHeight = 15;
    public int CurrentFloor;
    [Header("Needed to Work")]
    public Transform _player;
    public TextMeshProUGUI Current, Highest;
    Transform _tower;
    float _baseHeight;
    int _lastLevel = 0, _newLevel, _lastIndex = 0;
    int _highest;
    int highest 
    { 
        get => _highest; 
        set 
        {
            if(value > _highest) _highest = value;
        } 
    }

    [HideInInspector] public PlayerController PlayerController;
    [HideInInspector] public MaskHabilities MaskHabilities;
    List<Level> lastLevels;

    GameObject[] _spawnedLevels;

    void Awake()
    {
        if(Instance == null) Instance = this;
        else Destroy(this);
        _baseHeight = _player.transform.position.y;

        PlayerController = FindObjectOfType<PlayerController>();
        MaskHabilities = FindObjectOfType<MaskHabilities>();
    }

    private void Start()
    {
        _tower = new GameObject("Tower").transform;
        lastLevels = new List<Level>();
        _spawnedLevels = new GameObject[LevelPools.Count];
        NewLevel();
    }

    void Update()
    {
        _newLevel = Mathf.Clamp(Mathf.FloorToInt((_player.position.y + 1 - _baseHeight) / FloorHeight), 0, 100) + 1;
        CurrentFloor = _newLevel;
        highest = CurrentFloor;
        Current.text = $"Current Floor: {CurrentFloor - 1}";
        Highest.text = $"Highest Floor: {highest - 1}";

        if(_lastLevel != _newLevel && _lastLevel < _newLevel) NewLevel();

        if(_spawnedLevels[_newLevel + 1] == null || LevelPools[_newLevel + 1].IsFixed) return;

        Destroy(_spawnedLevels[_newLevel + 1]);
        _spawnedLevels[_newLevel + 1] = null;
        _lastLevel = _newLevel;
        lastLevels.RemoveAt(_newLevel + 1);
    }

    void NewLevel()
    {
        if(_spawnedLevels[_newLevel] != null) return;
        Level level;
        _lastLevel = _newLevel;
        if(_newLevel == 0 || LevelPools[_newLevel - 1] != LevelPools[_newLevel])
        {
            level = LevelPools[_newLevel].GetLevel();
            _spawnedLevels[_newLevel] = Instantiate(level.Prefab, new Vector3(0, 4 + (FloorHeight * _newLevel)), Quaternion.identity, _tower);
        }
        else
        {
            level = LevelPools[_newLevel].GetLevelWeighted(_lastIndex);
            _spawnedLevels[_newLevel] = Instantiate(level.Prefab, new Vector3(0, 4 + (FloorHeight * _newLevel)), Quaternion.identity, _tower);
            _lastIndex = lastLevels[_newLevel - 1].ID;
        }

        lastLevels.Add(level);

        if(Random.Range(1, 3) == 2) _spawnedLevels[_newLevel].transform.localScale = new Vector3(-1, 1, 1);
    }
}