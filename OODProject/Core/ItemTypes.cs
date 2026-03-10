namespace OODProject;

public interface IItem : IDescribable
{
    public bool OnPickup(Hero Player);
}

public abstract class IInventoryItem(string name, string description, char glyph = 'E') : IItem
{
    public abstract int item_size { get; }
    public abstract bool isTwoHanded { get; }
    public abstract int Damage { get; }
    public char Glyph { get; } = glyph;

    public string Description { get; } = description;

    public string Name { get; } = name;

    public bool OnPickup(Hero Player)
    {
        return Player.TryTakeItem(this);
    }


    public bool TryEquip(Hero Player)
    {
        return Player.TryTakeItem(this);
    }

    public abstract void ApplyEffect(Hero Player);
    public abstract void TakeOffEffect(Hero Player);
}

public interface Currency : IItem
{
    public int amount { get; set; }
}