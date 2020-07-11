using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildWarehouseButton : MonoBehaviour {

    public void BuildWarehouse()
    {
        Warehouse warehouse = new Warehouse();

        HexMap map = GameObject.FindObjectOfType<HexMap>();
        SelectionController sc = GameObject.FindObjectOfType<SelectionController>();

        map.SpawnStructureAt(
            warehouse,
            map.WarehousePrefab,
            sc.SelectedUnit.Hex.Q,
            sc.SelectedUnit.Hex.R,
            map.CurrentPlayer
        );
    }
	
}
