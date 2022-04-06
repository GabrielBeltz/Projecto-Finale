using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ParticleSystemJobs;

public class Item : MonoBehaviour
{
    public ItemObject item;

    public Collider2D[] myColliders;
    public Renderer[] myRenderers;
    public ParticleSystem myParticleSystem;

    public void Deactivate()
    {
        myParticleSystem.Stop(false, ParticleSystemStopBehavior.StopEmitting);

        foreach(var col in myColliders)
        {
            col.enabled = false;
        }

        foreach(var rdr in myRenderers)
        {
            rdr.enabled = false;
        }
    }
}
