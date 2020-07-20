using UnityEngine;

public class ResourceStorageItem : StorageItem
{
    public ResourceStorageItem() {
        Type = "resource";
    }
    public ResourceType ResourceType {get; set;} = null;

    public override StorageItem SplitItem(float maxStackVolume) {
        float volumeToTransfer = Mathf.Min(maxStackVolume, StackVolume);
        
        ResourceStorageItem result = new ResourceStorageItem();
        result.ResourceType = ResourceType;
        result.StackVolume = volumeToTransfer;
        StackVolume -= volumeToTransfer;

        return result;
    }

    public override bool MergeItem(StorageItem item, float maxStackVolume = 0) {
        if(item is ResourceStorageItem) {
            ResourceStorageItem resourceItem = (ResourceStorageItem)item;

            if(ResourceType == resourceItem.ResourceType) {
                if(maxStackVolume == 0) {
                    StackVolume += resourceItem.StackVolume;
                    resourceItem.StackVolume = 0;
                } else {
                    float maxVolumeToTransfer = Mathf.Max(0, maxStackVolume - StackVolume);
                    float volumeToTransfer = Mathf.Min(resourceItem.StackVolume, maxVolumeToTransfer);
                    StackVolume += volumeToTransfer;
                    resourceItem.StackVolume -= volumeToTransfer;
                }

                return true;
            }
        }

        return false;
    }
}