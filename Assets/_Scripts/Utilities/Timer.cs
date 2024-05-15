namespace Utilities
{
    public abstract class Timer
    {
        protected float InitialTime;

        protected float Time { get; set; }

        public bool IsRunning { get; protected set; }

        public float Progress => Time / InitialTime;

        public System.Action OnTimerStart = delegate { };
        public System.Action OnTimerStop = delegate { };

        protected Timer(float value)
        {
            InitialTime = value;
            IsRunning = false;
        }

        public void Start()
        {
            Time = InitialTime;
            if (IsRunning) return;

            IsRunning = true;
            OnTimerStart.Invoke();
        }

        public void Stop()
        {
            if (!IsRunning) return;

            IsRunning = false;
            OnTimerStop.Invoke();
        }

        public void Resume() => IsRunning = true;
        public void Pause() => IsRunning = false;
        public float GetTime() => Time;

        public abstract void Tick(float deltaTime);
    }
    
    public class CountdownTimer : Timer
    {
        public CountdownTimer(float value) : base(value) { }

        public override void Tick(float deltaTime)
        {
            if (IsRunning && Time > 0)
            {
                Time -= deltaTime;
            }

            if (IsRunning && Time <= 0)
            {
                Stop();
            }
        }

        public bool IsFinished() => Time <= 0;
        public void Reset() => Time = InitialTime;

        public void Reset(float newTime)
        {
            InitialTime = newTime;
            Reset();
        }
    }

    public class StopWatchTimer : Timer
    {
        public StopWatchTimer(float value) : base(0) { }

        public override void Tick(float deltaTime)
        {
            if (!IsRunning) return;
            Time += deltaTime;
        }
    
        public void Reset() => Time = 0;
    }

}