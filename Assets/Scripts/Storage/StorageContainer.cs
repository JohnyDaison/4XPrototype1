using System.Collections.Generic;
using UnityEngine;

public class StorageContainer
{
    public StorageContainer() {
    }

    public int TotalStackCount {get; set;} = 0;
    public float MaxStackVolume {get; set;} = 0;

    private Dictionary<int,StorageItem> stacks = new Dictionary<int, StorageItem>();

    
    public float GetOccupiedVolume() {
        float total = 0;
        
        foreach (KeyValuePair<int,StorageItem> entry in stacks) {
            StorageItem item = entry.Value;
            total += item.StackVolume;
        }

        return total;
    }

    public float GetTotalVolume() {
        return TotalStackCount * MaxStackVolume;
    }
    public bool CanFullItemBeAdded(StorageItem item) {
        float remainingVolume = GetTotalVolume() - GetOccupiedVolume();
        return item.StackVolume <= remainingVolume;
    }

    public bool AddItem(StorageItem newItem) {
        float amount = newItem.StackVolume;

        foreach (KeyValuePair<int,StorageItem> entry in stacks) {
            StorageItem stackItem = entry.Value;
            stackItem.MergeItem(newItem, MaxStackVolume);
        }

        for (int stackIndex = 0; stackIndex < TotalStackCount && newItem.StackVolume > 0; stackIndex++) {
            if(!stacks.ContainsKey(stackIndex)) {
                stacks.Add(stackIndex, newItem.SplitItem(MaxStackVolume));
            }
        }

        amount -= newItem.StackVolume;
        bool result = amount > 0;

        Debug.Log($"AddItem moved {amount}");

        return result;
    }

    public bool RemoveItem(StorageItem item) {
        bool isMyItem = stacks.ContainsValue(item);

        if(!isMyItem) {
            return false;
        }

        int stackIndex = -1;
        foreach (KeyValuePair<int,StorageItem> entry in stacks) {
            if(entry.Value == item) {
                stackIndex = entry.Key;
            }
        }

        if(stackIndex != -1) {
            stacks.Remove(stackIndex);
            return true;
        } else {
            return false;
        }
    }

    public bool TransferItem(StorageItem item, StorageContainer targetContainer) {
        bool isMyItem = stacks.ContainsValue(item);

        if(!isMyItem) {
            return false;
        }

        bool result = targetContainer.AddItem(item);

        DoStackCleanup();

        return result;
    }

    /// <summary>
    /// Transfer as much as you can to <paramref name="targetContainer"/>
    /// </summary>
    /// <param name="targetContainer">StorageContainer</param>
    /// <returns>True if anything was transferred, false otherwise</returns>
    public bool TransferAll(StorageContainer targetContainer) {
        bool result = false;
        float amount = GetOccupiedVolume();

        foreach (KeyValuePair<int,StorageItem> entry in stacks) {
            StorageItem stackItem = entry.Value;

            if(targetContainer.AddItem(stackItem)) {
                result = true;
            }
        }

        amount -= GetOccupiedVolume();

        Debug.Log($"TransferAll - moved {amount}");

        DoStackCleanup();

        return result;
    }

    /// <summary>
    /// Try to transfer all of specified <paramref name="resource"/> to <paramref name="targetContainer"/>
    /// </summary>
    /// <param name="resource">ResourceType</param>
    /// <param name="targetContainer">StorageContainer</param>
    /// <returns>True if at least some resource was transferred, false otherwise</returns>
    public bool TransferResource(ResourceType resource, StorageContainer targetContainer) {
        bool result = false;
        float amount = GetOccupiedVolume();

        foreach (KeyValuePair<int,StorageItem> entry in stacks) {
            StorageItem stackItem = entry.Value;
            if (stackItem is ResourceStorageItem) {
                ResourceStorageItem resourceItem = (ResourceStorageItem)stackItem;

                if (resourceItem.ResourceType == resource) {
                    if (targetContainer.AddItem(resourceItem)) {
                        result = true;
                    }
                }
            }
        }

        amount -= GetOccupiedVolume();

        Debug.Log($"TransferResource {resource.Name} - moved {amount}");

        DoStackCleanup();

        return result;

    }

    public void DoStackCleanup() {
        for (int stackIndex = TotalStackCount-1; stackIndex >= 0; stackIndex--) {
            if (!stacks.ContainsKey(stackIndex)) {
                continue;
            }

            StorageItem stackItem = stacks[stackIndex];

            if (stackItem.StackVolume <= 0) {
                stacks.Remove(stackIndex);
            }
        }
    }
}