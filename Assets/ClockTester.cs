using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WaterKat.TimeW;

public class ClockTester : MonoBehaviour
{
    [ContextMenu("PrintTicks")]
    public void ReturnTicks()
    {
        Debug.Log("Ticks:" + ClockManager.deltaTicks);
    }
    [ContextMenu("PrintSeconds")]
    public void ReturnSeconds()
    {
        Debug.Log("Seconds:" + ClockManager.deltaSeconds);
    }
    [ContextMenu("PrintMinutes")]
    public void ReturnMinutes()
    {
        Debug.Log("Minutes:" + ClockManager.deltaMinutes);
    }
    public double seconds;
    private void Update()
    {
        seconds = ClockManager.deltaSeconds;
    }
}
