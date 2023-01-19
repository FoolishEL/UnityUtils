using System;
using UnityEngine;

namespace Foolish.Utils.UI
{
    [Serializable]
    public class LoadSceneButtonHandler : AbstractButtonHandler
    {
        [SerializeField] private SceneReference sceneToLoad;
        public override void OnButtonClickedHandler()
        {
            if (sceneToLoad is not null)
            {
                sceneToLoad.LoadScene();
            }
        }
    }
}
