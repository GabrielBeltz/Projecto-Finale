using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatsManager : MonoBehaviour
{
    public static StatsManager Instance;

    public Stat Damage, MoveSpeed, DashLength, KnockbackResistance;

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
        MoveSpeed = new Stat("Move Speed");
        DashLength = new Stat("Dash Length");
        KnockbackResistance = new Stat("Knockback Resistance");
    }

    public void AddDamageMultiplier(float value, string ID) => Damage.AddMultiplier(ID, value);
    public void AddMoveSpeedMultiplier(float value, string ID) => MoveSpeed.AddMultiplier(ID, value);
    public void AddDashLengthMultiplier(float value, string ID) => DashLength.AddMultiplier(ID, value);
    public void AddKnockbackResistanceMultiplier(float value, string ID) => KnockbackResistance.AddMultiplier(ID, -value);
}
