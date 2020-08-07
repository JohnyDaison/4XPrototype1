using UnityEngine;
public class ResourceType: GeneralType
{
    public ResourceType()
    {
        ID = "EmptyResourceTypeId";
        Name = "EmptyResourceTypeName";
    }

    public float Density {get; set;} = 0;
    public Color Color {get; set;} = Color.white;
}