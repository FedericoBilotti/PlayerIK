using UnityEngine;

namespace Sensor
{
    public interface ISensor
    {
        RaycastHit Hit { get; }
        bool OnCollision { get; }
        
        /// <summary>
        /// Must be executed in fixed update
        /// </summary>
        void Execute();
    }
}