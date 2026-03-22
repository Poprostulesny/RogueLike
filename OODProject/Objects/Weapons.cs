using OODProject.Core;

namespace OODProject.Objects;

public class DragonSlayerSword : IInventoryItem
{
    public DragonSlayerSword() : base("Dragon Slayer Sword", "Mythical sword used to hunt dragons", 'W')
    {
    }

    public override int ItemSize => 1;
    public override bool IsTwoHanded => true;
    public override int Damage => 100;


    public override void ApplyEffect(Hero player)
    {
        player.Stats.Strength += 10;
    }

    public override void TakeOffEffect(Hero player)
    {
        player.Stats.Strength -= 10;
    }
}

public class RustySword : IInventoryItem
{
    public RustySword() : base("Rusty Sword", "If items could talk, this one could tell you a lot", 'W')
    {
    }

    public override int ItemSize => 1;
    public override bool IsTwoHanded => false;
    public override int Damage => 5;

    public override void ApplyEffect(Hero player)
    {
    }

    public override void TakeOffEffect(Hero player)
    {
    }
}

public class Shield : IInventoryItem
{
    public Shield() : base("Shield", "Increases your defense", 'W')
    {
    }

    public override int ItemSize => 1;
    public override bool IsTwoHanded => false;
    public override int Damage => 0;

    public override void ApplyEffect(Hero player)
    {
        player.Stats.Agility -= 10;
        player.Stats.Defense += 50;
    }

    public override void TakeOffEffect(Hero player)
    {
        player.Stats.Agility += 10;
        player.Stats.Defense -= 50;
    }
}