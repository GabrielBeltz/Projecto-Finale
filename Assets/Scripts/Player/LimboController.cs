using UnityEngine;

public class LimboController : MonoBehaviour
{
    public bool IsInLimboMode = false;
    public float limboDurationMod, limboTimer;
    private float limboDuration = 3f;
    [SerializeField] private GameObject SpriteMaksObj;

    void Start()
    {
        SpriteMaksObj.SetActive(false);
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.L) && limboTimer < limboDuration + limboDurationMod - 0.5f)
        {
            LimboMode();
        }

        if (IsInLimboMode)
        {
            limboTimer += Time.deltaTime;
            if (limboTimer > limboDuration + limboDurationMod)
            {
                LimboMode();
            }
        }
        else
        {
            limboTimer = Mathf.Max(limboTimer - Time.deltaTime, 0);
        }
    }

    private void LimboMode()
    {
        IsInLimboMode = !IsInLimboMode;
        SpriteMaksObj.SetActive(IsInLimboMode);
    }
}
