using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class platformov : MonoBehaviour
{
    [Header("Pos 1 com o eixo maior")]
    public Transform pos1;
    public Transform pos2;
    [Header("Lista de objetos destruidos para ativar")]
    public List <GameObject> alvos;
    [Header("começar a se mover para baixo = true")]
    public bool mv = true;
    public bool moverHorizontal, moververtical,livre =false;
    private bool enemies;
    public float vel;
    public float rot;
   


   
    void FixedUpdate()
    {
      
        if (alvos.Count == 0)
        {
             

            if (rot != 0) { transform.Rotate(0, 0, rot); livre = true; }
            if (vel > 0)
            {
                if (moververtical == true)
                {
                    if (transform.localPosition.y >= pos1.localPosition.y) mv = true;
                    if (transform.localPosition.y <= pos2.localPosition.y) mv = false;

                    if (mv == false) { transform.localPosition = new Vector2(transform.localPosition.x, transform.localPosition.y + vel * Time.deltaTime); }

                    else { transform.localPosition = new Vector2(transform.localPosition.x, transform.localPosition.y - vel * Time.deltaTime); }
                }
                {
                    if (moverHorizontal == true)
                    {
                        if (transform.localPosition.x >= pos1.localPosition.x)
                            mv = true;
                        if (transform.localPosition.x <= pos2.localPosition.x)
                            mv = false;

                        if (mv == false) { transform.localPosition = new Vector2(transform.localPosition.x + vel * Time.deltaTime, transform.localPosition.y); }

                        else { transform.localPosition = new Vector2(transform.localPosition.x - vel * Time.deltaTime, transform.localPosition.y); }
                    }

                }
                if (rot != 0) { transform.Rotate(0, 0, rot); }
            }
        }
        else
        {
            for (int x = 0; x < alvos.Count; x++)
            {if (alvos[x] == null) { alvos.Remove(alvos[x]); }} 
        }

    }
  

    public void OnCollisionEnter2D(Collision2D col)
    {

        if (col.gameObject.tag == "Player" && livre == false || col.gameObject.tag == "Enemy" && livre == false  ) 
        {         
            col.gameObject.transform.parent = transform;
        }
    }
    public void OnCollisionExit2D(Collision2D col)
    {
        if ( col.gameObject.tag == "Player"  || col.gameObject.tag == "Enemy" )
        {
            col.gameObject.transform.parent = null;
        }
    }
}

