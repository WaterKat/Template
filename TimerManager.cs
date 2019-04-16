using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimerManager : MonoBehaviour
{
    private static TimerManager _singleton = null;
    private static readonly object _lockobject = new object();

    public static TimerManager main
    {
        get
        {
            return _singleton;
        }
    }

    TimerManager() { }

    void Awake()
    {
        lock (_lockobject)
        {
            _singleton = this;

            TimerManager[] _timerList = UnityEngine.Object.FindObjectsOfType<TimerManager>();
            if (_timerList.Length > 1)
            {
                Debug.LogWarning("TimerManager.Awake() Warning: Duplicate TIMERMANAGER(s) found!");
            }
            foreach (TimerManager _timerManager in _timerList)
            {
                if (_timerManager != this)
                {
                    _timerManager.gameObject.SetActive(false);
                    Destroy(_timerManager.gameObject);
                }
            }
        }
        transform.gameObject.name = "TimerManager";
    }

    Dictionary<string, float[]> Timers = new Dictionary<string, float[]>();

    public bool AddTimer(float _duration, out string _timerName)
    {
        bool NewTimerAdded = false;

        if (_duration <= 0)
        {
            Debug.LogWarning("TimerManager.AddTimer() Warning: Cannot have negative TIMER duration!");

            _timerName = "";
            return NewTimerAdded;
        }

        string _tempTimerName = "Anon" + (UnityEngine.Random.Range(0, 10000)).ToString();
        while (Timers.ContainsKey(_tempTimerName))
        {
            _tempTimerName = "Anon" + (UnityEngine.Random.Range(0, 10000)).ToString();
        }
        _timerName = _tempTimerName;

        Timers.Add(_timerName, new float[2] { Time.time, Time.time + _duration });

        NewTimerAdded = true;

        return NewTimerAdded;
    }


    public bool OverrideTimer(string _timerName, float _duration)
    {
        bool TimerOverrided = false;

        if (Timers.ContainsKey(_timerName))
        {
            Timers[_timerName] = new float[2] { Time.time, Time.time + _duration };

            TimerOverrided = true;
        }
        else
        {
            Debug.LogWarning("TimerManager.OverrideTimer() Warning: Desired TIMER to override doesn't exist!");
        }

        return TimerOverrided;
    }

    public bool RemoveTimer(string _timerName)
    {
        bool TimerRemoved = false;

        if (Timers.ContainsKey(_timerName))
        {
            Timers.Remove(_timerName);

            RemoveTasksWithTimer(_timerName);
        }
        else
        {
            Debug.LogWarning("TimerManager.RemoveTimer() Warning: Desired TIMER to remove doesn't exist!");
        }

        return TimerRemoved;
    }

    public bool TimerMet(string _timerName)
    {
        bool TimerMet = false;

        if (Timers.ContainsKey(_timerName))
        {
            float[] times = Timers[_timerName];
            if (Time.time > times[1])
            {
                TimerMet = true;
            }
        }

        return TimerMet;
    }

    struct TaskPair
    {
        public string TimerName;
        public Action TimedAction;

        public TaskPair(string _timerName, Action _timedAction)
        {
            TimerName = _timerName;
            TimedAction = _timedAction;
        }
    }

    Dictionary<string, TaskPair> Tasks = new Dictionary<string, TaskPair>();

    public bool AddTask(Action _timedAction, float _duration, out string _taskName)
    {
        bool TaskAdded = false;

        string _tempTaskName = "Anon" + (UnityEngine.Random.Range(0, 10000)).ToString();
        while (Tasks.ContainsKey(_tempTaskName))
        {
            _tempTaskName = "Anon" + (UnityEngine.Random.Range(0, 10000)).ToString();
        }

        string _localTimerName;
        if (!AddTimer(_duration, out _localTimerName))
        {
            _tempTaskName = "";
            Debug.LogWarning("TimerManager.AddTask() Warning: AddTimer() Failed!");
        }

        _taskName = _tempTaskName;

        return TaskAdded;
    }

    public bool RemoveTask(string _taskName)
    {
        bool TaskRemoved = false;

        if (Tasks.ContainsKey(_taskName))
        {
            Tasks.Remove(_taskName);
        }
        else
        {
            Debug.LogWarning("TimerManager.RemoveTask() Warning: Desired TASK name doesn't exist!");
        }

        return TaskRemoved;
    }

    public bool RemoveTasksWithTimer(string _timerName)
    {
        bool TasksRemoved = false;

        foreach (KeyValuePair<string, TaskPair> _keyValuePair in Tasks)
        {
            if (_keyValuePair.Value.TimerName == _timerName)
            {
                Tasks.Remove(_keyValuePair.Key);

                if (TasksRemoved == false)
                {
                    TasksRemoved = true;
                }
            }
        }

        return TasksRemoved;
    }

    private bool RunTask(string _taskName)
    {
        bool TaskRun = false;

        try
        {
            Tasks[_taskName].TimedAction();
        }
        catch 
        {
            Debug.LogWarning("TimerManager.RunTask() Warning: TASK failed to run!");
            return false;
        }

        TaskRun = true;
        return TaskRun;
    }

    void Update()
    {
        foreach (KeyValuePair<string,TaskPair> _keyValuePair in Tasks)
        {
            if (TimerMet(_keyValuePair.Value.TimerName))
            {
                RunTask(_keyValuePair.Key);
                RemoveTask(_keyValuePair.Key);
                RemoveTimer(_keyValuePair.Value.TimerName);
            }
        }
    }
}
