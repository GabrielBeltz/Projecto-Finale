using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class platformov : MonoBehaviour
{
  
    public Vector3  pos1,pos2;
    private Vector3 atual,isso;
    public float vel,rot;
    void Start()
    { atual = pos1;}
    void Update()
    {if (vel > 0)
        {
            isso = this.transform.localPosition;
            float dist = Vector3.Distance(isso, pos1);
            float dist2 = Vector3.Distance(isso, pos2);
            transform.localPosition = Vector3.Lerp(transform.localPosition, atual, Time.deltaTime * vel);
            if (dist < 0.5f)
            { atual = pos2; }
            if (dist2 < 0.5f)
            {atual = pos1; }
        }
        if (rot != 0) { transform.Rotate(0, 0,rot); }
    }
    
}

