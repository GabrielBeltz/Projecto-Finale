using System.Collections;
using UnityEngine;

[RequireComponent(typeof(EnemyAttackTarget))]
public class RespawnBehaviour : MonoBehaviour
{
    [Header("Configs")]
    public Vector3 originalPosition;

    [Tooltip("Se deixado em 0 ou menos nunca respawna.")]
    public float respawningTime;
    EnemyAttackTarget enemyAttackTarget;
    [Header("Needed to Work")]
    public Collider2D Collider;
    public Renderer Renderer;
    [Header("Optional")]
    public Rigidbody2D RigidBody;
    RigidbodyConstraints2D constraints;

    private void Awake() => enabled = respawningTime > 0;

    private void Start()
    {
        enemyAttackTarget = this.GetComponent<EnemyAttackTarget>();
        enemyAttackTarget.onDeath += CallRespawn;
        if (RigidBody != null) constraints = RigidBody.constraints;
    }

    public void CallRespawn()
    {
        StopAllCoroutines();
        StartCoroutine(WaitRespawn());
    }

    IEnumerator WaitRespawn()
    {
        yield return new WaitForSeconds(respawningTime);
        Respawn();
    }

    void Respawn()
    {
        Collider.enabled = true;
        Renderer.enabled = true;
        enemyAttackTarget.currentHealth = enemyAttackTarget.TotalHealth;
        if(RigidBody != null) RigidBody.constraints = constraints;
    }
}
