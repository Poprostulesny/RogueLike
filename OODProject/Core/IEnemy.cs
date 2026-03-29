namespace OODProject.Core;

public interface IEnemy : IDescribable
{
    
    public int HealthPoints { get; }
    public int AttackValue { get; }
    public int ArmorPoints { get; }
    public List<IItem> Loot { get; }
    public (bool survived, int damage) TakeDamage(int AttackStrength);
    

}


public class Orc : IEnemy
{
    public char Glyph { get=>'O'; }
    public string Description { get=> "Ug ug og"; }
    public string Name { get=>"Orc"; }
    public int HealthPoints { get=>_hp; }
    public int AttackValue
    {
        get=>Random.Shared.Next(10, 30);
    }

    public int ArmorPoints { get=>armor; }
    public List<IItem> Loot { get=>new List<IItem>( ); }
    private int _hp=200;
    private int armor=15;
    public (bool survived, int damage) TakeDamage(int AttackStrength)
    {
        _hp -= Math.Max(0, AttackStrength-armor);
        return (_hp > 0, Math.Max(0, AttackStrength-armor)) ;


    }
}



