using UnityEngine;

namespace Sensor
{
    public interface ISensor
    {
        RaycastHit Hit { get; }
        bool OnCollision { get; }
        void Execute();
    }
}