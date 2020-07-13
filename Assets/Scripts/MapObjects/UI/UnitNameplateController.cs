using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitNameplateController : MonoBehaviour {

	// Use this for initialization
	void Start () {
        GameObject.FindObjectOfType<HexMap>().OnUnitCreated += CreateUnitNameplate;
	}

	// Update is called once per frame
	void Update () {
		
	}

    public GameObject UnitNameplatePrefab;

    public void CreateUnitNameplate( Unit unit, GameObject unitGO )
    {
        GameObject nameGO = (GameObject)Instantiate(UnitNameplatePrefab, this.transform);
        nameGO.GetComponent<MapObjectNamePlate>().MyTarget = unitGO;
        nameGO.GetComponentInChildren<UnitNameplate>().MyUnit = unit;

    }
}
