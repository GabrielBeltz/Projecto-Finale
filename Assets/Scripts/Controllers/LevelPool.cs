using UnityEngine;
using System.Collections.Generic;

[System.Serializable] [CreateAssetMenu(fileName = "Level X Pool", menuName = "Level Pool")]
public class LevelPool : ScriptableObject
{
    [SerializeField] List<GameObject> Prefabs = new List<GameObject>();

    public GameObject GetLevel() => Prefabs[Random.Range(0, Prefabs.Count)];
}