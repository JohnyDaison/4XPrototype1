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

    public static int MinutesToTurns(int totalMinutes, int minutesRemaining) {
        int turns = (int) Mathf.Ceil((float)Mathf.Max(0, totalMinutes-minutesRemaining) / GameController.instance.MinutesPerTurn);

        return turns;
    }

    public static string MinutesToTurnsString(int totalMinutes, int minutesRemaining) {
        int turns = MinutesToTurns(totalMinutes, minutesRemaining);

        return turns == 1 ? 
            $"{turns} turn" : 
            $"{turns} turns";
    }

    public static int MinutesPerHex(float baseSpeed) {
        float speedKpM = baseSpeed/60f;
        float distanceKm = GameController.instance.HexDiameter;
        int minutes = (int) Mathf.Ceil(distanceKm / speedKpM);
        return minutes;
    }

    public static float HexesPerTurn(float baseSpeed) {
        float hexes = GameController.instance.MinutesPerTurn / MinutesPerHex(baseSpeed);
        return hexes;
    }
}