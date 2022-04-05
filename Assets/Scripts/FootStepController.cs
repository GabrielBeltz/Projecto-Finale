using UnityEngine;

public class FootStepController : MonoBehaviour
{
    public AudioClip[] solidGroundClip, woodGroundClip, grassGroundClip, jumpClip;
    private AudioSource audioSource;
    [Range(-3f, 3f)] public float pitch;
    [Space] [SerializeField] private float deploymentHeight = 0.2f;
    public ContactFilter2D ContactFilter;
    private RaycastHit2D[] hit = new RaycastHit2D[1];

    void Awake() => audioSource = GetComponent<AudioSource>();

    private void FixedUpdate() => Physics2D.Raycast(transform.position, -Vector2.up * deploymentHeight, ContactFilter, hit, 1f);

    public void Jump() => PlayOneShot(jumpClip[0], pitch);

    public void Step()
    {
        if(hit[0].collider == null) return;
        if(hit[0].collider.tag == "GrassGround") PlayOneShot(RandomGrassClip(), pitch);
        else if(hit[0].collider.tag == "SolidGround") PlayOneShot(RandomSolidClip(), pitch);
        else if(hit[0].collider.tag == "WoodGround") PlayOneShot(RandomWoodClip(), pitch);
    }

    public void PlayOneShot(AudioClip audio, float capiTCHE)
    {
        audioSource.pitch = capiTCHE;
        audioSource.PlayOneShot(audio);
    }

    public AudioClip RandomGrassClip() => grassGroundClip[Random.Range(0, grassGroundClip.Length)];
    public AudioClip RandomSolidClip() => solidGroundClip[Random.Range(0, solidGroundClip.Length)];
    public AudioClip RandomWoodClip() => woodGroundClip[Random.Range(0, woodGroundClip.Length)];
}
