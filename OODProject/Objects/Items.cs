namespace OODProject;

public class Broomstick : IInventoryItem
{
    public Broomstick() : base("Broomstick", "Rumours say that the best witches can use it to fly")
    {
    }

    public override int item_size => 1;
    public override bool isTwoHanded => false;
    public override int Damage => 0;


    public override void ApplyEffect(Hero Player)
    {
    }

    public override void TakeOffEffect(Hero Player)
    {
    }
}

public class Teapot : IInventoryItem
{
    public Teapot() : base("Teapot", "The British say that it is the cure for all sicknesses. They are wrong")
    {
    }

    public override int item_size => 1;
    public override bool isTwoHanded => false;
    public override int Damage => 0;

    public override void ApplyEffect(Hero Player)
    {
    }

    public override void TakeOffEffect(Hero Player)
    {
    }
}

public class BrokenSword : IInventoryItem
{
    public BrokenSword() : base("Broken Sword", "Once great, now merely a bunch of rust")
    {
    }

    public override int item_size => 1;
    public override bool isTwoHanded => false;
    public override int Damage => 0;

    public override void ApplyEffect(Hero Player)
    {
    }

    public override void TakeOffEffect(Hero Player)
    {
    }
}