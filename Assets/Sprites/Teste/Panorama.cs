using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Panorama : MonoBehaviour
{
    private float largura, posinicial;
    public GameObject cam;
    public float efeito;
    // Start is called before the first frame update
    void Start()
    {
        posinicial = transform.position.x;
        largura = GetComponent<SpriteRenderer>().bounds.size.x;

    }
    private void FixedUpdate()
    {
        float temp = (cam.transform.position.x * (1 - efeito));
        float dist = (cam.transform.position.x * efeito);

        transform.position = new Vector3(posinicial + dist, transform.position.y, transform.position.z);

        if (temp > posinicial + largura) posinicial += largura;
        else if (temp < posinicial - largura) posinicial -= largura;

    }
    
}
