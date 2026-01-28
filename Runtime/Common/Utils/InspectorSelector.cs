namespace Foolish.Utils
{
    [System.AttributeUsage(System.AttributeTargets.Field)]
    public class InspectorSelector : UnityEngine.PropertyAttribute
    {
        public System.Type AbstractType { get; }

        public InspectorSelector(System.Type abstractType) => AbstractType = abstractType;
    }
}