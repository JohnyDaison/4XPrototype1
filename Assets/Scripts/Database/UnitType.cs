public class UnitType
{
    /*  the values in this object should be set only once at the start of the game
        and not changed afterwards
    */
    public UnitType()
    {
    }
    public string id = "EmptyUnitTypeId";
    public string name = "EmptyUnitTypeName";
    public float width = 0; // side to side
    public float length = 0; // front to back
    public float height = 0; // top to bottom
    public float cost = 0;

    public string modelId = null; 
    public string cargoPartId = null;

    public int storageStackCount = 0;
    public float storageStackVolume = 0;

    public int movementPoints = 0;
    public float baseMovementSpeed = 0;
    public bool canBuildCities = false;
    public bool isHillWalker = false;
    public bool isForestWalker = false;
    public bool isAquatic = false;
    public bool isFlier = false;
    
}