namespace OODProject.Core;

public interface IItem : IDescribable
{
    public bool OnPickup(Hero player);
}


public abstract class InventoryItemDecorator(IInventoryItemBase _inner) : IInventoryItemBase
{
    protected IInventoryItemBase inner = _inner;
    public virtual char Glyph { get=>inner.Glyph; }
    public virtual string Description { get=>inner.Description; }
    public virtual string Name { get=>inner.Name; }
    public abstract bool OnPickup(Hero player);

    public virtual int ItemSize { get=>inner.ItemSize; }
    public virtual bool IsTwoHanded { get=>inner.IsTwoHanded; }

    public virtual int Damage { get=>inner.Damage; }
    public virtual int NormalAttack(HeroStats stats) => inner.NormalAttack(stats);

    public virtual int StealthAttack(HeroStats stats) => inner.StealthAttack(stats);

    public virtual int MagicalAttack(HeroStats stats) => inner.MagicalAttack(stats);

    public virtual int DefenseAgainstNormal(HeroStats stats) => inner.DefenseAgainstNormal(stats);

    public virtual int DefenseAgainstStealth(HeroStats stats) => inner.DefenseAgainstStealth(stats);

    public virtual int DefenseAgainstMagical(HeroStats stats) => inner.DefenseAgainstMagical(stats);
   


    public virtual void OnEquip(Hero player)
    {
        inner.OnEquip(player);
    }
    public virtual void OnUnequip(Hero player)
    {
        inner.OnUnequip(player);
    }

   
}

public sealed class StrongDecorator(IInventoryItemBase inner, int amount = 5)
    : InventoryItemDecorator(inner)
{
    public override string Name { get=>inner.Name+"(Strong)"; }
    public override bool OnPickup(Hero player)
    {
       return player.TryTakeItem(this);
    }

    public override int Damage { get=>inner.Damage+amount; }
    
}

public sealed class LuckyDecorator(IInventoryItemBase inner, int amount = 5)
:InventoryItemDecorator(inner){
    public override string Name { get=>inner.Name+"(Lucky)"; }
    public override bool OnPickup(Hero player)
    {
        return player.TryTakeItem(this);
    }

    public override void OnEquip(Hero player)
    {
        player.Stats.Luck += 10;
        base.OnEquip(player);
    }

    public override void OnUnequip(Hero player)
    {
        player.Stats.Luck -= 10;
        base.OnUnequip(player);
    }
}

public interface IInventoryItemBase: IItem
{
    public int ItemSize { get; }
    public bool IsTwoHanded { get; }
    public int Damage { get; }
    public  int NormalAttack(HeroStats stats);

    public int StealthAttack(HeroStats stats);
    public int MagicalAttack(HeroStats stats);
    public int DefenseAgainstNormal(HeroStats stats);
    public int DefenseAgainstStealth(HeroStats stats);
    public int DefenseAgainstMagical(HeroStats stats);
    public void OnEquip(Hero player);
    public void OnUnequip(Hero player);
    
}
public abstract class IInventoryItem(string name, string description, char glyph = 'E') : IInventoryItemBase
{
    public abstract int ItemSize { get; }
    public virtual bool IsTwoHanded { get=>false; }
    public abstract int Damage { get; }

    public abstract int NormalAttack(HeroStats stats);

    public abstract int StealthAttack(HeroStats stats);
    public abstract int MagicalAttack(HeroStats stats);

    public char Glyph { get; } = glyph;

    public string Description { get; } = description;

    public string Name { get; } = name;

    public bool OnPickup(Hero player)
    {
        return player.TryTakeItem(this);
    }

    public abstract int DefenseAgainstNormal(HeroStats stats);

    public abstract int DefenseAgainstStealth(HeroStats stats);

    public abstract int DefenseAgainstMagical(HeroStats stats);
    

    public virtual void OnEquip(Hero player)
    {
        
    }


    public virtual void OnUnequip(Hero player)
    {
        
    }
}

public abstract class INormalItem(string name, string description, char glyph = 'E')
    : IInventoryItem(name, description, glyph)
{
    public override int Damage { get=>0; }
    public override int NormalAttack(HeroStats stats)
    {
        return Damage;
    }
    public override int StealthAttack(HeroStats stats)
    {
        return Damage;
    }

    public override int MagicalAttack(HeroStats stats)
    {
        return Damage;
    }
    public override int DefenseAgainstNormal(HeroStats stats)
    {
        return stats.Dexterity;
    }

    public override int DefenseAgainstStealth(HeroStats stats)
    {
        return 0;
    }

    public override int DefenseAgainstMagical(HeroStats stats)
    {
        return stats.Luck;
    }
}
public abstract class IMagicalWeapon(string name, string description, char glyph = 'W')
    : IInventoryItem(name, description, glyph)
{   
    
    public override int DefenseAgainstNormal(HeroStats stats)
    {
        return Damage*(stats.Wisdom);
    }

    public override int DefenseAgainstStealth(HeroStats stats)
    {
        return 0;
    }
    

    public override int DefenseAgainstMagical(HeroStats stats)
    {
        return stats.Wisdom*2;
    }

    public override int NormalAttack(HeroStats stats)
    {
        return 1;
    }

    public override int MagicalAttack(HeroStats stats)
    {
        return Damage * stats.Wisdom;
    }

    public override int StealthAttack(HeroStats stats)
    {
        return 1;
    }
}

public abstract class IHeavyWeapon(string name, string description, char glyph = 'W')
    : IInventoryItem(name, description, glyph)
{   
  
    public override int NormalAttack(HeroStats stats)
    {
        return Damage * (stats.Aggression+stats.Strength);
    }

    public override int MagicalAttack(HeroStats stats)
    {
        return 1;
    }

    public override int StealthAttack(HeroStats stats)
    {
        return Damage * (stats.Aggression+stats.Strength)/2;
    }
   
    
    public override int DefenseAgainstNormal(HeroStats stats)
    {
        return stats.Strength+stats.Luck;
    }

    public override int DefenseAgainstStealth(HeroStats stats)
    {
        return stats.Strength;
    }

    public override int DefenseAgainstMagical(HeroStats stats)
    {
        return stats.Luck;
    }
    
}

public abstract class ILightWeapon(string name, string description, char glyph = 'W')
    : IInventoryItem(name, description, glyph)
{   
    public override int NormalAttack(HeroStats stats)
    {
        return Damage*(stats.Dexterity+stats.Luck);
    }

    public override int MagicalAttack(HeroStats stats)
    {
        return 1;
    }

    public override int StealthAttack(HeroStats stats)
    {
        return  Damage*(stats.Dexterity+stats.Luck)*2;
    }
    
    public override int DefenseAgainstNormal(HeroStats stats)
    {
        return stats.Dexterity+stats.Luck;
    }

    public override int DefenseAgainstStealth(HeroStats stats)
    {
        return stats.Dexterity;
    }

    public override int DefenseAgainstMagical(HeroStats stats)
    {
        return stats.Luck;
    }
}

public interface Currency : IItem
{
    public int Amount { get; set; }
}