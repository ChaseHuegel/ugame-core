using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Swordfish
{

public enum TimeOfDay
{
    NONE,
    DAWN,
    MORNING,
    NOON,
    AFTERNOON,
    EVENING
}

public class World : MonoBehaviour
{
    public event EventHandler<DayCycleEvent> OnDayCycleEvent;
    public class DayCycleEvent : Event
    {
        public TimeOfDay state;
    }


    [Header("Time Settings")]
    public float timeScale = 1.0f;
    public TimeOfDay dayState = TimeOfDay.NONE;
    public int day = 0;
    public float time = 0.0f;
    public int dayCycleSeconds = 60;

    [Header("References")]
    public GameObject sun;

    public bool updateSunPosition = true;
    public float updateSunProgress;
    public float updateSunTarget;
    public float updateSunStart;

    private void Start()
    {
        TickCycle();
    }

    private void Update()
    {
        TickCycle();
    }

    public void TickCycle()
    {
        time += (1f/dayCycleSeconds) * timeScale * Time.deltaTime;
        updateSunTarget = time;

        if ( (int)(time * 180) % 20 == 0 && updateSunPosition == false )
        {
            updateSunPosition = true;
        }

        if (sun != null && updateSunPosition)
        {
            sun.transform.rotation = Quaternion.Euler( Mathf.LerpUnclamped(updateSunStart * 180, updateSunTarget * 180, updateSunProgress), 0, 0 );

            if (updateSunProgress >= 1)
            {
                updateSunPosition = false;

                if (updateSunTarget >= 1f) updateSunTarget = 0;

                updateSunStart = updateSunTarget;
                updateSunProgress = 0;
            }
            else
            {
                updateSunProgress += 0.5f * Time.deltaTime;
            }
        }

        TimeOfDay cycleState = TimeOfDay.NONE;
        if (time > 1)
        {
            if (dayState != TimeOfDay.DAWN) cycleState = TimeOfDay.DAWN;
            time -= 1;
        }
        else if (time > 0.8f)
        {
            if (dayState != TimeOfDay.EVENING) cycleState = TimeOfDay.EVENING;
        }
        else if (time > 0.6f)
        {
            if (dayState != TimeOfDay.AFTERNOON) cycleState = TimeOfDay.AFTERNOON;
        }
        else if (time > 0.5f)
        {
            if (dayState != TimeOfDay.NOON) cycleState = TimeOfDay.NOON;
        }
        else if (time > 0.25)
        {
            if (dayState != TimeOfDay.MORNING) cycleState = TimeOfDay.MORNING;
        }
        else
        {
            if (dayState != TimeOfDay.DAWN) cycleState = TimeOfDay.DAWN;
        }

        if (cycleState != TimeOfDay.NONE)
        {
            dayState = cycleState;

            //  Invoke a day cycle event
            DayCycleEvent e = new DayCycleEvent{ state = cycleState };
            OnDayCycleEvent?.Invoke(this, e);
            if (e.cancel == true) { return; }   //  return if the event has been cancelled by any subscriber
        }
    }
}

}