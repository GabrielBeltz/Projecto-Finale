using UnityEngine;

public class SpawnChance : MonoBehaviour
{
    [Help("Não usar esse script em plataformas que podem ter inimigo, já que o inimigo spawnaria sem uma plataforma e bugaria. A chance vai de 0 a 100 em int, chance maior que 100 sempre spawna, menor que 0 nunca.")]
    public float BaseChance;
    [Help("Modificador que considera o nível atual na torre. Pode ser usado para coisas ficarem mais ou menos comuns mais alto na torre.")]
    public float ChancePerFloor;
    public float ChanceToSpawn { get => BaseChance + (ChancePerFloor * TowerController.Instance.CurrentFloor); }

    private void Awake()
    {
        if(Random.Range(0, 100) > ChanceToSpawn) gameObject.SetActive(false);
    }
}
