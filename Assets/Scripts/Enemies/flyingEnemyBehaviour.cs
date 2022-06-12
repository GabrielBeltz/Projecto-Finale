using System.Collections;
using UnityEngine;

public class flyingEnemyBehaviour : MonoBehaviour
{
    public Vector2 HeightToTeleport = new Vector2(0.4f, 0.8f);
    bool Vulnerable, Attacking;
    [Range(0.2f, 10f)] public float TimeToReachPlayer = 1.5f, TimeToGoBack = 2.5f;
    [Range(0, 1f)] public float AccelerationGoing = 0.75f, AccelerationComing = 0.25f;
    public float VulnerableTime = 1f, minimalDistanceToAttack = 10f, overshootDistance = 5, lerpCutoff = 2f, CatchUpDistance = 20f;
    public LayerMask DoesntTeleportInside;
    public GameObject Knife;
    public Transform KnifeFloatingPoint;
    public SpriteRenderer mySprite;
    public Collider2D KnifeHitbox, myHitbox;
    public ParticleSystem TeleportParticleSystem;
    IEnumerator vulnerableTimer;

    private void Update()
    {
        mySprite.flipX = transform.position.x - PlayerController.Instance.transform.position.x < 0;
        if(transform.lossyScale.x > 0) mySprite.flipX = !mySprite.flipX;
        KnifeFloatingPoint.localPosition = new Vector3(mySprite.flipX? -0.8f : 0.8f, KnifeFloatingPoint.localPosition.y, KnifeFloatingPoint.localPosition.z);
        if(!Attacking) 
        {
            Knife.transform.rotation = Quaternion.LookRotation(new Vector3(0, 0, -1f), (PlayerController.Instance.transform.position - Knife.transform.position).normalized);
            Knife.transform.position = KnifeFloatingPoint.position;
        }

        if(Vulnerable) return;
        if(PlayerController.Instance.transform.position.y - transform.position.y < -CatchUpDistance) return;
        else if(transform.position.y - PlayerController.Instance.transform.position.y < -CatchUpDistance) StartCoroutine(TeleportToRandomLocation());
        else if(Vector2.Distance(transform.position, PlayerController.Instance.transform.position) > minimalDistanceToAttack) StartCoroutine(Attack());
        else StartCoroutine(TeleportToRandomLocation());
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
        Vector3 startingPoint = KnifeFloatingPoint.position;

        float TimeComparison = Time.time;

        for(float i = Time.time ; i < TimeComparison + TimeToReachPlayer; i = Time.time)
        {
            yield return new WaitForSeconds(Time.deltaTime);

            float positionPercentage = Mathf.InverseLerp(TimeComparison, TimeComparison + TimeToReachPlayer, Time.time);
            Vector3 SmoothedTarget = Vector3.Lerp(startingPoint, Knife.transform.position, AccelerationGoing);
            Knife.transform.position = Vector3.Lerp(SmoothedTarget, attackTarget, positionPercentage);
        }

        Knife.transform.rotation = Quaternion.LookRotation(new Vector3(0, 0, -1f), (KnifeFloatingPoint.position - Knife.transform.position).normalized);
        TimeComparison = Time.time;
        startingPoint = Knife.transform.position;

        for (float i = Time.time; i < TimeComparison + TimeToGoBack; i = Time.time)
        {
            yield return new WaitForSeconds(Time.deltaTime);

            float positionPercentage = Mathf.InverseLerp(TimeComparison, TimeComparison + TimeToGoBack, Time.time);
            Vector3 SmoothedTarget = Vector3.Lerp(startingPoint, Knife.transform.position, AccelerationComing);
            Knife.transform.position = Vector3.Lerp(KnifeFloatingPoint.position, SmoothedTarget, 1 - positionPercentage);
        }
        
        Knife.transform.position = KnifeFloatingPoint.position;
        Attacking = false;
        KnifeHitbox.enabled = false;
        CallVulnerable(false);
    }

    void CallVulnerable(bool value)
    {
        if(vulnerableTimer != null) StopCoroutine(vulnerableTimer);
        vulnerableTimer = VulnerableTimer(value);
        StartCoroutine(vulnerableTimer);
    }

    IEnumerator VulnerableTimer(bool teleport)
    {
        Vulnerable = true;
        if(teleport)
        {
            yield return new WaitForSeconds(VulnerableTime * 0.25f);
            Knife.SetActive(true);
            mySprite.enabled = true;
            yield return new WaitForSeconds(VulnerableTime * 0.5f);
        }
        else yield return new WaitForSeconds(VulnerableTime);
        
        Vulnerable = false;
        myHitbox.enabled = true;
    }

    IEnumerator TeleportToRandomLocation()
    {
        myHitbox.enabled = false;
        TeleportParticleSystem.Stop();
        Vulnerable = true;
        mySprite.enabled = false;
        Knife.SetActive(false);
        int counter = 0;
        bool found = false;
        Vector3 CheckPosition = Vector3.zero;
        while(!found)
        {
            counter++;
            if(counter > 99) gameObject.SetActive(false); // Se já tiver procurado 100 vzs e não achou melhor desligar
            yield return new WaitForSeconds(Time.deltaTime);
            Collider2D[] colision = new Collider2D[1];
            CheckPosition = GetSemirandomPosition();
            found = Physics2D.OverlapBoxNonAlloc(CheckPosition, transform.localScale * 1.25f, 0, colision, DoesntTeleportInside) == 0;
        }

        transform.position = CheckPosition;
        TeleportParticleSystem.Play();
        CallVulnerable(true);
    }

    Vector2 GetSemirandomPosition()
    {
        Vector2 screenXLimits = new Vector2(0, 1f);
        if(PlayerController.Instance.transform.position.x > 7f) screenXLimits.y = 0.5f;
        else if(PlayerController.Instance.transform.position.x < -7f) screenXLimits.x = 0.5f;
        else
        {
            bool side = Random.Range(0, 2) == 0;
            screenXLimits = side? new Vector2(0.7f, 1f) : new Vector2(0, 0.3f);
        }
        Vector2 ScreenPosition = new Vector3(Random.Range(screenXLimits.x, screenXLimits.y) * Screen.width, Random.Range(HeightToTeleport.x, HeightToTeleport.y) * Screen.height, 0f);
        return Camera.main.ScreenToWorldPoint(ScreenPosition);
    }
}