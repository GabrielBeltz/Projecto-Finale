using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class platformov : MonoBehaviour
{
    [Header("Pos 1 com o eixo maior")]
    public Transform pos1;
    public Transform pos2;
    [Header("come�ar a se mover para baixo = true")]
    public bool mv = true;
    public bool moverHorizontal, moververtical;
    public float vel;
    [Header("n�o colcoar rota��o")]
    public float rot;

    void Update()
    {
        if (vel > 0)
        { if (moververtical == true)
            { if (transform.localPosition.y >= pos1.localPosition.y) mv = true;
              if (transform.localPosition.y <= pos2.localPosition.y) mv = false;

              if (mv == false) { transform.localPosition = new Vector2(transform.localPosition.x, transform.localPosition.y + vel * Time.deltaTime); }
               
               else { transform.localPosition = new Vector2(transform.localPosition.x, transform.localPosition.y - vel * Time.deltaTime); }
            }
            {if (moverHorizontal == true)
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

    public void OnCollisionEnter2D(Collision2D col)
    {
        if ( col.gameObject.tag == "Player" || col.gameObject.tag == "Enemy")
        {
            col.gameObject.transform.parent = this.transform;
        }
    }
    public void OnCollisionExit2D(Collision2D col)
    {
        if ( col.gameObject.tag == "Player" || col.gameObject.tag == "Enemy")
        {
            col.gameObject.transform.parent = null;
        }
    }
}
