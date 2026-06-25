using UnityEngine.SceneManagement;

public sealed class UnitySceneRestartService : ISceneRestartService
{
    public void RestartActiveScene()
    {
        Scene activeScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(activeScene.buildIndex);
    }
}
