using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace WaterKat.OLD_Time
{
    public class TimerManager : MonoBehaviour
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
                    GameObject newHome = new GameObject();
                    newHome.name = "TimerManager";
                    newHome.AddComponent<TimerManager>();
                    return newHome.GetComponent<TimerManager>();
                }
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

        #region "Timers"

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
                        Debug.LogWarning("TimerManager.AddTimer() Warning: Cannot have negative TIMER duration!");
                        endTime = StartTime;
                    }
                }
            }

            public TimePair(float _startTime, float _endTime)
            {
                StartTime = _startTime;

                if (_endTime >= _startTime)
                {
                    endTime = _endTime;
                }
                else
                {
                    Debug.LogWarning("TimerManager.AddTimer() Warning: Cannot have negative TIMER duration!");
                    endTime = _startTime;
                }

            }

            public static TimePair TimePairWithDuration(float desiredDuration)
            {
                //    Debug.Log("TimerPair " + UnityEngine.Time.time + " " + (UnityEngine.Time.time + desiredDuration));
                return new TimePair(UnityEngine.Time.time, UnityEngine.Time.time + desiredDuration);
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

        public static bool TimerExists(string _timerName)
        {
            return instance.Timers.ContainsKey(_timerName);
        }

        public static bool TimerMet(string _timerName)
        {
            bool TimerMet = false;

            if (instance.Timers.ContainsKey(_timerName))
            {
                TimePair timePair = instance.Timers[_timerName];
                if (UnityEngine.Time.time > timePair.EndTime)
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
                return (UnityEngine.Time.time - instance.Timers[_timerName].StartTime);
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
                TimePair timePair = instance.Timers[_timerName];
                _deltaTime = (UnityEngine.Time.time - timePair.StartTime) / (timePair.EndTime - timePair.StartTime);
            }
            else
            {
                Debug.LogWarning("TimerManager.deltaTime() Warning: TIMER doesn't exist!");
            }

            return Mathf.Clamp(_deltaTime, 0, 1);
        }
        #endregion

        #region "Tasks"
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

            string _tempTaskName = "WKT" + Guid.NewGuid().ToString();
            while (instance.Tasks.ContainsKey(_tempTaskName))
            {
                _tempTaskName = "WKT" + Guid.NewGuid().ToString();
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

            string _tempTaskName = "WKT" + Guid.NewGuid().ToString();
            while (instance.Tasks.ContainsKey(_tempTaskName))
            {
                _tempTaskName = "WKT" + Guid.NewGuid().ToString();
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
        #endregion

        #region "LoopedTasks"
        struct LoopedTaskPair
        {
            public string TimerName;
            public Action<float> TimedAction;

            public LoopedTaskPair(string _timerName, Action<float> _timedAction)
            {
                TimerName = _timerName;
                TimedAction = _timedAction;
            }
        }

        Dictionary<string, LoopedTaskPair> LoopedTasks = new Dictionary<string, LoopedTaskPair>();

        public static bool AddLoopedTask(Action<float> _timedAction, float _duration, out string _taskName)
        {
            bool TaskAdded = false;

            string _tempTaskName = "WKLT" + Guid.NewGuid().ToString();
            while (instance.Tasks.ContainsKey(_tempTaskName))
            {
                _tempTaskName = "WKLT" + Guid.NewGuid().ToString();
            }

            string _localTimerName;
            if (!AddTimer(_duration, out _localTimerName))
            {
                _tempTaskName = "";
                Debug.LogWarning("TimerManager.AddLoopedTask() Warning: AddTimer() Failed!");
            }

            _taskName = _tempTaskName;

            instance.LoopedTasks.Add(_tempTaskName, new LoopedTaskPair(_localTimerName, _timedAction));

            return TaskAdded;
        }

        public static bool AddLoopedTask(Action<float> _timedAction, float _duration)
        {
            bool TaskAdded = false;

            string _tempTaskName = "WKLT" + Guid.NewGuid().ToString();
            while (instance.Tasks.ContainsKey(_tempTaskName))
            {
                _tempTaskName = "WKLT" + Guid.NewGuid().ToString();
            }

            string _localTimerName;
            if (!AddTimer(_duration, out _localTimerName))
            {
                _tempTaskName = "";
                Debug.LogWarning("TimerManager.AddLoopedTask() Warning: AddTimer() Failed!");
            }

            instance.LoopedTasks.Add(_tempTaskName, new LoopedTaskPair(_localTimerName, _timedAction));

            return TaskAdded;
        }

        public static bool RemoveLoopedTask(string _taskName)
        {
            bool TaskRemoved = false;

            if (instance.LoopedTasks.ContainsKey(_taskName))
            {
                instance.LoopedTasks.Remove(_taskName);
            }
            else
            {
                Debug.LogWarning("TimerManager.RemoveLoopedTask() Warning: Desired LOOPEDTASK name doesn't exist!");
            }

            return TaskRemoved;
        }

        public static bool RemoveLoopedTasksWithTimer(string _timerName)
        {
            bool TasksRemoved = false;

            foreach (KeyValuePair<string, LoopedTaskPair> _keyValuePair in instance.LoopedTasks)
            {
                if (_keyValuePair.Value.TimerName == _timerName)
                {
                    instance.LoopedTasks.Remove(_keyValuePair.Key);

                    if (TasksRemoved == false)
                    {
                        TasksRemoved = true;
                    }
                }
            }

            return TasksRemoved;
        }

        private static bool RunLoopedTask(string _taskName)
        {
            bool TaskRun = false;

            try
            {
                float timeElapsed = TimerManager.deltaTimeElapsed(instance.LoopedTasks[_taskName].TimerName);
                instance.LoopedTasks[_taskName].TimedAction(timeElapsed);
            }
            catch
            {
                Debug.LogWarning("TimerManager.RunLoopedTask() Warning: LOOPEDTASK failed to run!");
                return false;
            }

            TaskRun = true;
            return TaskRun;
        }
        #endregion

        void Update()
        {
            foreach (KeyValuePair<string, TaskPair> _keyValuePair in instance.Tasks.ToArray())
            {
                if (TimerMet(_keyValuePair.Value.TimerName))
                {
                    RunTask(_keyValuePair.Key);
                    RemoveTask(_keyValuePair.Key);
                    RemoveTimer(_keyValuePair.Value.TimerName);
                }
            }

            foreach (KeyValuePair<string, LoopedTaskPair> _keyValuePair in instance.LoopedTasks.ToArray())
            {
                if (TimerMet(_keyValuePair.Value.TimerName))
                {
                    RunLoopedTask(_keyValuePair.Key);
                    RemoveLoopedTask(_keyValuePair.Key);
                    RemoveTimer(_keyValuePair.Value.TimerName);
                }
                else
                {
                    RunLoopedTask(_keyValuePair.Key);
                }
            }
        }
    }
}