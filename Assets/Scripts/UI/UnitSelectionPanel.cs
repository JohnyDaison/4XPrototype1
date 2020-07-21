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

    public GameObject CityBuildButton;
    public GameObject SkipButton;

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


            if( unit.CanBuildCities && unit.Hex.City == null)
            {
                CityBuildButton.SetActive( true );
            }
            else
            {
                CityBuildButton.SetActive( false );
            }

            SkipButton.SetActive(!unit.AnimationIsPlaying && hexPath == null);

        }

	}
}
