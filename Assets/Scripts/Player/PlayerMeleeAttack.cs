using UnityEngine;

[CreateAssetMenu]
public class PlayerMeleeAttack : ScriptableObject
{
    public AttackDirection direction, up;
    public float damage;
    public float cooldown;
    public float range;
    public float knockback;
    public float selfKnockback;
    public string animatorTrigger;
    public ComboInfo comboInfo;
    public AudioClip sound;
}

[System.Serializable]
public class ComboInfo
{
    public float timeBetween;
    public string previousAttack;
}

[System.Serializable]
public enum AttackDirection
{
    Front, Back, Up, Down, Special
}
