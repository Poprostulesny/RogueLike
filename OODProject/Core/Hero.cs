using System.Text;

namespace OODProject;

public enum Hand
{
    Right,
    Left
}
public class Hero() : IDescribable
{
   
    public char Glyph { get=>'¶'; }
    public string Description { get =>_description; }
    public string Name { get=>"Hero";  }
    public string Message()
    {
        throw new NotImplementedException();
    }
    
    private string _description;
    public HeroStats stats = new HeroStats();
    public int PosY;
    public int PosX;
    public Hands hands  = new Hands();

    public Inventory inventory = new Inventory(5);
  
    
    
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
        stats.Gold+= amount;
    }

    public void AddCoins(int amount)
    {
        stats.Coins+=amount;
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
    public int Strength=20;
    public int  Agility=20;
    public int Wisdom=20;
    public int Persuasion=20;
    public int Health=20;
    public int Gold=0;
    public int Coins=0;
    public int Defense=20;

    public string[] DisplayStats()
    {
        string[] stats = new string[7];
        stats[0] = "Hero";
        stats[1] = $"Strength: {Strength}";
        stats[2] = $"Agility: {Agility}";
        stats[3] = $"Wisdom: {Wisdom}";
        stats[4] = $"Persuasion: {Persuasion}";
        stats[5] = $"Health: {Health}";
        stats[6] = $"Gold: {Gold} | Coins: {Coins}";
        return stats;
    }

}

