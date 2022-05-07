using UnityEngine;

public class StatsManager : MonoBehaviour
{
    public static StatsManager Instance;
    public Stat Damage, MoveSpeed, DashLength, KnockbackResistance, Health;

    private void Awake()
    {
        if(Instance == null) { Instance = this; }
        else { Destroy(this); }
    }

    private void Start()
    {
        Damage = new Stat("Damage");
        MoveSpeed = new Stat("Move Speed");
        DashLength = new Stat("Dash Length");
        KnockbackResistance = new Stat("Knockback Resistance");
        Health = new Stat("Health");
        PlayerController.Instance.OnPlayerDeath += ResetAllStats;
    }

    void ResetAllStats()
    {
        Damage.Reset();
        MoveSpeed.Reset();
        DashLength.Reset();
        KnockbackResistance.Reset();
    }
}
