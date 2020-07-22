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
    public Text HexPath;

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

            Movement.text = string.Format(
                "{0}/{1}", 
                unit.MovementRemaining, 
                unit.Movement
            );

            Hex[] hexPath = unit.GetHexPath();
            HexPath.text  = hexPath == null ? "0" : hexPath.Length.ToString();

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
