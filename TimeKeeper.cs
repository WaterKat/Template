using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeKeeper : MonoBehaviour
{
    private static TimeKeeper _singleton = null;
    private static readonly object _lockobject = new object();

    public static TimeKeeper instance
    {
        get
        {
            if (_singleton != null)
            {
                return _singleton;
            }
            else
            {
                GameObject newHome = new GameObject();
                newHome.name = "TimerManager";
                newHome.AddComponent<TimeKeeper>();
                return newHome.GetComponent<TimeKeeper>();
            }
        }
    }

    TimeKeeper() { }

    void Awake()
    {
        lock (_lockobject)
        {
            _singleton = this;

            TimeKeeper[] _timerList = UnityEngine.Object.FindObjectsOfType<TimeKeeper>();
            if (_timerList.Length > 1)
            {
                Debug.LogWarning(this.GetType().ToString()+": Duplicate TIMERMANAGER(s) found!");
            }
            foreach (TimeKeeper _timerManager in _timerList)
            {
                if (_timerManager != this)
                {
                    _timerManager.gameObject.SetActive(false);
                    Destroy(_timerManager.gameObject);
                }
            }
        }

        transform.gameObject.name = this.GetType().ToString();
    }

    struct TimePair
    {
        public float StartTime;

        float endTime;
        public float EndTime
        {
            get { return endTime; }
            set
            {
                if (value >= StartTime)
                {
                    endTime = value;
                }
                else
                {
                    Debug.LogWarning(this.GetType().ToString() + ": Cannot have negative TIMER duration!");
                    endTime = StartTime;
                }
            }
        }

        public TimePair(float _startTime,float _endTime)
        {
            StartTime = _startTime;

            if (_endTime >= _startTime)
            {
                endTime = _endTime;
            }
            else
            {
                Debug.LogWarning(this.GetType().ToString() + ": Cannot have negative TIMER duration!");
                endTime = _startTime;
            }

        }

        public static TimePair TimePairWithDuration(float desiredDuration)
        {
            return new TimePair(Time.time, Time.time + desiredDuration);
        }
    }

    Dictionary<string, TimePair> Timers = new Dictionary<string, TimePair>();

    public static bool AddTimer(float _duration, out string _timerName)
    {
        bool NewTimerAdded = false;
        _timerName = "";

        string _tempTimerName = "WK" + Guid.NewGuid().ToString();
        while (instance.Timers.ContainsKey(_tempTimerName))
        {
            _tempTimerName = "WK" + Guid.NewGuid().ToString();
        }
        _timerName = _tempTimerName;

        instance.Timers.Add(_timerName, TimePair.TimePairWithDuration(_duration));

        NewTimerAdded = true;

        return NewTimerAdded;
    }

    public static bool OverrideTimer(string _timerName, float _duration)
    {
        bool TimerOverrided = false;

        if (instance.Timers.ContainsKey(_timerName))
        {
            instance.Timers[_timerName] = TimePair.TimePairWithDuration(_duration);

            TimerOverrided = true;
        }
        else
        {
            Debug.LogWarning(this.GetType().ToString() + ": Desired TIMER to override doesn't exist!");
        }

        return TimerOverrided;
    }

    public static bool RemoveTimer(string _timerName)
    {
        bool TimerRemoved = false;

        if (instance.Timers.ContainsKey(_timerName))
        {
            instance.Timers.Remove(_timerName);

            RemoveTasksWithTimer(_timerName);
        }
        else
        {
            Debug.LogWarning(this.GetType().ToString() + ": Desired TIMER to remove doesn't exist!");
        }

        return TimerRemoved;
    }

    public static bool TimerMet(string _timerName)
    {
        bool TimerMet = false;

        if (instance.Timers.ContainsKey(_timerName))
        {
            TimePair timePair = instance.Timers[_timerName];
            if (Time.time > timePair.EndTime)
            {
                TimerMet = true;
            }
        }

        return TimerMet;
    }

    public static float TimeElapsed(string _timerName)
    {
        if (instance.Timers.ContainsKey(_timerName))
        {
            return (Time.time - instance.Timers[_timerName].StartTime);
        }
        else
        {
            Debug.LogWarning(this.GetType().ToString() + ": TIMER doesn't exist!");
        }
        return 0f;
    }

    public static float deltaTimeElapsed(string _timerName)
    {
        float _deltaTime = 0f;

        if (instance.Timers.ContainsKey(_timerName))
        {
            TimePair timePair = instance.Timers[_timerName];
            _deltaTime = (Time.time - timePair.StartTime) / (timePair.EndTime - timePair.StartTime);
        }
        else
        {
            Debug.LogWarning(this.GetType().ToString() + ": TIMER doesn't exist!");
        }

        return _deltaTime;
    } 
}