using UnityEngine;

namespace Foolish.Utils
{
    public static class SceneReferenceExtensions
    {
        public static void LoadScene(this SceneReference reference)
        {
            if (reference is null)
            {
                Debug.LogError("SceneReference is null!");
                return;
            }
            UnityEngine.SceneManagement.SceneManager.LoadScene(reference);
        }
    }
}
