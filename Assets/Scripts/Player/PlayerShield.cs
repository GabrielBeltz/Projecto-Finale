using UnityEngine;
using System.Collections;

public class PlayerShield : MonoBehaviour
{
    PlayerInputs inputs;

    public float DurationBase = 1f, Cooldown;
    public bool Active;
    public SpriteRenderer ShieldSprite;

    float cooldownTimer;

    private void Awake() => inputs = GetComponent<PlayerInputs>();

    public float HandleShield(int rank)
    {
        if(DurationBase * 2 > Cooldown) Cooldown = DurationBase * 2;
        cooldownTimer -= Time.deltaTime;

        if(rank < 1) return 1f;

        if(inputs.GetInputDown("Shield") && !Active && cooldownTimer < 0) SetActive(true, rank);

        return GetMoveMultiplier(rank);
    }

    void Deactivate() => SetActive(false, 0);

    void SetActive(bool active, int rank)
    {
        Active = active;
        ShieldSprite.enabled = active;
        FindObjectOfType<AudioManager>().Play("ShieldUp");
        if(active) 
        {
            cooldownTimer = Cooldown;
            gameObject.layer = 11;
            StartCoroutine(Deactivate(DurationBase + (0.33f * rank - 1)));
        }
        else
        {
            gameObject.layer = 10;
        }
    }

    IEnumerator Deactivate(float duration)
    {
        yield return new WaitForSeconds(duration);
        FindObjectOfType<AudioManager>().Play("ShieldDown");
        Deactivate();
    }

    float GetMoveMultiplier(int rank) => !Active ? 1f : rank > 2 ? 1f : rank > 1 ? 0.5f : 0;
}
