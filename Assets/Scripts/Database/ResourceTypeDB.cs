using UnityEngine;

// should be singleton, probably
public class ResourceTypeDB: GeneralTypeDB<ResourceType>
{
    public ResourceTypeDB() {
        CreateResourceTypes();
    }

    private void CreateResourceTypes() {
        AddType(CreateIronOre());
    }

    private ResourceType CreateIronOre() {
        ResourceType type = new ResourceType();

        type.ID = "ironOre1";
        type.Name = "Iron Ore";
        type.Color = Color.red;

        return type;
    }
}