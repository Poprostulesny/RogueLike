namespace OODProject;

public enum Hand
{
    Right,
    Left
}

public class Hero : IDescribable
{
    public Hands hands = new();

    public Inventory inventory = new(5);
    public int PosX;
    public int PosY;
    public HeroStats stats = new();

    public char Glyph => '¶';
    public string Description { get; }

    public string Name => "Hero";

    public string Message()
    {
        throw new NotImplementedException();
    }


    public void TakeDamage(int amount)
    {
        stats.Health -= amount;
    }


    public bool TryTakeItem(IInventoryItem item)
    {
        return inventory.TryAdd(item);
    }

    public void Drop(IInventoryItem item)
    {
        inventory.Remove(item);
    }

    public void AddGold(int amount)
    {
        stats.Gold += amount;
    }

    public void AddCoins(int amount)
    {
        stats.Coins += amount;
    }

    public void Attack(int hand)
    {
        throw new NotImplementedException();
    }

    public void ChangePosition(int x, int y)
    {
        PosX = x;
        PosY = y;
    }
}

public class HeroStats
{
    public int Agility = 20;
    public int Coins;
    public int Defense = 20;
    public int Gold;
    public int Health = 20;
    public int Persuasion = 20;
    public int Strength = 20;
    public int Wisdom = 20;
}