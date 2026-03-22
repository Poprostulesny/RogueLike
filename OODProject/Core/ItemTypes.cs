namespace OODProject.Core;

public interface IItem : IDescribable
{
    public bool OnPickup(Hero player);
}

public abstract class IInventoryItem(string name, string description, char glyph = 'E') : IItem
{
    public abstract int ItemSize { get; }
    public abstract bool IsTwoHanded { get; }
    public abstract int Damage { get; }
    public char Glyph { get; } = glyph;

    public string Description { get; } = description;

    public string Name { get; } = name;

    public bool OnPickup(Hero player)
    {
        return player.TryTakeItem(this);
    }


    public bool TryEquip(Hero player)
    {
        return player.TryTakeItem(this);
    }

    public abstract void ApplyEffect(Hero player);
    public abstract void TakeOffEffect(Hero player);
}

public interface Currency : IItem
{
    public int Amount { get; set; }
}