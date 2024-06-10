using UnityEngine.SceneManagement;

namespace NeonLadder.Events
{
    public class RestartScene : BaseGameEvent<RestartScene>
    {
        public override void Execute()
        {
            RestartCurrentScene();
        }

        private void RestartCurrentScene()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}
