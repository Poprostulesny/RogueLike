using OODProject.Core;

namespace OODProject.Objects;

public class Gold(int amount) : Currency
{
    public int Amount { get; set; } = amount;

    public bool OnPickup(Hero player)
    {
        player.AddGold(Amount);
       // MessageBus.Send($"Player picked up {amount} gold");
        return true;
    }

    public char Glyph { get; set; } = 'C';
    public string Description { get; set; } = "Precious metal";
    public string Name { get; set; } = $"Gold ({amount.ToString()})";
}

public class Coins(int amount) : Currency
{
    public int Amount { get; set; } = amount;
    public char Glyph { get; set; } = 'C';
    public string Description { get; set; } = "Currency of this world";
    public string Name { get; set; } = $"Coins ({amount.ToString()})";

    public bool OnPickup(Hero player)
    {
        player.AddCoins(Amount);
        //MessageBus.Send($"Player picked up {amount} coins");
        return true;
    }
}