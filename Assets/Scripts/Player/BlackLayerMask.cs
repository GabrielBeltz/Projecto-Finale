using UnityEngine;

public class BlackLayerMask : MonoBehaviour
{
    public Camera myCam;
    public Material blackLayerMaterial;
    public Vector2 lerpLimitsX = new Vector2(-0.15f, 1f), lerpLimitsY = new Vector2(0f, 1f);
    Vector2 myPos, screen, lerp;

    private void Awake() => myCam ??= Camera.main;

    private void Update()
    {
        myPos = myCam.WorldToScreenPoint(transform.position);
        screen = new Vector2(Screen.width, Screen.height);
        lerp = new Vector2(Mathf.Lerp(lerpLimitsX.x, lerpLimitsX.y, myPos.x / screen.x), Mathf.Lerp(lerpLimitsY.x, lerpLimitsY.y, myPos.y / screen.y));
        blackLayerMaterial.SetVector("PlayerPosition", lerp);
    }
}
