using UnityEngine;

public class StatsManager : MonoBehaviour
{
    public static StatsManager Instance;
    public PlayerController PlayerController;
    public Stat Damage, MoveSpeed, DashLength, KnockbackResistance;

    private void Awake()
    {
        if(Instance == null) { Instance = this; }
        else { Destroy(this); }
        PlayerController = GetComponent<PlayerController>();
    }

    private void Start()
    {
        Damage = new Stat("Damage");
        MoveSpeed = new Stat("Move Speed");
        DashLength = new Stat("Dash Length");
        KnockbackResistance = new Stat("Knockback Resistance");
        PlayerController.OnPlayerDeath += ResetAllStats;
    }

    void ResetAllStats()
    {
        Damage.Reset();
        MoveSpeed.Reset();
        DashLength.Reset();
        KnockbackResistance.Reset();
    }

    public void AddDamageMultiplier(float value, string ID) => Damage.AddMultiplier(ID, value);
    public void AddMoveSpeedMultiplier(float value, string ID) => MoveSpeed.AddMultiplier(ID, value);
    public void AddDashLengthMultiplier(float value, string ID) => DashLength.AddMultiplier(ID, value);
    public void AddKnockbackResistanceMultiplier(float value, string ID) => KnockbackResistance.AddMultiplier(ID, -value);
}
