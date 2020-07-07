using System.Collections.Generic;
using UnityEngine;

public class City : MapObject {
    float productionPerTurn = 50;

    public Player player;

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
            buildUnit(Unit.UNIT_TYPE.DWARF);
        }

        // For testing, build one toucan after the dwarf
        if (buildingJob == null && lastBuiltUnit != null)
        {
            if (lastBuiltUnit.unitType == Unit.UNIT_TYPE.DWARF) {
                buildUnit(Unit.UNIT_TYPE.TOUCAN);
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

    bool buildUnit(Unit.UNIT_TYPE unitType) {
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

    string getUnitBuildName(Unit.UNIT_TYPE unitType) {
        string buildName = "Building A Unit";

        switch (unitType) {
            case Unit.UNIT_TYPE.HUMAN:
                buildName = "Human worker";
                break;
            case Unit.UNIT_TYPE.DWARF:
                buildName = "Dwarf Warrior";
                break;
        }

        return buildName;
    }

    GameObject getUnitPrefab(Unit.UNIT_TYPE unitType) {
        GameObject prefab = null;

        switch (unitType) {
            case Unit.UNIT_TYPE.HUMAN:
                prefab = this.Hex.HexMap.UnitHumanPrefab;
                break;
            case Unit.UNIT_TYPE.DWARF:
                prefab = this.Hex.HexMap.UnitDwarfPrefab;
                break;
            case Unit.UNIT_TYPE.TOUCAN:
                prefab = this.Hex.HexMap.UnitToucanPrefab;
                break;
        }

        return prefab;
    }

    float getUnitCost(Unit.UNIT_TYPE unitType) {
        float cost = 1f;

        switch (unitType) {
            case Unit.UNIT_TYPE.HUMAN:
                cost = 100f;
                break;
            case Unit.UNIT_TYPE.DWARF:
                cost = 100f;
                break;
            case Unit.UNIT_TYPE.TOUCAN:
                cost = 100f;
                break;
        }

        return cost;
    }
}

