/*
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimerManager_OLD : MonoBehaviour
{
    private static TimerManager _singleton = null;
    private static readonly object _lockobject = new object();

    public static TimerManager instance
    {
        get
        {
            if (_singleton != null)
            {
                return _singleton;
            }
            else
            {
                GameObject newHome  = new GameObject();
                newHome.name = "TimerManager";
                newHome.AddComponent<TimerManager>();
                return newHome.GetComponent<TimerManager>();
            }
        }
    }

    TimerManager_OLD() { }

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

    public static bool AddTimer(float _duration, out string _timerName)
    {
        bool NewTimerAdded = false;

        if (_duration <= 0)
        {
            Debug.LogWarning("TimerManager.AddTimer() Warning: Cannot have negative TIMER duration!");

            _timerName = "";
            return NewTimerAdded;
        }

        string _tempTimerName = "Anon" + (UnityEngine.Random.Range(0, 10000)).ToString();
        while (instance.Timers.ContainsKey(_tempTimerName))
        {
            _tempTimerName = "Anon" + (UnityEngine.Random.Range(0, 10000)).ToString();
        }
        _timerName = _tempTimerName;

        instance.Timers.Add(_timerName, new float[2] { Time.time, Time.time + _duration });

        NewTimerAdded = true;

        return NewTimerAdded;
    }

    public static bool OverrideTimer(string _timerName, float _duration)
    {
        bool TimerOverrided = false;

        if (instance.Timers.ContainsKey(_timerName))
        {
            instance.Timers[_timerName] = new float[2] { Time.time, Time.time + _duration };

            TimerOverrided = true;
        }
        else
        {
            Debug.LogWarning("TimerManager.OverrideTimer() Warning: Desired TIMER to override doesn't exist!");
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
            Debug.LogWarning("TimerManager.RemoveTimer() Warning: Desired TIMER to remove doesn't exist!");
        }

        return TimerRemoved;
    }

    public static bool TimerMet(string _timerName)
    {
        bool TimerMet = false;

        if (instance.Timers.ContainsKey(_timerName))
        {
            float[] times = instance.Timers[_timerName];
            if (Time.time > times[1])
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
            return (Time.time - instance.Timers[_timerName][0]);
        }
        else
        {
            Debug.LogWarning("TimerManager.TimeElapsed() Warning: TIMER doesn't exist!");
        }
        return 0f;
    }

    public static float deltaTimeElapsed(string _timerName)
    {
        float _deltaTime = 0f;

        if (instance.Timers.ContainsKey(_timerName))
        {
            float[] _timerValues = instance.Timers[_timerName];
            _deltaTime = (Time.time - _timerValues[0]) / (_timerValues[1] - _timerValues[0]);
        }
        else
        {
            Debug.LogWarning("TimerManager.deltaTime() Warning: TIMER doesn't exist!");
        }

        return _deltaTime;
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

    public static bool AddTask(Action _timedAction, float _duration, out string _taskName)
    {
        bool TaskAdded = false;

        string _tempTaskName = "Anon" + (UnityEngine.Random.Range(0, 10000)).ToString();
        while (instance.Tasks.ContainsKey(_tempTaskName))
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

        instance.Tasks.Add(_tempTaskName, new TaskPair(_localTimerName, _timedAction));

        return TaskAdded;
    }

    public static bool AddTask(Action _timedAction, float _duration)
    {
        bool TaskAdded = false;

        string _tempTaskName = "Anon" + (UnityEngine.Random.Range(0, 10000)).ToString();
        while (instance.Tasks.ContainsKey(_tempTaskName))
        {
            _tempTaskName = "Anon" + (UnityEngine.Random.Range(0, 10000)).ToString();
        }

        string _localTimerName;
        if (!AddTimer(_duration, out _localTimerName))
        {
            _tempTaskName = "";
            Debug.LogWarning("TimerManager.AddTask() Warning: AddTimer() Failed!");
        }

        instance.Tasks.Add(_tempTaskName, new TaskPair(_localTimerName, _timedAction));

        return TaskAdded;
    }

    public static bool RemoveTask(string _taskName)
    {
        bool TaskRemoved = false;

        if (instance.Tasks.ContainsKey(_taskName))
        {
            instance.Tasks.Remove(_taskName);
        }
        else
        {
            Debug.LogWarning("TimerManager.RemoveTask() Warning: Desired TASK name doesn't exist!");
        }

        return TaskRemoved;
    }

    public static bool RemoveTasksWithTimer(string _timerName)
    {
        bool TasksRemoved = false;

        foreach (KeyValuePair<string, TaskPair> _keyValuePair in instance.Tasks)
        {
            if (_keyValuePair.Value.TimerName == _timerName)
            {
                instance.Tasks.Remove(_keyValuePair.Key);

                if (TasksRemoved == false)
                {
                    TasksRemoved = true;
                }
            }
        }

        return TasksRemoved;
    }

    private static bool RunTask(string _taskName)
    {
        bool TaskRun = false;

        try
        {
            instance.Tasks[_taskName].TimedAction();
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
        foreach (KeyValuePair<string,TaskPair> _keyValuePair in instance.Tasks.ToArray())
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
*/