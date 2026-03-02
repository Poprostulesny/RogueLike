namespace OODProject;
public class Broomstick : IInventoryItem
{
    public  Broomstick() : base("Broomstick", "Rumours say that the best witches can use it to fly", 'E'){}
    
    public override int item_size { get => 1; }
    public override bool isTwoHanded { get => false; }
    public override int Damage { get=> 0;  }
    
    
    public override void ApplyEffect(Hero Player)
    {
        
    }

    public override void TakeOffEffect(Hero Player)
    {
        
    }
}

public class Teapot : IInventoryItem
{
    public Teapot() : base("Teapot", "The British say that it is the cure for all sicknesses. They are wrong", 'E'){}
    public override int item_size { get => 1; }
    public override bool isTwoHanded { get => false; }
    public override int Damage { get=> 0;  }

    public override void ApplyEffect(Hero Player)
    {
        
    }

    public override void TakeOffEffect(Hero Player)
    {
    }
}

public class BrokenSword : IInventoryItem
{
    public BrokenSword():base("Broken Sword", "Once great, now merely a bunch of rust", 'E'){}
    
    public override int item_size { get => 1; }
    public override bool isTwoHanded { get => false; }
    public override int Damage { get=> 0;  }
    public override void ApplyEffect(Hero Player)
    {}

    public override void TakeOffEffect(Hero Player)
    {
        
    }
}

