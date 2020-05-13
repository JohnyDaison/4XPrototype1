using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DayNightCycle : MonoBehaviour
{
    public float timeOfDay = 0; // 0 is midnight, dayLength/2 is noon
    public float timeSpeed = 0.1f;
    public float dayLength = 3600;
    public int daysPassed = 0;
    public Light lightComponent;
    
    // Start is called before the first frame update
    void Start()
    {
        lightComponent = this.GetComponent<Light>();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateTimeOfDay();

        UpdateLightDirection();
    }

    void UpdateTimeOfDay()
    {
        timeOfDay += timeSpeed;

        if (timeOfDay >= dayLength) {
            timeOfDay -= dayLength;
            daysPassed++;
        }
    }

    void UpdateLightDirection()
    {
        float sunAngle = 360 * (timeOfDay / dayLength);
        float unityLightAngle = sunAngle - 90;

        float angleDistanceFromMeridian = Math.Abs(sunAngle - 180);
        float angleOfMinimumLight = 90;

        lightComponent.intensity = Mathf.Max(0, 1 - Mathf.Abs(angleDistanceFromMeridian / angleOfMinimumLight));
        lightComponent.intensity = Mathf.Sqrt(lightComponent.intensity);

        this.transform.rotation = Quaternion.Euler(unityLightAngle, -60, 0);
    }

}
