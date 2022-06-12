using UnityEngine;

public class SpawnChance : MonoBehaviour
{
    public float totalCalculatedChance;
    bool delay;
    [Help("A chance vai de 0 a 100 em int, chance maior que 100 sempre spawna, menor que 0 nunca.")]
    public float BaseChance =0 ;
    [Help("Modificador que considera o nível atual na torre. Pode ser usado para coisas ficarem mais ou menos comuns mais alto na torre.")]
    public float ChancePerFloor;
    [Help("Modificador que considera quantos double jumps o player tem. Pode ser usado para situações que ficariam muito difíceis sem pulos extras.")]
    public float ChancePerExtraJump;
    [Help("Modificador que considera quantos ranks de Dash o player tem. Pode ser usado para situações que ficariam muito difíceis sem o dash.")]
    public float ChancePerDashRank;
    [Help("Modificador que considera se esse spawn depende de outro spawn. Pode ser usado para desativar inimigos que spawnariam numa plataforma que não spawnou.")]
    public SpawnChance DependsOn;
    [Help("Contrário do DependsOn, somente spawna se outro não spawnar.")]
    public SpawnChance InverseDependsOn;
    [Help("Muda a chance de spawnar se outro objeto não spawnar. Pode ser usado para aumentar a chance de uma plataforma spawnar se outra não spawnar.")]
    public SpawnChance ChanceIfNotSpawnedTarget;
    public float ChanceIfNotSpawned;

    private void Awake()
    {
        delay = DependsOn != null || InverseDependsOn != null || ChanceIfNotSpawnedTarget != null;
        if(!delay) gameObject.SetActive(DecideSpawn());
    }

    private void Start() 
    { 
        if(delay) gameObject.SetActive(DecideSpawn());
    }
    
    bool DecideSpawn()
    {
        totalCalculatedChance = GetChanceToSpawn();
        if(DependsOn != null)
        {
            if(InverseDependsOn != null)
            {
                if(totalCalculatedChance > InverseDependsOn.totalCalculatedChance) InverseDependsOn.gameObject.SetActive(false);
                if(Random.Range(0, 100) > totalCalculatedChance || !DependsOn.gameObject.activeSelf || InverseDependsOn.gameObject.activeSelf) return false;
            }
            else if(Random.Range(0, 100) > totalCalculatedChance || !DependsOn.gameObject.activeSelf) return false;
        }
        else if(InverseDependsOn != null)
            if(Random.Range(0, 100) > totalCalculatedChance || InverseDependsOn.gameObject.activeSelf) return false;

        return Random.Range(0, 100) <= totalCalculatedChance;
    }

    float GetChanceToSpawn() =>
        BaseChance +
        (ChancePerFloor * TowerController.Instance.CurrentFloor) +
        (ChancePerExtraJump * TowerController.Instance.PlayerController.AbilityRanks.MobilityRank > 2 ? 1 : 0) +
        (ChancePerDashRank * TowerController.Instance.PlayerController.AbilityRanks.DashRank) +
        (ChanceIfNotSpawnedTarget == null ? 0 : !ChanceIfNotSpawnedTarget.gameObject.activeSelf ? ChanceIfNotSpawned : 0);
}
