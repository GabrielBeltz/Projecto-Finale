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
#if UNITY_EDITOR
    [Help("Se esses ficarem nulos o script pega do EnemyAttackTarget.", UnityEditor.MessageType.Info)]
#endif
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
        constraints = RigidBody != null ? RigidBody.constraints : RigidbodyConstraints2D.None;
        Collider = Collider != null ? Collider : enemyAttackTarget.Collider;
        Renderer = Renderer != null ? Renderer : enemyAttackTarget.Renderer;
        RigidBody = RigidBody != null ? RigidBody : enemyAttackTarget.RigidBody != null ? enemyAttackTarget.RigidBody : null;
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
