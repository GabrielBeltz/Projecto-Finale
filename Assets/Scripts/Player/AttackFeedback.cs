using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackFeedback : MonoBehaviour
{
    Renderer myRenderer;
    Coroutine coroutine;

    private void Start()
    {
        myRenderer = this.GetComponent<Renderer>();
    }

    public void CallFeedback(Vector3 size, Vector3 pos, float rotation, float time)
    {
        if (coroutine != null)
        {
            StopCoroutine(coroutine);
        }

        coroutine = StartCoroutine(Feedback(size * 2, pos, rotation, time));    
    }

    public IEnumerator Feedback(Vector3 size, Vector3 pos, float rotation, float time)
    {
        this.transform.position = pos;
        this.transform.localScale = new Vector3(size.z, size.y, size.x);
        this.transform.rotation = Quaternion.Euler(0,0, rotation);
        myRenderer.enabled = true;
        yield return new WaitForSeconds(time);
        myRenderer.enabled = false;
    }
}
