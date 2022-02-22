using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Cinemachine;

public class SceneTransitionManager : MonoBehaviour
{
    static public SceneTransitionManager Instance;
    public List<SceneTransition> sceneTransitions;
    public SceneTransitionsList transitionsList;
    Coroutine coroutine;

    private void Start()
    {
        if (SceneTransitionManager.Instance != null)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }

        sceneTransitions = transitionsList.list;
        GameObject.DontDestroyOnLoad(this.gameObject);
    }

    public void LoadScene(string sceneName)
    {
        if (coroutine == null)
        {
            coroutine = StartCoroutine(RestartSceneCoroutine(sceneName, SceneManager.GetActiveScene().name));
        }
    }

    IEnumerator RestartSceneCoroutine(string sceneName, string cameFrom)
    {
        AsyncOperation async = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);

        while (!async.isDone)
        {
            yield return null;
        }

        coroutine = null;
        SceneTransition transition = sceneTransitions.Find(trans => trans.cameFrom == cameFrom && trans.goingTo == sceneName);

        if (transition != null)
        {
            GameObject.Find("Player").transform.position = transition.position;
            GameObject camera =  GameObject.Find("CM vcam1");
            Vector3 newPosition = new Vector3(transition.position.x, transition.position.y + 5, camera.transform.position.z);
            camera.GetComponent<CinemachineVirtualCamera>().ForceCameraPosition(newPosition, this.transform.rotation);
        }
    }
}

[System.Serializable]
public class SceneTransition
{
    public string cameFrom, goingTo;
    public Vector2 position;
}