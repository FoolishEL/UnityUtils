using UnityEngine;

namespace Foolish.Utils.Editor.Windows
{
    [CreateAssetMenu(fileName = "WindowsSettings", menuName = "Foolish/Windows Settings")]
    public class WindowsSettingsAsset : ScriptableObject
    {
        [field: SerializeField]
        public string ProjectTitle { get; set; }
    }
}
