using UnityEngine;
using UnityEngine.SceneManagement;

public class DebuggingShit : MonoBehaviour
{
    public bool SceneReloading;

    private void Update()
    {
        if (SceneReloading && Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
