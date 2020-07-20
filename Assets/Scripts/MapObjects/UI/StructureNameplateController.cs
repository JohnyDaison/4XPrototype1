using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StructureNameplateController : MonoBehaviour {

	// Use this for initialization
	void Start () {
        GameObject.FindObjectOfType<HexMap>().OnStructureCreated += CreateStructureNameplate;
	}

	// Update is called once per frame
	void Update () {
		
	}

    public GameObject StructureNameplatePrefab;

    public void CreateStructureNameplate( SurfaceStructure Structure, GameObject StructureGO )
    {
        GameObject nameGO = (GameObject)Instantiate(StructureNameplatePrefab, this.transform);
        nameGO.GetComponent<MapObjectNamePlate>().MyTarget = StructureGO;
        nameGO.GetComponentInChildren<StructureNameplate>().MyStructure = Structure;

    }
}
