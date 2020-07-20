using UnityEngine;
public class ResourceType
{
    public ResourceType()
    {
    }

    public string ID {get; set;} = "EmptyResourceTypeId";
    public string Name {get; set;} = "EmptyResourceTypeName";
    public float Density {get; set;} = 0;
    public Color Color {get; set;} = Color.white;
}