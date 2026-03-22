namespace OODProject.Core;

public enum Hand
{
    Right,
    Left
}

public class Hero : IDescribable
{
    public readonly Hands Hands = new Hands();

    public readonly Inventory Inventory = new(5);
    public int PosX;
    public int PosY;
    public readonly HeroStats Stats = new();

    public char Glyph => '¶';
    public string Description => "Players character";

    public string Name => "Hero";


    public void TakeDamage(int amount)
    {
        Stats.Health -= amount;
    }


    public bool TryTakeItem(IInventoryItem item)
    {
        return Inventory.TryAdd(item);
    }

    public void Drop(IInventoryItem item)
    {
        Inventory.Remove(item);
    }

    public void AddGold(int amount)
    {
        Stats.Gold += amount;
    }

    public void AddCoins(int amount)
    {
        Stats.Coins += amount;
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