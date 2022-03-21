using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatsManager : MonoBehaviour
{
    public static StatsManager Instance;

    public Stat Damage, MoveSpeed, DashLength;

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    private void Start()
    {
        Damage = new Stat("Damage");
        MoveSpeed = new Stat("MoveSpeed");
        DashLength = new Stat("DashLength");
    }

    public void AddDamageMultiplier(float value, string ID) => Damage.AddMultiplier(ID, value);
    public void AddMoveSpeedMultiplier(float value, string ID) => MoveSpeed.AddMultiplier(ID, value);
    public void AddDashLengthMultiplier(float value, string ID) => DashLength.AddMultiplier(ID, value);
}
