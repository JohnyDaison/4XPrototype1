public abstract class StorageItem
{
    public string Type {get; set;} = null;
    public float StackVolume {get; set;} = 0;

    public abstract StorageItem SplitItem(float maxStackVolume);
    public abstract bool MergeItem(StorageItem item, float maxStackVolume = 0);
}