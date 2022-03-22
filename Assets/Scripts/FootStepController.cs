using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class FootStepController : MonoBehaviour
{
    public AudioClip[] solidGroundClip, woodGroundClip, grassGroundClip;
    private AudioSource audioSource;
    [Range(-3f, 3f)] public float pitch;
    [Space]
    [SerializeField] private float deploymentHeight = 0.2f;
    public LayerMask groundLayer;
    private RaycastHit2D hit;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.pitch = pitch;
    }

    private void FixedUpdate()
    {
        Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.down) * deploymentHeight, Color.yellow);
        hit = Physics2D.Raycast(transform.position, -Vector2.up * deploymentHeight);
    }

    public void Step()
    {
        if(hit.collider != null)
        { 
            if(hit.collider.tag == "GrassGround")
            {
                AudioClip grassGroundClip = RandomGrassClip();
                audioSource.PlayOneShot(grassGroundClip);
            }

            if(hit.collider.tag == "SolidGround")
            {
                AudioClip solidGroundClip = RandomSolidClip();
                audioSource.PlayOneShot(solidGroundClip);
            }

            if(hit.collider.tag == "WoodGround")
            {
                AudioClip woodGroundClip = RandomWoodClip();
                audioSource.PlayOneShot(woodGroundClip);
            }

            Debug.Log("Did Hit a Ground Tag: " + hit.collider.tag);
        }
        else
        {
            Debug.Log("Did not Hit");
        }
    }

    private AudioClip RandomGrassClip()
    {
        return grassGroundClip[Random.Range(0, grassGroundClip.Length)];
    }

    private AudioClip RandomSolidClip()
    {
        return solidGroundClip[Random.Range(0, solidGroundClip.Length)];
    }

    private AudioClip RandomWoodClip()
    {
        return woodGroundClip[Random.Range(0, woodGroundClip.Length)];
    }

}
