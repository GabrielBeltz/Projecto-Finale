using System.Collections.Generic;
using UnityEngine;
using System;

[System.Serializable]
public class Stat
{
    public Action OnChange;
    public string name;
    public float totalValue
    {
        get
        {
            float totalmultipliers = 1;

            foreach(var mult in Multipliers)
            {
                totalmultipliers += mult.value;
            }

            return 1 * totalmultipliers;
        }
    }

    [SerializeField] List<StatMultiplier> Multipliers;

    public Stat(string _name)
    {
        name = _name;
        Multipliers = new List<StatMultiplier>();
    }

    public void AddMultiplier(string _name, float _value)
    {
        Multipliers.Add(new StatMultiplier(_name, _value));
        OnChange?.Invoke();
    }

    public void RemoveMultiplier(string name)
    {
        Multipliers.Remove(Multipliers.Find(mult => mult.name == name));
        OnChange?.Invoke();
    }

    public void Reset()
    {
        Multipliers.Clear();
        OnChange?.Invoke();
    }

    [System.Serializable]
    public class StatMultiplier
    {
        public string name;
        public float value;

        public StatMultiplier(string _name, float _value)
        {
            name = _name;
            value = _value;
        }
    }
}