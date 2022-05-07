using System.Collections;
using UnityEngine;

public class flyingEnemyBehaviour : MonoBehaviour
{
    public bool Vulnerable, CanTeleport = true, Attacking;
    [Range(0.00001f, 1f)] public float KnifeSpeed;
    public float VulnerableTime = 1f, minimalDistanceToAttack = 10f, overshootDistance = 5, lerpCutoff = 0.9f, CatchUpDistance = 20f;
    public LayerMask DoesntTeleportInside;
    public GameObject Knife;
    public Transform KnifeFloatingPoint;
    public SpriteRenderer mySprite;
    public Collider2D KnifeHitbox;
    public ParticleSystem TeleportParticleSystem;

    private void Update()
    {
        mySprite.flipX = transform.position.x - PlayerController.Instance.transform.position.x > 0;
        KnifeFloatingPoint.localPosition = new Vector3(mySprite.flipX? -0.8f : 0.8f, KnifeFloatingPoint.localPosition.y, KnifeFloatingPoint.localPosition.z);
        if(!Attacking) 
        {
            Knife.transform.rotation = Quaternion.LookRotation(new Vector3(0, 0, -1f), (PlayerController.Instance.transform.position - Knife.transform.position).normalized);
            Knife.transform.localPosition = KnifeFloatingPoint.position;
        }

        if(Vulnerable) return;
        if(PlayerController.Instance.transform.position.y - transform.position.y < -CatchUpDistance && CanTeleport) StartCoroutine(TeleportToRandomLocation());
        if(transform.position.y - PlayerController.Instance.transform.position.y < -CatchUpDistance && CanTeleport) StartCoroutine(TeleportToRandomLocation());
        if(Vector2.Distance(transform.position, PlayerController.Instance.transform.position) > minimalDistanceToAttack) StartCoroutine(Attack());
        else if(CanTeleport) StartCoroutine(TeleportToRandomLocation());
    }

    IEnumerator Attack()
    {
        KnifeHitbox.enabled = true;
        Vulnerable = true;
        Attacking = true;
        Vector3 attackTarget = PlayerController.Instance.transform.position + ((PlayerController.Instance.transform.position - KnifeFloatingPoint.position).normalized * overshootDistance);
        Knife.transform.position = KnifeFloatingPoint.position;
        Knife.transform.rotation = Quaternion.LookRotation(new Vector3(0, 0, -1f), (PlayerController.Instance.transform.position - KnifeFloatingPoint.position).normalized);
        Knife.SetActive(true);

        for(float i = 0; i < 1; i += 1 * Time.deltaTime* KnifeSpeed)
        {
            yield return new WaitForSeconds(Time.deltaTime);

            Knife.transform.position = Vector3.Lerp(Knife.transform.position, attackTarget, i);
            if(i > lerpCutoff) break;
        }

        Knife.transform.rotation = Quaternion.LookRotation(new Vector3(0, 0, -1f), (KnifeFloatingPoint.position - Knife.transform.position).normalized);

        for(float i = 1; i > 0; i -= 1 * Time.deltaTime * KnifeSpeed * 1.33f)
        {
            yield return new WaitForSeconds(Time.deltaTime);
            
            Knife.transform.position = Vector3.Lerp(KnifeFloatingPoint.position, Knife.transform.position, i);
            if(1 - lerpCutoff + 0.1f > i) break;
        }
        
        Knife.transform.position = KnifeFloatingPoint.position;
        Attacking = false;
        KnifeHitbox.enabled = false;
        StartCoroutine(VulnerableTimer(false));
    }

    IEnumerator VulnerableTimer(bool teleport)
    {
        Vulnerable = true;
        if(teleport)
        {
            yield return new WaitForSeconds(VulnerableTime * 0.25f);
            Knife.SetActive(true);
            mySprite.enabled = true;
            yield return new WaitForSeconds(VulnerableTime * 0.25f);
            CanTeleport = true;
        }
        else yield return new WaitForSeconds(VulnerableTime);
        
        Vulnerable = false;
    }

    IEnumerator TeleportToRandomLocation()
    {
        TeleportParticleSystem.Stop();
        CanTeleport = false;
        mySprite.enabled = false;
        Knife.SetActive(false);
        int counter = 0;
        bool found = false;
        Vector3 CheckPosition = Vector3.zero;
        while(!found)
        {
            counter++;
            if(counter > 99) gameObject.SetActive(false); // Se j� tiver procurado 100 vzs e n�o achou melhor desligar
            yield return new WaitForSeconds(Time.deltaTime);
            Collider2D[] colision = new Collider2D[1];
            CheckPosition = GetSemirandomPosition();
            found = Physics2D.OverlapBoxNonAlloc(CheckPosition, transform.localScale, 0, colision, DoesntTeleportInside) == 0;
        }

        transform.position = CheckPosition;
        TeleportParticleSystem.Play();
        StartCoroutine(VulnerableTimer(true));
    }

    Vector2 GetSemirandomPosition()
    {
        Vector2 screenXLimits = new Vector2(0, 1f);
        if(PlayerController.Instance.transform.position.x > 7f) screenXLimits.y = 0.5f;
        if(PlayerController.Instance.transform.position.x < -7f) screenXLimits.x = 0.5f;
        Vector2 ScreenPosition = new Vector3(Random.Range(screenXLimits.x, screenXLimits.y) * Screen.width, 0.9f * Screen.height, 0f);
        return Camera.main.ScreenToWorldPoint(ScreenPosition);
    }
}