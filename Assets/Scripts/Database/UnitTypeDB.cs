// should be singleton, probably
public class UnitTypeDB: GeneralTypeDB<UnitType>
{
    public UnitTypeDB() {
        CreateUnitTypes();
    }

    private void CreateUnitTypes() {
        AddType(CreateHuman());
        AddType(CreateDwarf());
        AddType(CreateElf());
        AddType(CreateToucan());
        AddType(CreateMerman());
        AddType(CreateTruck());
    }

    private UnitType CreateHuman() {
        UnitType type = new UnitType();

        type.ID = "human1";
        type.Name = "Human";
        type.cost = 100f;
        type.baseMovementSpeed = 10;
        type.canBuildCities = true;

        return type;
    }

    private UnitType CreateDwarf() {
        UnitType type = new UnitType();

        type.ID = "dwarf1";
        type.Name = "Dwarf";
        type.cost = 100f;
        type.baseMovementSpeed = 10;
        type.canBuildCities = true;
        type.isHillWalker = true;

        return type;
    }

    private UnitType CreateElf() {
        UnitType type = new UnitType();

        type.ID = "elf1";
        type.Name = "Elf";
        type.cost = 100f;
        type.baseMovementSpeed = 10;
        type.isForestWalker = true;

        return type;
    }

    private UnitType CreateToucan() {
        UnitType type = new UnitType();

        type.ID = "toucan1";
        type.Name = "Toucan";
        type.cost = 100f;
        type.baseMovementSpeed = 25;
        type.isFlier = true;

        return type;
    }

    private UnitType CreateMerman() {
        UnitType type = new UnitType();

        type.ID = "mermam1";
        type.Name = "Merman";
        type.cost = 100f;
        type.baseMovementSpeed = 10;
        type.isAquatic = true;

        return type;
    }

    private UnitType CreateTruck() {
        UnitType type = new UnitType();

        type.ID = "truck1";
        type.Name = "Truck";
        type.cost = 100f;
        type.storageStackCount = 1;
        type.storageStackVolume = 0.5f;
        type.cargoPartId = "trailer";
        type.baseMovementSpeed = 15;
        type.isHillWalker = true;

        return type;
    }
}