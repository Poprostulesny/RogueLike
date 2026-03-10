namespace OODProject;

public class Gold(int amount) : Currency
{
    public int amount { get; set; } = amount;

    public bool OnPickup(Hero Player)
    {
        Player.AddGold(amount);
        MessageBus.Send($"Player picked up {amount} gold");
        return true;
    }

    public char Glyph { get; set; } = 'C';
    public string Description { get; set; } = "Precious metal";
    public string Name { get; set; } = $"Gold ({amount.ToString()})";

    public string Message()
    {
        throw new NotImplementedException();
    }
}

public class Coins(int amount) : Currency
{
    public int amount { get; set; } = amount;
    public char Glyph { get; set; } = 'C';
    public string Description { get; set; } = "Currency of this world";
    public string Name { get; set; } = $"Coins ({amount.ToString()})";

    public bool OnPickup(Hero Player)
    {
        Player.AddCoins(amount);
        MessageBus.Send($"Player picked up {amount} coins");
        return true;
    }

    public string Message()
    {
        throw new NotImplementedException();
    }
}