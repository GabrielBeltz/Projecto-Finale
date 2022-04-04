using UnityEngine;

public class FootStepController : MonoBehaviour
{
    public AudioClip[] solidGroundClip, woodGroundClip, grassGroundClip, jumpClip;
    private AudioSource audioSource;
    [Range(-3f, 3f)] public float pitch;
    [Space] [SerializeField] private float deploymentHeight = 0.2f;
    public ContactFilter2D ContactFilter;
    private RaycastHit2D[] hit = new RaycastHit2D[1];

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.pitch = pitch;
    }

    private void FixedUpdate() => Physics2D.Raycast(transform.position, -Vector2.up * deploymentHeight, ContactFilter, hit, 1f);

    public void Jump() => audioSource.PlayOneShot(jumpClip[0]);

    public void Step()
    {
        if(hit[0].collider != null)
        {
            if(hit[0].collider.tag == "GrassGround") audioSource.PlayOneShot(RandomGrassClip());
            else if(hit[0].collider.tag == "SolidGround") audioSource.PlayOneShot(RandomSolidClip());
            else if(hit[0].collider.tag == "WoodGround") audioSource.PlayOneShot(RandomWoodClip());
        }
    }

    private AudioClip RandomGrassClip() => grassGroundClip[Random.Range(0, grassGroundClip.Length)];
    private AudioClip RandomSolidClip() => solidGroundClip[Random.Range(0, solidGroundClip.Length)];
    private AudioClip RandomWoodClip() => woodGroundClip[Random.Range(0, woodGroundClip.Length)];
    private AudioClip JumpingClip() => jumpClip[Random.Range(0, jumpClip.Length)];
}
