namespace OODProject.Core;

public interface IItem : IDescribable
{
    public bool OnPickup(Hero player);
}


public abstract class InventoryItemDecorator(IInventoryItemBase _inventoryItem) : IInventoryItemBase
{
    protected IInventoryItemBase inventoryItem = _inventoryItem;
    public virtual char Glyph { get=>inventoryItem.Glyph; }
    public virtual string Description { get=>inventoryItem.Description; }
    public virtual string Name { get=>inventoryItem.Name; }
    public virtual int ItemSize { get=>inventoryItem.ItemSize; }
    public virtual bool IsTwoHanded { get=>inventoryItem.IsTwoHanded; }
    public int DamageNormal { get=>0; }
    public int DamageStealth { get=>0; }
    public int DamageMagical { get=>0; }
    public virtual int Damage { get=>inventoryItem.Damage; }
    
    

    public virtual void OnEquip(Hero player)
    {
        inventoryItem.OnEquip(player);
    }
    public virtual void OnUnequip(Hero player)
    {
        inventoryItem.OnUnequip(player);
    }

    public virtual bool OnPickup(Hero player)
    {
        return inventoryItem.OnPickup(player);
    }
}

public sealed class StrongDecorator(IInventoryItemBase _inventoryItem, int amount = 5)
    : InventoryItemDecorator(_inventoryItem)
{
    public override string Name { get=>_inventoryItem.Name+"(Strong)"; }
    public override int Damage { get=>_inventoryItem.Damage+amount; }
    
}

public sealed class LuckyDecorator(IInventoryItemBase _inventoryItem, int amount = 5)
:InventoryItemDecorator(_inventoryItem){
    public override string Name { get=>_inventoryItem.Name+"(Lucky)"; }

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
    public int DamageNormal { get; }
    public int DamageStealth { get; }
    public int DamageMagical{ get; }
    public int Damage { get; }

    public void OnEquip(Hero player);
    public void OnUnequip(Hero player);
    
}
public abstract class IInventoryItem(string name, string description, char glyph = 'E') : IInventoryItemBase
{
    public abstract int ItemSize { get; }
    public abstract bool IsTwoHanded { get; }
    public virtual int DamageNormal { get=>0; }
    public virtual int DamageStealth { get=>0; }
    public virtual int DamageMagical { get=>0; }
    public virtual int Damage { get=>0; }
    public char Glyph { get; } = glyph;

    public string Description { get; } = description;

    public string Name { get; } = name;

    public bool OnPickup(Hero player)
    {
        return player.TryTakeItem(this);
    }


    public abstract void OnEquip(Hero player);


    public abstract void OnUnequip(Hero player);
}

public abstract class IMagicalWeapon(string name, string description, char glyph = 'W')
    : IInventoryItem(name, description, glyph)
{
    public override int DamageStealth { get=>1; }
    public override int DamageMagical { get=>Damage; }
    public override int DamageNormal { get=>1; }
    
}

public abstract class IHeavyWeapon(string name, string description, char glyph = 'W')
    : IInventoryItem(name, description, glyph)
{   
    
    public override int DamageStealth { get=>Damage/2; }
    public override int DamageMagical { get=>1; }

    public override int DamageNormal
    {
        get => Damage;
    }
    
}

public interface Currency : IItem
{
    public int Amount { get; set; }
}