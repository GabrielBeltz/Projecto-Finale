using UnityEngine;

public class SpawnChance : MonoBehaviour
{
    [Help("A chance vai de 0 a 100 em int, chance maior que 100 sempre spawna, menor que 0 nunca.")]
    public float BaseChance;
    [Help("Modificador que considera o n�vel atual na torre. Pode ser usado para coisas ficarem mais ou menos comuns mais alto na torre.")]
    public float ChancePerFloor;
    [Help("Modificador que considera quantos double jumps o player tem. Pode ser usado para situa��es que ficariam muito dif�ceis sem pulos extras.")]
    public float ChancePerExtraJump;
    [Help("Modificador que considera quantos ranks de Dash o player tem. Pode ser usado para situa��es que ficariam muito dif�ceis sem o dash.")]
    public float ChancePerDashRank;
    [Help("Modificador que considera se esse spawn depende de outro spawn. Pode ser usado para desativar inimigos que spawnariam numa plataforma que n�o spawnou.")]
    public SpawnChance DependsOn;
    [Help("Contr�rio do DependsOn, somente spawna se outro n�o spawnar.")]
    public SpawnChance InverseDependsOn;

    private void Awake()
    {
        if(DependsOn == null)
            if(InverseDependsOn == null)
                if(Random.Range(0, 100) > GetChanceToSpawn()) gameObject.SetActive(false);
    }

    private void Start()
    {
        if(DependsOn != null)
        {
            if(InverseDependsOn != null)
            {
                if(Random.Range(0, 100) > GetChanceToSpawn() || !DependsOn.gameObject.activeSelf || InverseDependsOn.gameObject.activeSelf) gameObject.SetActive(false);
            }
            else
            {
                if(Random.Range(0, 100) > GetChanceToSpawn() || !DependsOn.gameObject.activeSelf) gameObject.SetActive(false);
            }
        }
        else
        {
            if(InverseDependsOn != null)
                if(Random.Range(0, 100) > GetChanceToSpawn() || InverseDependsOn.gameObject.activeSelf) gameObject.SetActive(false);
        }
    }

    float GetChanceToSpawn() => 
        BaseChance + 
        (ChancePerFloor * TowerController.Instance.CurrentFloor) + 
        (ChancePerExtraJump * TowerController.Instance.PlayerController.ExtraJumpsMax) + 
        (ChancePerDashRank * TowerController.Instance.PlayerController.DashRank)
        ;
}
