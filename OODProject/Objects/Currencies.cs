namespace OODProject;
public class Gold(int amount) : Currency
{
    private int _amount=amount;
    public int amount { get => _amount; set  => _amount = value; }
    public void OnPickup(Hero Player)
    {
        Player.AddGold(_amount);
        MessageBus.Send($"Player picked up {_amount} gold");
    }

    private char _glyph = 'C';
    private string _name = $"Gold ({amount.ToString()})";
    private string _description = "Precious metal";
    public char Glyph { get=>_glyph; set=>_glyph=value; }
    public string Description { get=>_description; set=> _description=value; }
    public string Name { get=>_name; set=>_name=value; }
    public string Message()
    {
        throw new NotImplementedException();
    }
}

public class Coins(int amount) : Currency
{
    private int _amount = amount;
    public int amount { get => _amount; set  => _amount = value; }
    

    private char _glyph = 'C';
    private string _name = $"Coins ({amount.ToString()})";
    private string _description = "Currency of this world";
    public char Glyph { get=>_glyph; set=>_glyph=value; }
    public string Description { get=>_description; set=> _description=value; }
    public string Name { get=>_name; set=>_name=value; }
    public void OnPickup(Hero Player)
    {
        Player.AddCoins(_amount);
        MessageBus.Send($"Player picked up {_amount} coins");
    }
    public string Message()
    {
        throw new NotImplementedException();
    }
}