using System;
using System.Collections.Generic;
using UnityEngine;

namespace WaterKat.TimeW
{
    public class ClockManager : MonoBehaviour
    {
        private static ClockManager _singleton = null;
        private static readonly object _lockobject = new object();
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
                    newHome.name = typeof(ClockManager).Name;
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

        [SerializeField]
        private DateTime activationTime = new DateTime();

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
        public static double TicksToSeconds(long _ticks)
        {
            return (_ticks / (double)10000000);
        }
        public static double TicksToMinutes(long _ticks)
        {
            return (_ticks / (double)600000000);
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
        public static int AddClock(double _duration)
        {
            int randomKey = ClockManager.instance.currentRandom.Next(-2147483648, 2147483647);
            while (ClockManager.instance.Clocks.ContainsKey(randomKey)||(randomKey==0))
            {
                randomKey = ClockManager.instance.currentRandom.Next(-2147483648, 2147483647);
            }

            ClockManager.instance.Clocks.Add(randomKey, new Clock(_duration));

            return randomKey;
        }
        public static int AddDelayedClock(double _delay, double _duration)
        {
            int randomKey = ClockManager.instance.currentRandom.Next(-2147483648, 2147483647);
            while (ClockManager.instance.Clocks.ContainsKey(randomKey) || (randomKey == 0))
            {
                randomKey = ClockManager.instance.currentRandom.Next(-2147483648, 2147483647);
            }

            ClockManager.instance.Clocks.Add(randomKey, new Clock(ClockManager.deltaSeconds+_delay, ClockManager.deltaSeconds + _delay+_duration));

            return randomKey;
        }
        public static void OverrideClock(int _clockID, double _duration)
        {
            if (ClockManager.instance.Clocks.ContainsKey(_clockID))
            {
                ClockManager.instance.Clocks[_clockID] = new Clock(_duration);
            }
        }
        public static void RemoveClock(int _clockID)
        {
            if (ClockManager.instance.Clocks.ContainsKey(_clockID))
            {
                ClockManager.instance.Clocks.Remove(_clockID);
            }
        }

        public static bool ClockMet(int _clockID)
        {
            if (ClockManager.instance.Clocks.ContainsKey(_clockID))
            {
                Clock _clock = ClockManager.instance.Clocks[_clockID];
                if (ClockManager.deltaSeconds >= _clock.EndTime)
                {
                    return true;
                }
            }
            return false;
        }
        public static bool ClockStarted(int _clockID)
        {
            if (ClockManager.instance.Clocks.ContainsKey(_clockID))
            {
                Clock _clock = ClockManager.instance.Clocks[_clockID];
                if (ClockManager.deltaSeconds < _clock.StartTime)
                {
                    return false;
                }
            }
            return true;
        }
        public static double ClockTimeElapsed(int _clockID)
        {
            if (ClockManager.instance.Clocks.ContainsKey(_clockID))
            {
                Clock _clock = ClockManager.instance.Clocks[_clockID];
                return ClockManager.deltaSeconds - _clock.StartTime;
            }
            return 0;
        }
        public static double ClockRelativeTimeElapsed(int _clockID)
        {
            if (ClockManager.instance.Clocks.ContainsKey(_clockID))
            {
                Clock _clock = ClockManager.instance.Clocks[_clockID];
                return (ClockManager.deltaSeconds - _clock.StartTime) / (_clock.EndTime - _clock.StartTime);
            }
            return 0;
        }
    }
}