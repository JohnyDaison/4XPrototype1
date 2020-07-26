using UnityEngine;

public static class MyUtils
{
    public static string MinutesToTimeDisplayString(int totalMinutes) {
        int minutes = totalMinutes % 60;
        string result = $"{minutes}m";
        
        int hours = (totalMinutes - minutes) / 60;
        if(hours > 0) {
            result = $"{hours}h " + result;
        }

        return result;
    }

    public static string MinutesToTurnsString(int totalMinutes) {
        int turns = (int) Mathf.Ceil((float)totalMinutes / GameController.instance.MinutesPerTurn);

        return $"{turns} turns";
    }

    public static int MinutesPerHex(float baseSpeed) {
        float speedKpM = baseSpeed/60f;
        float distanceKm = GameController.instance.HexDiameter;
        int minutes = (int) Mathf.Ceil(distanceKm / speedKpM);
        return minutes;
    }
}