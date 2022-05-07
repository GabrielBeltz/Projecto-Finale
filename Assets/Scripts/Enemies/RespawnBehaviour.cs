using UnityEngine;

public class RespawnBehaviour : MonoBehaviour
{
    [Header("Configs")]
    public GameObject[] GameObjectsToRespawnOnPlayerDeath;

    private void Start()
    {
        if(GameObjectsToRespawnOnPlayerDeath.Length > 0)
            PlayerController.Instance.OnPlayerDeath += PlayerDeathRespawn;
    }

    void PlayerDeathRespawn()
    {
        foreach(var gObj in GameObjectsToRespawnOnPlayerDeath)
        {
            gObj.SetActive(true);
        }
    }
}
