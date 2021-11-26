using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class LimboController : MonoBehaviour
{
    public bool IsInLimboMode = false;
    private float t = 0;
    [SerializeField] private GameObject SpriteMaksObj;
    Coroutine limboGeneration;

    void Start()
    {
        SpriteMaksObj.SetActive(false);
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.L))
        {
            IsInLimboMode = !IsInLimboMode;
        }

        LimboMode();
    }

    private void LimboMode()
    {
        if (limboGeneration != null)
        {
            StopCoroutine(limboGeneration);
        }

        limboGeneration = StartCoroutine(GeneratingMask());
    }

    private IEnumerator GeneratingMask()
    {
        switch (IsInLimboMode)
        {
            case true:
                SpriteMaksObj.SetActive(true);
                while (!(t > 1))
                {
                    yield return new WaitForEndOfFrame();
                    t += Time.deltaTime;
                    SpriteMaksObj.transform.localScale = new Vector3(Mathf.Lerp(0.5f, 800, t / 1f), Mathf.Lerp(0.5f, 800, t / 1f), 0);
                }
                break;
            case false:
                while (!(t < 0))
                {
                    yield return new WaitForEndOfFrame();
                    t -= Time.deltaTime * 3;
                    SpriteMaksObj.transform.localScale = new Vector3(Mathf.Lerp(0.5f, 800, t / 1f), Mathf.Lerp(0.5f, 800, t / 1f), 0);
                }
                SpriteMaksObj.SetActive(false);
                break;
        }

    }
}
