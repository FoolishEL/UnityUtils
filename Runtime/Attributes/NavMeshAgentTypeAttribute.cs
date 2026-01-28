#if UNITY_AI_NAVIGATION
using System;
using UnityEngine;

namespace Foolish.Utils
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class NavMeshAgentTypeAttribute : PropertyAttribute
    {
    }
    
}
#endif