using System.Collections.Generic;

public class GeneralTypeDB<Type> where Type: GeneralType
{
    public GeneralTypeDB() {
        
    }

    private Dictionary<string, Type> typeDict = new Dictionary<string, Type>();
    private List<string> typeList = new List<string>();

    // readonly by always returning a copy
    public List<string> GetAllTypes() {
        return new List<string>(typeList);
    }

    public Type GetTypeById(string id) {
        if (!typeList.Contains(id)) {
            return null;
        }
        
        return typeDict[id];
    }

    protected bool AddType(Type type) {
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
}