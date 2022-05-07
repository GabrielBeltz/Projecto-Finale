using UnityEngine;

public class SpiiiiinController : MonoBehaviour
{
    public float Speeeeed = 1f;

    void Update() => transform.rotation = Quaternion.Euler(0, 0, transform.rotation.eulerAngles.z + (Speeeeed * Time.deltaTime));
}
