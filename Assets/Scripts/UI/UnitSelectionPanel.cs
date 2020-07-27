using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnitSelectionPanel : MonoBehaviour {

	// Use this for initialization
	void Start () {
        selectionController = GameObject.FindObjectOfType<SelectionController>();
	}

    public Text Title;
    public Text Movement;
    public Text Time;
    public Text HexPathDistance;
    public Text HexPathDuration;

    public Button CityBuildButton;
    public Button WarehouseBuildButton;
    public Button MineBuildButton;
    public Button SkipButton;

    SelectionController selectionController;
    	
	// Update is called once per frame
	void Update () {
		
        if(selectionController.SelectedUnit != null)
        {
            Unit unit = selectionController.SelectedUnit;

            Title.text = unit.Name;

            Movement.text = $"Speed: {unit.BaseMoveSpeed}km/h";
            
            string remainingTime = MyUtils.MinutesToTimeDisplayString(unit.MinutesRemaining);
            string timePerTurn = MyUtils.MinutesToTimeDisplayString(GameController.instance.MinutesPerTurn);
            Time.text = $"Time: {remainingTime} / {timePerTurn}";

            Hex[] hexPath = unit.GetHexPath();
            int pathLength = hexPath == null || hexPath.Length == 0 ? 0 : hexPath.Length - 1;
            int pathDistance = pathLength * GameController.instance.HexDiameter;
            int minutesPerHex = MyUtils.MinutesPerHex(unit.BaseMoveSpeed);
            int pathMinutesExact = 0;
            if (hexPath != null) {
                for(int index = 1; index < hexPath.Length; index++)
                {
                    Hex hex = hexPath[index];
                    pathMinutesExact += (int) Mathf.Ceil(minutesPerHex * unit.MovementCostToEnterHex(hex));
                } 
            }
            
            string timeString = MyUtils.MinutesToTimeDisplayString(pathMinutesExact);
            string turnsString = MyUtils.MinutesToTurnsString(pathMinutesExact, unit.MinutesRemaining);

            HexPathDistance.text = $"Path distance: {pathDistance}km ({pathLength} tiles)";
            HexPathDuration.text = $"Path duration: {timeString} ({turnsString})";

            bool tileIsFree = unit.Hex.SurfaceStructure == null;
            bool canBuildStructure = unit.CanBuildCities && tileIsFree;
            CityBuildButton.gameObject.SetActive(unit.CanBuildCities);
            CityBuildButton.interactable = canBuildStructure;
            WarehouseBuildButton.gameObject.SetActive(unit.CanBuildCities);
            WarehouseBuildButton.interactable = canBuildStructure;
            MineBuildButton.gameObject.SetActive(unit.CanBuildCities);
            MineBuildButton.interactable = canBuildStructure;

            bool canSkip = !unit.AnimationIsPlaying && hexPath == null;
            SkipButton.interactable = canSkip;
        }

	}
}
