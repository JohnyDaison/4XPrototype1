public class Warehouse : SurfaceStructure
{
    public Warehouse()
    {
        Name = "Warehouse";
        storageContainer.TotalStackCount = 6;
        storageContainer.MaxStackVolume = 2;
    }

    private StorageContainer storageContainer = new StorageContainer();

    public override void DoTurn() {
        
    }
    
    public override string GetNamePlateText() {
        return Name;
    }
}