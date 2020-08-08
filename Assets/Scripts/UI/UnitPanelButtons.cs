using UnityEngine;

public class UnitPanelButtons : MonoBehaviour {
    
    public void SkipUnitTurn()
    {
        SelectionController sc = GameObject.FindObjectOfType<SelectionController>();

        Unit u = sc.SelectedUnit;

        if(u == null)
        {
            Debug.LogError("How is this button active if there's no selected unit?!?!?!");
            return;
        }

        u.SkipThisUnit = true;

        // Might as well go to the next idle unit

        sc.SelectNextUnit( true );
    }

    public void BuildCity()
    {
        City city = new City();

        HexMap map = GameObject.FindObjectOfType<HexMap>();
        SelectionController sc = GameObject.FindObjectOfType<SelectionController>();

        map.SpawnCityAt(
            city,
            map.CityPrefab,
            sc.SelectedUnit.Hex.Q,
            sc.SelectedUnit.Hex.R,
            map.CurrentPlayer
        );
    }

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
    
    public void BuildOreMine()
    {
        OreMine oreMine = new OreMine();

        HexMap map = GameObject.FindObjectOfType<HexMap>();
        SelectionController sc = GameObject.FindObjectOfType<SelectionController>();

        map.SpawnStructureAt(
            oreMine,
            map.OreMinePrefab,
            sc.SelectedUnit.Hex.Q,
            sc.SelectedUnit.Hex.R,
            map.CurrentPlayer
        );
    }

    public void AddWaypoint() {
        SelectionController sc = GameObject.FindObjectOfType<SelectionController>();
        Unit unit = sc.SelectedUnit;
        
        if (unit.hexPathValid) {
            Hex[] hexPath = unit.GetHexPath();
            Hex lastHex = hexPath[hexPath.Length-1];
            unit.AddWaypoint(lastHex);
        } else {
            unit.AddWaypoint(unit.Hex);
        }
        
    }

    public void SwitchWaypointMode() {
        SelectionController sc = GameObject.FindObjectOfType<SelectionController>();
        sc.SelectedUnit.SwitchWaypointMode();
    }
}