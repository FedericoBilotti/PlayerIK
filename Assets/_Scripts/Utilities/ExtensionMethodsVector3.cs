using UnityEngine;

namespace Utilities
{
    public static class ExtensionMethodsVector3
    {
        /// <summary>
        /// Normaliza el vector sin el eje X
        /// </summary>
        /// <param name="vector"></param>
        /// <returns></returns>
        public static Vector3 NormalizeWithoutX(this Vector3 vector)
        {
            Vector3 newVector = vector;
            newVector.x = 0;
            return newVector.normalized;
        }
        
        /// <summary>
        /// Normaliza el vector sin el eje Y
        /// </summary>
        /// <param name="vector"></param>
        /// <returns></returns>
        public static Vector3 NormalizeWithoutY(this Vector3 vector)
        {
            Vector3 newVector = vector;
            newVector.y = 0;
            return newVector.normalized;
        }
        
        /// <summary>
        /// Normaliza el vector sin el eje Z
        /// </summary>
        /// <param name="vector"></param>
        /// <returns></returns>
        public static Vector3 NormalizeWithoutZ(this Vector3 vector)
        {
            Vector3 newVector = vector;
            newVector.z = 0;
            return newVector.normalized;
        }
    }
}