namespace OODProject.Core;

public class Hands
{
    public bool IsTwoHandedEquipped;
    public IInventoryItemBase? Left;
    public IInventoryItemBase? Right;
    public Hands(Hands other)
    {
        Left = other.Left;
        Right = other.Right;
        IsTwoHandedEquipped= other.IsTwoHandedEquipped;
        
    }

    public Hands()
    {
        IsTwoHandedEquipped = false;
        Left = null;
        Right = null;
    }
    private bool CanEquip(IInventoryItemBase item, Hand dir)
    {
        if (item == Left || item == Right)
        {
            MessageBus.Send($"{item.Name} already equipped");
            return false;
        }

        if (IsTwoHandedEquipped)
        {
            MessageBus.Send("Before equipping an item free your hands!");
            return false;
        }

        if (item.IsTwoHanded && Right == null && Left == null) return true;

        if (item.IsTwoHanded && (Left != null || Right != null)) return false;

        if ((dir == Hand.Left && Left == null) || (dir == Hand.Right && Right == null)) return true;

        return false;
    }

    public bool TryEquip(IInventoryItemBase item, Hand where, Hero player)
    {
        if (CanEquip(item, where))
        {
            if (item.IsTwoHanded)
            {
                Left = item;
                item.OnEquip(player);//item,pickupitem
                IsTwoHandedEquipped = true;
                return true;
            }

            switch (where)
            {
                case Hand.Left:
                    Left = item;
                    break;
                case Hand.Right:
                    Right = item;
                    break;
            }

            item.OnEquip(player);
            return true;
        }

        return false;
    }

    public IInventoryItemBase? TryRemove(Hand where, Hero player)
    {
        IInventoryItemBase  ? item = null;
        if (IsTwoHandedEquipped)
        {
            item = Left;
            IsTwoHandedEquipped = false;
            Right = null;
            Left = null;
            if (item != null) item.OnUnequip(player);
            return item;
        }

        switch (where)
        {
            case Hand.Left:
                if (Left != null)
                {
                    item = Left;
                    Left = null;
                }

                break;
            case Hand.Right:
                if (Right != null)
                {
                    item = Right;
                    Right = null;
                }

                break;
        }

        if (item != null) item.OnUnequip(player);
        return item;
    }
}

public class Inventory(int capacity)
{
    public readonly int Capacity = capacity;
    private int _usedUpCapacity;

    public List<IInventoryItemBase> Items { get; } = new();

    public bool TryAdd(IInventoryItemBase item)
    {
        if (_usedUpCapacity + item.ItemSize <= Capacity)
        {
            Items.Add(item);
            _usedUpCapacity += item.ItemSize;
            MessageBus.Send($"{item.Name} has been added to Inventory");
            return true;
        }

        MessageBus.Send($"{item.Name} couldn't have been added to Inventory");
        return false;
    }

    public bool Remove(IInventoryItemBase item)
    {
        var res = Items.Remove(item);
        if (res) _usedUpCapacity -= item.ItemSize;

        return res;
    }
}