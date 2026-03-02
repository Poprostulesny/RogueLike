namespace OODProject;

public class DragonSlayerSword : IInventoryItem
{ 
    public  DragonSlayerSword() : base("Dragon Slayer Sword", "Mythical sword used to hunt dragons", 'W'){}
    
    public override int item_size { get => 1; }
    public override bool isTwoHanded { get => true; }
    public override int Damage { get=> 100;  }
    
    
    public override void ApplyEffect(Hero Player)
    {
        Player.stats.Strength += 10;
    }

    public override void TakeOffEffect(Hero Player)
    {
        Player.stats.Strength -= 10;
    }
}

public class RustySword : IInventoryItem
{
    public RustySword() : base("Rusty Sword", "If items could talk, this one could tell you a lot", 'W')
    {
        
    }
    public override int item_size { get => 1; }
    public override bool isTwoHanded { get => false; }
    public override int Damage { get=> 5; }

    public override void ApplyEffect(Hero Player)
    {
        
    }

    public override void TakeOffEffect(Hero Player)
    {
        
    }
}

public class Shield : IInventoryItem
{
    public Shield(): base("Shield", "Increases your defense", 'W'){}
    
    public override int item_size { get => 1; }
    public override bool isTwoHanded { get => false; }
    public override int Damage { get=> 0; }

    public override void ApplyEffect(Hero Player)
    {
        Player.stats.Agility -= 10;
        Player.stats.Defense += 50;
    }

    public override void TakeOffEffect(Hero Player)
    {
        Player.stats.Agility += 10;
        Player.stats.Defense -= 50;
    }
}