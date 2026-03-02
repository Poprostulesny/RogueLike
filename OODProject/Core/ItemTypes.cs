namespace OODProject;

public interface IItem : IDescribable
{
    public void OnPickup(Hero Player);
    
}

public abstract class IInventoryItem(string name, string description, char glyph = 'E') : IItem
{   
    
    public abstract int item_size{get;}
    public abstract bool isTwoHanded{get;}
    public abstract int Damage{get;}
    private char _glyph = glyph;
    private string _name = name;
    private string _description = description;
    public char Glyph { get=>_glyph; }
    public string Description { get=>_description;  }
    public string Name { get=>_name;  }
    public void OnPickup(Hero Player)
    {
        Player.TryTakeItem(this);
    }

    public bool TryEquip(Hero Player)
    {
      return  Player.TryTakeItem(this);
    }
    public abstract void ApplyEffect(Hero Player);
    public abstract void TakeOffEffect(Hero Player);
    public string Message()
    {
        throw new NotImplementedException();
    }
}

public interface Currency : IItem
{
    public int amount{get;set;}
}
