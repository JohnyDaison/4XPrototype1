using UnityEngine;

public class OreMine : SurfaceStructure
{
    public OreMine()
    {
        Name = "Ore Mine";
        storageContainer.TotalStackCount = 1;
        storageContainer.MaxStackVolume = 1;
        turnController = GameObject.FindObjectOfType<TurnController>();
    }
    
    public float extractionSpeed = 0.1f;

    private TurnController turnController;
    private StorageContainer storageContainer = new StorageContainer();

    public override void DoTurn() {
        DoExtractionTick();
    }
    public void DoExtractionTick() {
        bool hasStorageSpace = storageContainer.GetOccupiedVolume() < storageContainer.GetTotalVolume();

        if(!hasStorageSpace) {
            return;
        }
        
        float extractableOreVolume = Mathf.Min(extractionSpeed, Hex.floatParams[Hex.HEX_FLOAT_PARAMS.IronOre]);

        Hex.floatParams[Hex.HEX_FLOAT_PARAMS.IronOre] -= extractableOreVolume;

        ResourceStorageItem item = new ResourceStorageItem();
        item.ResourceType = GameController.instance.ResourceTypeDB.GetResourceTypeById("ironOre1");
        item.StackVolume = extractableOreVolume;

        storageContainer.AddItem(item);
    }

    public override string GetNamePlateText() {
        float inGround = Hex.floatParams[Hex.HEX_FLOAT_PARAMS.IronOre];
        float inStorage = storageContainer.GetOccupiedVolume();

        return $"{inGround.ToString("0.00")} -> {inStorage.ToString("0.00")}";
    }
}