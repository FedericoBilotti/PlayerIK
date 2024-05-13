using UnityEngine;

namespace Utilities
{
    public static class ExtensionMethodsVector3
    {
        public static Vector3 NormalizeWithoutY(this Vector3 vector)
        {
            Vector3 newVector = vector;
            newVector.y = 0;
            return newVector.normalized;
        }
    }
}