using UnityEngine.SceneManagement;

public sealed class UnitySceneLoader : ISceneLoader
{
    public void LoadScene(string sceneName)
    {
        if (string.IsNullOrWhiteSpace(sceneName))
        {
            return;
        }

        SceneManager.LoadScene(sceneName);
    }
}
