using System.Collections.Generic;

public class StorageContainer
{
    public StorageContainer() {
    }

    public int TotalStackCount {get; set;} = 0;
    public float MaxStackVolume {get; set;} = 0;

    private Dictionary<int,StorageItem> stacks = new Dictionary<int, StorageItem>();

    
    public float GetOccupiedVolume() {
        float total = 0;
        
        foreach (KeyValuePair<int,StorageItem> entry in stacks)
        {
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
        bool result = CanFullItemBeAdded(newItem);

        if(result) {
            foreach (KeyValuePair<int,StorageItem> entry in stacks)
            {
                StorageItem stackItem = entry.Value;
                stackItem.MergeItem(newItem, MaxStackVolume);
            }

            int stackIndex = 0;
            while(newItem.StackVolume > 0) {
                if(!stacks.ContainsKey(stackIndex)) {
                    stacks.Add(stackIndex, newItem.SplitItem(MaxStackVolume));
                }

                stackIndex++;
            }
        }

        return result;
    }

    public bool RemoveItem(StorageItem item) {
        bool isMyItem = stacks.ContainsValue(item);

        if(!isMyItem) {
            return false;
        }

        int stackIndex = -1;
        foreach (KeyValuePair<int,StorageItem> entry in stacks)
        {
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

    public void DoStackCleanup() {
        foreach (KeyValuePair<int,StorageItem> entry in stacks)
        {
            StorageItem stackItem = entry.Value;
            if(stackItem.StackVolume <= 0) {
                stacks.Remove(entry.Key);
            }
        }
    }
}