using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WaterKat.TimeW
{
    public class TaskManager : MonoBehaviour
    {
        private static TaskManager _singleton = null;
        private static readonly object _lockobject = new object();
        public static TaskManager instance
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
                    TaskManager newTaskManager = newHome.AddComponent<TaskManager>();
                    newHome.name = typeof(TaskManager).Name;
                    return newTaskManager;
                }
            }
        }
        private void Awake()
        {
            lock (_lockobject)
            {
                _singleton = this;

                TaskManager[] _taskList = FindObjectsOfType<TaskManager>();
                foreach (TaskManager task in _taskList)
                {
                    if (task != this)
                    {
                        task.gameObject.SetActive(false);
                        Destroy(task);
                        Destroy(task.gameObject);
                    }
                }
                transform.gameObject.name = this.GetType().Name;
            }
        }

        [System.Serializable]
        private struct Task
        {
            public int ClockID;
            public Action TaskAction;

            public Task(Action _taskAction, int _clockID)
            {
                ClockID = _clockID;
                TaskAction = _taskAction;
            }
        }

        Dictionary<int, Task> Tasks = new Dictionary<int, Task>();
        System.Random currentRandom = new System.Random();

        public static int AddTask(Action _timedAction, double _delay)
        {
            int randomKey = TaskManager.instance.currentRandom.Next(-2147483648, 2147483647);
            while (TaskManager.instance.Tasks.ContainsKey(randomKey)||(randomKey == 0))
            {
                randomKey = TaskManager.instance.currentRandom.Next(-2147483648, 2147483647);
            }

            TaskManager.instance.Tasks.Add(randomKey, new Task(_timedAction, ClockManager.AddClock(_delay)));

            return randomKey;
        }
        public static void RemoveTask(int _taskID)
        {
            if (TaskManager.instance.Tasks.ContainsKey(_taskID))
            {
                ClockManager.RemoveClock(TaskManager.instance.Tasks[_taskID].ClockID);
                TaskManager.instance.Tasks.Remove(_taskID);
            }
        }
        public static void RemoveTasksWithClock(int _clockID)
        {
            foreach (KeyValuePair<int,Task> keyValuePair in TaskManager.instance.Tasks)
            {
                if (keyValuePair.Value.ClockID == _clockID)
                {
                    TaskManager.instance.Tasks.Remove(keyValuePair.Key);
                }
            }
        }
        private static void RunTask(int _taskID)
        {
            try
            {
                TaskManager.instance.Tasks[_taskID].TaskAction();
            }
            catch
            {
                Debug.LogError("TimeW.TaskManager.RunTask() Attempt to run task failed!");
            }
        }

        private void Update()
        {
            foreach (KeyValuePair<int, Task> _keyValuePair in TaskManager.instance.Tasks.ToArray())
            {
                if (ClockManager.ClockMet(_keyValuePair.Value.ClockID))
                {
                    RunTask(_keyValuePair.Key);
                    RemoveTask(_keyValuePair.Key);
                }
            }
        }
    }
}