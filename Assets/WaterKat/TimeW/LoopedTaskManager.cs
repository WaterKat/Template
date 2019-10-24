using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WaterKat.TimeW
{
    public class LoopedTaskManager : MonoBehaviour
    {
        private static LoopedTaskManager _singleton = null;
        private static readonly object _lockobject = new object();
        public static LoopedTaskManager instance
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
                    LoopedTaskManager newLoopedTimeManager = newHome.AddComponent<LoopedTaskManager>();
                    newHome.name = typeof(LoopedTaskManager).Name;
                    return newLoopedTimeManager;
                }
            }
        }
        private void Awake()
        {
            lock (_lockobject)
            {
                _singleton = this;

                LoopedTaskManager[] _taskList = FindObjectsOfType<LoopedTaskManager>();
                foreach (LoopedTaskManager loopedTask in _taskList)
                {
                    if (loopedTask != this)
                    {
                        loopedTask.gameObject.SetActive(false);
                        Destroy(loopedTask);
                        Destroy(loopedTask.gameObject);
                    }
                }
                transform.gameObject.name = this.GetType().Name;
            }
        }

        [System.Serializable]
        private struct LoopedTask
        {
            public int ClockID;
            public Action<double> LoopedTaskAction;
            
            public static LoopedTask blank
            {
                get
                {
                    return new LoopedTask() { ClockID = 0, LoopedTaskAction = Blank };
                }
            }
            static void Blank(double _blank) { }

            public LoopedTask(Action<double> _taskAction, int _clockID)
            {
                ClockID = _clockID;
                LoopedTaskAction = _taskAction;
            }
        }

        Dictionary<int, LoopedTask> LoopedTasks = new Dictionary<int, LoopedTask>();
        System.Random currentRandom = new System.Random();

        public static int AddLoopedTask(Action<double> _timedAction, double _loopedTime)
        {
            int randomKey = LoopedTaskManager.instance.currentRandom.Next(-2147483648, 2147483647);
            while (LoopedTaskManager.instance.LoopedTasks.ContainsKey(randomKey)||(randomKey==0))
            {
                randomKey = LoopedTaskManager.instance.currentRandom.Next(-2147483648, 2147483647);
            }

            LoopedTaskManager.instance.LoopedTasks.Add(randomKey, new LoopedTask(_timedAction, ClockManager.AddClock(_loopedTime)));

            return randomKey;
        }
        public static void RemoveLoopedTask(int _taskID)
        {
            if (LoopedTaskManager.instance.LoopedTasks.ContainsKey(_taskID))
            {
                ClockManager.RemoveClock(LoopedTaskManager.instance.LoopedTasks[_taskID].ClockID);
                LoopedTaskManager.instance.LoopedTasks.Remove(_taskID);
            }
        }
        public static void RemoveLoopedTasksWithClock(int _clockID)
        {
            foreach (KeyValuePair<int, LoopedTask> keyValuePair in LoopedTaskManager.instance.LoopedTasks)
            {
                if (keyValuePair.Value.ClockID == _clockID)
                {
                    LoopedTaskManager.instance.LoopedTasks.Remove(keyValuePair.Key);
                }
            }
        }
        private static void RunLoopedTask(int _taskID)
        {
            if (LoopedTaskManager.instance.LoopedTasks[_taskID].ClockID==0) { return; }
            try
            {
                LoopedTask loopedTask = LoopedTaskManager.instance.LoopedTasks[_taskID];
                loopedTask.LoopedTaskAction(ClockManager.ClockRelativeTimeElapsed(loopedTask.ClockID));
            }
            catch
            {
                Debug.LogError("TimeW.LoopedTimeManager.RunLoopedTask() Attempt to run task failed!");
            }
        }
        public static int AddDelayedLoopedTask(double _delay, Action<double> _timedAction, double _loopedTime)
        {
            int randomKey = LoopedTaskManager.instance.currentRandom.Next(-2147483648, 2147483647);
            while (LoopedTaskManager.instance.LoopedTasks.ContainsKey(randomKey) || (randomKey == 0))
            {
                randomKey = LoopedTaskManager.instance.currentRandom.Next(-2147483648, 2147483647);
            }

            LoopedTaskManager.instance.LoopedTasks.Add(randomKey, new LoopedTask(_timedAction, ClockManager.AddDelayedClock(_delay,_loopedTime)));

            return randomKey;
        }

        private void Update()
        {
            foreach (KeyValuePair<int, LoopedTask> _keyValuePair in LoopedTaskManager.instance.LoopedTasks.ToArray())
            {
                if (!ClockManager.ClockStarted(_keyValuePair.Value.ClockID)) { continue; }
                if (ClockManager.ClockMet(_keyValuePair.Value.ClockID))
                {
                    RunLoopedTask(_keyValuePair.Key);
                    RemoveLoopedTask(_keyValuePair.Key);
                }
                else
                {
                    RunLoopedTask(_keyValuePair.Key);
                }
            }
        }
    }
}