using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class Mudacena : MonoBehaviour
{
    public int cena,esta;

    void Update()
    {
        if (Input.GetKeyDown("z"))
        {
            SceneManager.LoadScene(cena);
        }
        if (Input.GetKeyDown("p"))
        {
            SceneManager.LoadScene(esta);
        }
    }
}
