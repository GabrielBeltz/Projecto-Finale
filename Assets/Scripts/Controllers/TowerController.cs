using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerController : MonoBehaviour
{
    public List<LevelPool> Floors;
    public float FloorHeight = 15;
    [Header("Needed to Work")]
    public Transform _player;
    float _baseHeight;
    int _lastLevel = 0, _newLevel;

    void Awake()
    {
        _baseHeight = _player.transform.position.y;
    }

    void Update()
    {
        _newLevel = Mathf.FloorToInt((_player.position.y - _baseHeight) / FloorHeight);
        _newLevel = Mathf.Clamp(_newLevel, 0, 100);
        if(_lastLevel != _newLevel)
        {
            NewLevel();
            _lastLevel = _newLevel;
        }
    }

    void NewLevel()
    {
        Debug.Log($"Exiting Level {_lastLevel}, new floor {_newLevel} generated", this);
    }
}

[System.Serializable]
[CreateAssetMenu(fileName = "LevelPool", menuName = "Level Pool")]
public class LevelPool : ScriptableObject
{
    [SerializeField] List<GameObject> Prefabs = new List<GameObject>();

    public GameObject GetLevel() => Prefabs[Random.Range(0, Prefabs.Count)];
}