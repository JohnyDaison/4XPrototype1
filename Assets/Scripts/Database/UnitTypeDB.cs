using System.Collections.Generic;

// should be singleton, probably
public class UnitTypeDB
{
    public UnitTypeDB() {
        CreateUnitTypes();
    }

    private Dictionary<string, UnitType> typeDict = new Dictionary<string, UnitType>();
    private List<string> typeList = new List<string>();

    // readonly by always returning a copy
    public List<string> GetAllUnitTypes() {
        return new List<string>(typeList);
    }

    public UnitType GetUnitTypeById(string id) {
        if (!typeList.Contains(id)) {
            return null;
        }
        
        return typeDict[id];
    }

    private bool AddUnitType(UnitType type) {
        if (type == null) {
            return false;
        }

        if (typeList.Contains(type.id)) {
            return false;
        }

        typeList.Add(type.id);
        typeDict.Add(type.id, type);

        return true;
    }

    private void CreateUnitTypes() {
        AddUnitType(CreateHuman());
        AddUnitType(CreateDwarf());
        AddUnitType(CreateElf());
        AddUnitType(CreateToucan());
        AddUnitType(CreateMerman());
        AddUnitType(CreateTruck());
    }

    private UnitType CreateHuman() {
        UnitType type = new UnitType();

        type.id = "human1";
        type.name = "Human";
        type.cost = 100f;
        type.movementPoints = 2;
        type.baseMovementSpeed = 10;
        type.canBuildCities = true;

        return type;
    }

    private UnitType CreateDwarf() {
        UnitType type = new UnitType();

        type.id = "dwarf1";
        type.name = "Dwarf";
        type.cost = 100f;
        type.movementPoints = 2;
        type.baseMovementSpeed = 10;
        type.canBuildCities = true;
        type.isHillWalker = true;

        return type;
    }

    private UnitType CreateElf() {
        UnitType type = new UnitType();

        type.id = "elf1";
        type.name = "Elf";
        type.cost = 100f;
        type.movementPoints = 2;
        type.baseMovementSpeed = 10;
        type.isForestWalker = true;

        return type;
    }

    private UnitType CreateToucan() {
        UnitType type = new UnitType();

        type.id = "toucan1";
        type.name = "Toucan";
        type.cost = 100f;
        type.movementPoints = 5;
        type.baseMovementSpeed = 25;
        type.isFlier = true;

        return type;
    }

    private UnitType CreateMerman() {
        UnitType type = new UnitType();

        type.id = "mermam1";
        type.name = "Merman";
        type.cost = 100f;
        type.movementPoints = 2;
        type.baseMovementSpeed = 10;
        type.isAquatic = true;

        return type;
    }

    private UnitType CreateTruck() {
        UnitType type = new UnitType();

        type.id = "truck1";
        type.name = "Truck";
        type.cost = 100f;
        type.storageStackCount = 1;
        type.storageStackVolume = 0.5f;
        type.cargoPartId = "trailer";
        type.movementPoints = 3;
        type.baseMovementSpeed = 15;
        type.isHillWalker = true;

        return type;
    }
}