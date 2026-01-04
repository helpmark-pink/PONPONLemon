using UnityEngine;
using System;
using PONPONLemon.Core;

namespace PONPONLemon.Managers
{
    public class TimerManager : MonoBehaviour
    {
        public float CurrentTime { get; private set; }
        public float MaxTime { get; private set; }
        public float TimeRatio => MaxTime > 0 ? CurrentTime / MaxTime : 0f;
        public bool IsRunning { get; private set; }
        public bool IsTimeUp => CurrentTime <= 0;
        
        public event Action<float> OnTimeChanged;
        public event Action OnTimeUp;
        public event Action OnTimerStarted;
        public event Action OnTimerPaused;
        
        public void Initialize(float maxTime = -1)
        {
            MaxTime = maxTime > 0 ? maxTime : GameConstants.GAME_TIME;
            CurrentTime = MaxTime;
            IsRunning = false;
            OnTimeChanged?.Invoke(CurrentTime);
        }
        
        public void StartTimer()
        {
            if (IsTimeUp) return;
            IsRunning = true;
            OnTimerStarted?.Invoke();
        }
        
        public void PauseTimer()
        {
            IsRunning = false;
            OnTimerPaused?.Invoke();
        }
        
        public void ResumeTimer()
        {
            if (IsTimeUp) return;
            IsRunning = true;
        }
        
        public void ResetTimer()
        {
            CurrentTime = MaxTime;
            IsRunning = false;
            OnTimeChanged?.Invoke(CurrentTime);
        }
        
        public void AddTime(float seconds)
        {
            CurrentTime = Mathf.Min(CurrentTime + seconds, MaxTime);
            OnTimeChanged?.Invoke(CurrentTime);
        }
        
        private void Update()
        {
            if (!IsRunning || IsTimeUp) return;
            
            CurrentTime -= Time.deltaTime;
            
            if (CurrentTime <= 0)
            {
                CurrentTime = 0;
                IsRunning = false;
                OnTimeUp?.Invoke();
            }
            
            OnTimeChanged?.Invoke(CurrentTime);
        }
        
        public string GetFormattedTime()
        {
            int seconds = Mathf.CeilToInt(CurrentTime);
            return seconds.ToString();
        }
    }
}
