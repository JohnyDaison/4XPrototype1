public class Warehouse : SurfaceStructure
{
    public Warehouse()
    {
        Name = "Warehouse";
        storageContainer.TotalStackCount = 6;
        storageContainer.MaxStackVolume = 2;
        isGeneralStorage = true;
    }

    public override void DoTurn() {
        
    }
    
    public override string GetNamePlateText() {
        string inStorage = storageContainer.GetOccupiedVolume().ToString("0.00");
        string maxStorage = storageContainer.GetTotalVolume().ToString("0.00");

        return $"{inStorage}/{maxStorage}";
    }
}