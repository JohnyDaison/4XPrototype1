using System.Collections.Generic;
using UnityEngine;

public class City : SurfaceStructure {
    float productionPerTurn = 100;

    BuildingJob buildingJob;
    Unit lastBuiltUnit;

    private List<string> names;

    public City()
    {
        GenerateCityName();
    }

    private void GenerateCityName() {
        names = new List<string>();
        names.Add("Strawfield");
        names.Add("Di-gihow'l");
        names.Add("Blue Lagoo-see");
        names.Add("Long-eared Forest");
        names.Add("Two Cans' Rest");

        Name = names[Random.Range(0, names.Count)];
    }

    override public void SetHex(Hex newHex)
    {
        if (Hex != null)
        {
            // Will a city ever LEAVE a hex and enter a new one?
            Hex.RemoveCity(this);
        }

        base.SetHex(newHex);

        Hex.AddCity(this);
    }

    public void DoTurn()
    {
        if (buildingJob != null)
        {
            float workLeft = buildingJob.DoWork(productionPerTurn);
            if (workLeft <= 0)
            {
                // Job is complete
                buildingJob = null;
                // TODO: Save overflow
            }
        }

        // For testing, build one dwarf
        if (buildingJob == null && lastBuiltUnit == null)
        {
            buildUnit("truck1");
        }

        // For testing, build one toucan after the dwarf
        if (buildingJob == null && lastBuiltUnit != null)
        {
            if (lastBuiltUnit.unitType.id == "truck1") {
                buildUnit("toucan1");
            }
        }
    }

    public BuildingBlueprint[] GetPossibleBuildings()
    {
        // TODO: Apply tech/uniqueness filters
        return BuildingDatabase.GetListOfBuilding();
    }

    bool cannotAddBuildingJob() {
        return (buildingJob != null && buildingJob.workLeft > 0);
    }

    bool buildUnit(string unitTypeId) {
        UnitType unitType = GameController.instance.UnitTypeDB.GetUnitTypeById(unitTypeId);
        if (unitType == null) {
            return false;
        }

        if (cannotAddBuildingJob()) {
            return false;
        }

        GameObject prefab = getUnitPrefab(unitType);

        if (prefab == null) {
            return false;
        }

        buildingJob = new BuildingJob(
                null,
                getUnitBuildName(unitType),
                getUnitCost(unitType),
                0,
                () => {
                    Unit newUnit = new Unit(unitType, prefab);

                    this.Hex.HexMap.SpawnUnitAt(newUnit, this.Hex.Q, this.Hex.R, this.player);
                    lastBuiltUnit = newUnit;
                },
                null
        );

        return true;
    }

    string getUnitBuildName(UnitType unitType) {
        return "Building " + unitType.name;
    }

    GameObject getUnitPrefab(UnitType unitType) {
        GameObject prefab = null;

        switch (unitType.id) {
            case "human1":
                prefab = this.Hex.HexMap.UnitHumanPrefab;
                break;
            case "dwarf1":
                prefab = this.Hex.HexMap.UnitDwarfPrefab;
                break;
            case "toucan1":
                prefab = this.Hex.HexMap.UnitToucanPrefab;
                break;
            case "truck1":
                prefab = this.Hex.HexMap.UnitTruckPrefab;
                break;
        }

        return prefab;
    }

    float getUnitCost(UnitType unitType) {
        return unitType.cost;
    }
}

