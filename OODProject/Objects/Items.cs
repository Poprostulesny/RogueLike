using OODProject.Core;

namespace OODProject.Objects;

public class Broomstick : IInventoryItem
{
    public Broomstick() : base("Broomstick", "Rumours say that the best witches can use it to fly")
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

public class Teapot : IInventoryItem
{
    public Teapot() : base("Teapot", "The British say that it is the cure for all sicknesses. They are wrong")
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

public class BrokenSword : IInventoryItem
{
    public BrokenSword() : base("Broken Sword", "Once great, now merely a bunch of rust")
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