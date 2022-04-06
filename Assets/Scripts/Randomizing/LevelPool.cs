using UnityEngine;
using System.Collections.Generic;

[System.Serializable] [CreateAssetMenu(fileName = "Level X Pool", menuName = "Level Pool")]
public class LevelPool : ScriptableObject
{
    public bool IsFixed;
    public List<Level> Levels = new List<Level>();

    public Level GetLevelWeighted(int index) 
    {
        if(Levels.Count == 1 || index < 0) return GetLevel();
        List<Level> temp = new List<Level>(Levels);
        temp.RemoveAt(index);
        return temp[Random.Range(0, temp.Count)];
    }

    public Level GetLevel() => Levels[Random.Range(0, Levels.Count)];
}

[System.Serializable]
public class Level
{
    public int ID;
    public GameObject Prefab;
}