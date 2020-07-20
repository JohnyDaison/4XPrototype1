using System.Collections.Generic;
using UnityEngine;

// should be singleton, probably
public class ResourceTypeDB
{
    public ResourceTypeDB() {
        CreateResourceTypes();
    }

    private Dictionary<string, ResourceType> typeDict = new Dictionary<string, ResourceType>();
    private List<string> typeList = new List<string>();

    // readonly by always returning a copy
    public List<string> GetAllResourceTypes() {
        return new List<string>(typeList);
    }

    public ResourceType GetResourceTypeById(string id) {
        if (!typeList.Contains(id)) {
            return null;
        }
        
        return typeDict[id];
    }

    private bool AddResourceType(ResourceType type) {
        if (type == null) {
            return false;
        }

        if (typeList.Contains(type.ID)) {
            return false;
        }

        typeList.Add(type.ID);
        typeDict.Add(type.ID, type);

        return true;
    }

    private void CreateResourceTypes() {
        AddResourceType(CreateIronOre());
    }

    private ResourceType CreateIronOre() {
        ResourceType type = new ResourceType();

        type.ID = "ironOre1";
        type.Name = "Iron Ore";
        type.Color = Color.red;

        return type;
    }
}