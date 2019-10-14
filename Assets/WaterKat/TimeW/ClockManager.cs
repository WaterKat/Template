using System;
using System.Collections.Generic;
using UnityEngine;

namespace WaterKat.TimeW
{
    public class ClockManager : MonoBehaviour
    {
        private static ClockManager _singleton = null;
        private static readonly object _lockobject = new object();

        [SerializeField]
        private DateTime activationTime = new DateTime();

        public static ClockManager instance
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
                    ClockManager newClockManager = newHome.AddComponent<ClockManager>();
                    return newClockManager;
                }
            }
        }

        private void Awake()
        {
            lock (_lockobject)
            {
                _singleton = this;

                ClockManager[] _clockList = FindObjectsOfType<ClockManager>();
                foreach (ClockManager clock in _clockList)
                {
                    if (clock != this)
                    {
                        clock.gameObject.SetActive(false);
                        Destroy(clock);
                        Destroy(clock.gameObject);
                    }
                }
                transform.gameObject.name = this.GetType().Name;
            }

            activationTime = DateTime.Now;
        }

        public static long deltaTicks
        {
            get
            {
                return (DateTime.Now - ClockManager.instance.activationTime).Ticks;
            }
        }

        public static double deltaSeconds
        {
            get
            {
                return (deltaTicks / (double)10000000);
            }
        }

        public static double deltaMinutes
        {
            get
            {
                return (deltaTicks / (double)600000000);
            }
        }

        [System.Serializable]
        private struct Clock
        {
            private double startTime;
            public double StartTime
            {
                get
                {
                    return startTime;
                }
                set
                {
                    startTime = Math.Min(value,endTime);
                }
            }
            private double endTime;
            public double EndTime
            {
                get
                {
                    return endTime;
                }
                set
                {
                    endTime = Math.Max(value, startTime);
                }
            }

            public Clock(double _startTime,double _endTime)
            {
                endTime = 0;
                startTime = 0;
                EndTime = _endTime;
                StartTime = _startTime;
            }
            public Clock(double _duration)
            {
                endTime = 0;
                startTime = 0;
                EndTime = ClockManager.deltaSeconds + _duration;
                StartTime = ClockManager.deltaSeconds;
            }
        }

        Dictionary<int, Clock> Clocks = new Dictionary<int, Clock>();
        System.Random currentRandom = new System.Random();
        public static int AddClock(float _duration)
        {
            int randomKey = ClockManager.instance.currentRandom.Next(-2147483648, 2147483647);
            while (ClockManager.instance.Clocks.ContainsKey(randomKey))
            {
                randomKey = ClockManager.instance.currentRandom.Next(-2147483648, 2147483647);
            }

            ClockManager.instance.Clocks.Add(randomKey, new Clock(_duration));

            return randomKey;
        }
    }
}