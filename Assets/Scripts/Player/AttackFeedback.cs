using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackFeedback : MonoBehaviour
{
    Renderer myRenderer;
    IEnumerator coroutine;
    ParticleSystem particles;

    private void Start()
    {
        myRenderer = GetComponent<Renderer>();
        particles = GetComponentInChildren<ParticleSystem>();
        Material m = myRenderer.material;
        m.renderQueue++;
        myRenderer.material = m;
    }

    public void CallFeedback(Vector3 size, Vector3 pos, float rotation, float time, bool hit)
    {
        if (coroutine != null) StopCoroutine(coroutine);

        coroutine = Feedback(size * 2, pos, rotation, time, hit);
        StartCoroutine(coroutine);
    }

    public IEnumerator Feedback(Vector3 size, Vector3 pos, float rotation, float time, bool hit)
    {
        particles.transform.SetParent(null);
        transform.position = pos;
        transform.localScale = new Vector3(rotation != 0 ? -size.z : size.z, size.y, size.x);
        transform.localRotation = Quaternion.Euler(0,0, rotation);
        myRenderer.enabled = true;
        particles.transform.position = Vector3.Lerp(transform.parent.position, pos, 0.25f);
        if(hit) particles.Play();

        yield return new WaitForSeconds(time);
        myRenderer.enabled = false;
    }
}
