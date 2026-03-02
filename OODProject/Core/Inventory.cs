namespace OODProject;


public class Hands
{
    public IInventoryItem? Left;
    public IInventoryItem? Right;
    public bool isTwoHandedEquipped  = false;

    public bool CanEquip(IInventoryItem item, Hand dir)
    {
        if (item == Left || item == Right)
        {
            MessageBus.Send($"{item.Name} already equipped");
            return false;
        }
        if (item.isTwoHanded && Right==null && Left==null)
        {
            return true;
        }

        if (item.isTwoHanded && (Left != null || Right != null))
        {
            return false;
        }

        if (dir == Hand.Left && Left == null|| dir == Hand.Right && Right == null)
        {
            return true;
        }

        return false;
    }

    public bool TryEquip(IInventoryItem item, Hand where, Hero Player)
    {
        if (CanEquip(item, where))
        {
            if (item.isTwoHanded)
            {
                Left = item;
                item.ApplyEffect(Player);
                isTwoHandedEquipped = true;
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
            item.ApplyEffect(Player);
            return true;
        }

        return false;

    }

    public IInventoryItem? TryRemove(Hand where, Hero Player)
    {   
        IInventoryItem? item = null;
        if (isTwoHandedEquipped)
        {
            item = Left;
            isTwoHandedEquipped = false;
            Right = null;
            Left = null;
            if (item != null)
            {
                item.TakeOffEffect(Player);
            }
            return item;
        }
        switch(where)
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
        if (item != null)
        {
            item.TakeOffEffect(Player);
        }
        return item;
    }

    public string DisplayHands()
    {
        if (isTwoHandedEquipped)
        {
            return $"Both Hands: {Left.Name}";
        }

        if (Left == null && Right == null)
        {
            return "Left: None | Right: None";
        }

        if (Left == null)
        {
            return $"Left: None | Right: {Right.Name}";
        }

        if (Right == null)
        {
            return $"Left: {Left.Name} | Right: None";
        }
        return $"Left: {Left.Name} | Right: {Right.Name}";
    }
    
}

public class Inventory(int _capacity)
{
    public int Capacity = _capacity;
    public int UsedUpCapacity=0;
    private List<IInventoryItem> _items = new List<IInventoryItem>();
    public List<IInventoryItem> Items 
    {
        get => _items;
    }
    
    public bool TryAdd(IInventoryItem item)
    {
        if(UsedUpCapacity + item.item_size  <= Capacity)
        {
            _items.Add(item);
            UsedUpCapacity += item.item_size;
            MessageBus.Send($"{item.Name} has been added to Inventory");
            return true;
        }
        MessageBus.Send($"{item.Name} has not been added to Inventory");
        return false;
    }

    public bool Remove(IInventoryItem item)
    {
      var res = _items.Remove(item);
      if (res)
      {
          UsedUpCapacity -= item.item_size;
      }

      return res;
    }

    public string[] DisplayInventory()
    {
        string[] display = new string[Capacity+1];
        display[0] = "Inventory:";
        int cnt = 1;
        foreach(var item in _items)
        {
            display[cnt] = new string($"{cnt.ToString()}. {item.Name}");
            cnt++;
        }

        while (cnt < Capacity + 1)
        {
            display[cnt] = "";
            cnt++;
        }

        return display;
    }
    

}