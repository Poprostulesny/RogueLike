namespace OODProject.Core;

public interface AttackStrategy
{ 
    public int AttackDamage(IInventoryItemBase item, HeroStats stats); 
    public int Defense(IInventoryItemBase item, HeroStats stats);
    
}

public class MagicStrategy : AttackStrategy
{
 
    public int AttackDamage(IInventoryItemBase item, HeroStats stats) => item.MagicalAttack(stats);

    public int Defense(IInventoryItemBase item, HeroStats stats) => item.DefenseAgainstMagical(stats);
    
}

public class StealthStrategy : AttackStrategy
{
  
    public int AttackDamage(IInventoryItemBase item, HeroStats stats) => item.StealthAttack(stats);

    public int Defense(IInventoryItemBase item, HeroStats stats) => item.DefenseAgainstStealth(stats);
}
public class NormalStrategy : AttackStrategy
{
   
    public int AttackDamage(IInventoryItemBase item, HeroStats stats) => item.NormalAttack(stats);

    public int Defense(IInventoryItemBase item, HeroStats stats) => item.DefenseAgainstNormal(stats);
}