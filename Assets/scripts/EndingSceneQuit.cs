using UnityEngine;
using System.Collections;

public class EndingSceneQuit : MonoBehaviour
{
    private void Start()
    {
        Debug.Log("EndingSceneQuit: Ending scene loaded, quitting in 5 seconds...");
        StartCoroutine(QuitAfterDelay(5f));
    }

    private IEnumerator QuitAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#elif UNITY_WEBGL
        Debug.Log("WebGL cannot quit. Show a final message instead.");
#else
        Application.Quit();
#endif
    }
}
