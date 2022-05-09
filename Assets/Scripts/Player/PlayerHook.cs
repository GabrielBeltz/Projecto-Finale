using UnityEngine;
using System.Collections;

public class PlayerHook : MonoBehaviour
{
    public float AimTurningSpeed => rank > 2? 0.05f : 0.1f;
    public float BaseRange = 10f, PlayerSpeed = 10f, HookSpeed = 20f;
    public bool Traveling;
    bool jump;
    public ContactFilter2D contactFilter2D;
    RaycastHit2D[] raycastHit = new RaycastHit2D[1];
    float ModifiedRange => rank > 2 ? BaseRange * 1.75f : rank> 1 ? BaseRange * 1.5f : BaseRange;
    int rank;

    PlayerInputs inputs;
    Vector2 aimDirection, originalHookAimScale;
    Vector3 hitPosition;
    bool aiming;
    [SerializeField] GameObject hookAim, hookGameObject;
    [SerializeField] GameObject hookHead;
    [SerializeField] LineRenderer lineRenderer;
    IEnumerator hookTraveling;

    private void Start()
    {
        originalHookAimScale = hookAim.transform.localScale;   
        inputs = GetComponent<PlayerInputs>();
        PlayerController.Instance.OnPlayerDeath += UnnatachHook;
    }

    public void HandleHooking(int rank)
    {
        if(rank < 1) return;
        this.rank = rank;
        if(!Traveling)
        {
            if(inputs.GetInputDown("Hook")) StartAiming();
            if(inputs.GetInputUp("Hook")) LaunchHook();

            if(aiming) AimHook();
        }
        else if(inputs.GetInputDown("Hook")) Traveling = false;
    }

    void AimHook()
    {
        PlayerController.Instance.StopMoving = true;
        if(inputs.Inputs.RawX != 0 || inputs.Inputs.RawY != 0) aimDirection = Vector3.Slerp(aimDirection, new Vector3(inputs.Inputs.RawX, inputs.Inputs.RawY, 0), AimTurningSpeed);
        hookAim.transform.rotation = Quaternion.LookRotation(new Vector3(0, 0, -1f), aimDirection);
        hookAim.transform.position = transform.position + (Vector3)aimDirection;
        hookAim.transform.localScale = new Vector3(originalHookAimScale.x * Mathf.Sign(transform.lossyScale.x), originalHookAimScale.y, 1f);
    }

    public void UnnatachHook() => UnnatachHook(false); 
    public void UnnatachHook(bool Jump)
    {
        Time.timeScale = 1f;
        Traveling = false;

        if(Jump) jump = true;
        else
        {
            PlayerController.Instance.TimeLeftGrounded = Time.time;
            PlayerController.Instance._rb.velocity = Vector3.zero;
            PlayerController.Instance.FallImpact = false;
        }
    }

    void StartAiming()
    {
        hookAim.SetActive(true);
        aiming = true;
        aimDirection = Vector2.up;
        Time.timeScale = rank > 2 ? 0.01f : rank > 1? 0.33f : 0.67f;
    }

    public void EndAiming()
    {
        hookAim.SetActive(false);
        aiming = false;
        Time.timeScale = 1f;
    }

    void LaunchHook()
    {
        if(!aiming) return;
        PlayerController.Instance.StopMoving = false;
        hookAim.SetActive(false);
        aiming = false;
        Time.timeScale = 1f;
        lineRenderer.enabled = true;
        hookTraveling = HookTravel();
        raycastHit[0].point = Vector3.zero;
        if(Physics2D.Raycast(transform.position, aimDirection, contactFilter2D, raycastHit, ModifiedRange) > 0)
        {
            EnemyAttackTarget target;
            if(!raycastHit[0].collider.gameObject.TryGetComponent<EnemyAttackTarget>(out target)) hitPosition = raycastHit[0].point;
            else if(!(target.DamageReceived > 0)) hitPosition = raycastHit[0].point;
            else if(rank > 2)
            {
                //knockback???
            }
        }
        else hitPosition = transform.position + ((Vector3)aimDirection * ModifiedRange);

        StartCoroutine(hookTraveling);
    }

    IEnumerator HookTravel()
    {
        lineRenderer.enabled = true;
        lineRenderer.SetPosition(0, Vector3.zero);
        lineRenderer.SetPosition(1, Vector3.zero);
        hookGameObject.SetActive(true);
        hookHead.transform.localPosition = Vector3.zero;
        if(raycastHit[0].point != Vector2.zero) hitPosition += (Vector3)raycastHit[0].normal * 0.5f;
        RaycastHit2D[] newRaycastHit = new RaycastHit2D[1];
        float dist = Vector2.Distance(hookHead.transform.position, hitPosition) * 0.5f;

        for(float i = 0; i <= 1f; i += 0.01f * HookSpeed * (ModifiedRange/dist))
        {
            yield return new WaitForSeconds(0.01f);

            hookGameObject.transform.localScale = new Vector3(1f * Mathf.Sign(transform.lossyScale.x), 1f, 1f);
            hookHead.transform.position = Vector3.Lerp(hookHead.transform.position, hitPosition, i);
            lineRenderer.SetPosition(0, Vector3.zero);
            lineRenderer.SetPosition(1, hookHead.transform.position - transform.position);

            float distHeadToPoint = Vector2.Distance(hookHead.transform.position, hitPosition);
            if(Vector2.Distance(hookHead.transform.position, transform.position) > ModifiedRange) break;
            if(distHeadToPoint < 0.1f) break;
            Physics2D.Raycast(transform.position, aimDirection, contactFilter2D, newRaycastHit, distHeadToPoint);
            if(newRaycastHit[0].collider != null)
            {
                if(raycastHit[0].collider != null)
                    if(newRaycastHit[0].collider.gameObject.GetInstanceID() != raycastHit[0].collider.gameObject.GetInstanceID())
                    {
                        raycastHit[0].point = Vector2.zero;
                        break;
                    }
            }
        }

        if(raycastHit[0].point != Vector2.zero) HookHit(raycastHit[0]);
        else hookGameObject.SetActive(false);
    }

    public void HookHit(RaycastHit2D hit)
    {
        StopCoroutine(hookTraveling);

        StartCoroutine(PlayerTravel(hit));
    }

    IEnumerator PlayerTravel(RaycastHit2D hit)
    {
        Traveling = true;
        Vector3 target = hit.point + hit.normal;

        for(float i = 0; i < 1; i += 0.01f)
        {
            yield return new WaitForSeconds(0.1f / PlayerSpeed);
            hookGameObject.transform.localScale = new Vector3(1f * Mathf.Sign(transform.lossyScale.x), 1f, 1f);
            PlayerController.Instance._rb.velocity = Vector3.zero;
            transform.position = Vector3.Lerp(transform.position, target, i);
            hookHead.transform.position = hit.point;

            if(!Traveling) break;

            lineRenderer.SetPosition(0, Vector3.zero);
            lineRenderer.SetPosition(1, hookHead.transform.position - transform.position);
        }

        while(Traveling) 
        {
            hookGameObject.transform.localScale = new Vector3(1f * Mathf.Sign(transform.lossyScale.x), 1f, 1f);
            PlayerController.Instance._rb.velocity = Vector3.zero;
            transform.position = target;
            yield return new WaitForSeconds(0.01f);
        }

        if(jump) 
        {
            PlayerController.Instance.ExecuteJump(false);
            jump = false; 
        }

        lineRenderer.enabled = false;
        hookGameObject.SetActive(false);
    }
}
