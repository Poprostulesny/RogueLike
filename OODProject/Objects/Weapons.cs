using OODProject.Core;

namespace OODProject.Objects;

public class DragonSlayerSword() : IHeavyWeapon("Dragon Slayer Sword", "Mythical sword used to hunt dragons", 'W')
{
 
    public override int ItemSize => 1; 
    public override bool IsTwoHanded => true;
    public override int Damage => 100;

    
}

public class MagicWand() : IMagicalWeapon("Magic Wand", "Used by the best sorcerers", 'W')
{
    public override int ItemSize => 1;
    public override int Damage { get=>30; }
    
}
public class RustySword() : ILightWeapon("Rusty Sword", "If items could talk, this one could tell you a lot", 'W')
{
 

    public override int ItemSize => 1;
    public override int Damage => 5;


 
}

public class Shield : INormalItem
{
    public Shield() : base("Shield", "Increases your defense", 'W')
    {
    }

    public override int ItemSize => 1;
    public override bool IsTwoHanded => false;
    public override int Damage => 0;


    public override void OnEquip(Hero player)
    {
        
    }

    public override void OnUnequip(Hero player)
    {
      
    }
}