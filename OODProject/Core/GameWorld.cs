namespace OODProject;

public enum GameObjects
{
    Item,
    Enemies,
    Movement,
    Quitting
}

public interface IGameWorldView
{
    int Width { get; }
    int Height { get; }
    char GetGlyphAt(int x, int y);
    int PlayerPosX { get; }
    int PlayerPosY { get; }
    IReadOnlyCollection<IItem> GetItemsAt(int x, int y);
    IOccupant? GetOccupantAt(int x, int y);
    Hands GetHands();
    (IReadOnlyCollection<IInventoryItem>, int) GetInventoryItems();
    HeroStats GetHeroStats();
   IReadOnlyCollection<GameObjects> WorldFeatures { get; }
    
}

public interface IInputPrimitives
{
    
    public (int x, int y) MoveHero(Direction dir);
    public (IItem? item, bool result) DropItem(int cnt);
    public int  CntInventoryItems();
    public (IItem? item, bool result) PickupItem(int cnt);
    public IItem? FreeHerosHand(Hand hand);
    public IItem? EquipItem(int cnt,Hand side);
    public IInventoryItem GetInventoryItem(int cnt);
    public int CntFieldItems();
    public bool isTwoHandedEquipped();
    
}
public class GameWorld:IGameWorldView , IInputPrimitives
{
    public readonly SortedSet<GameObjects> _worldFeatures;
    
    private Hero Player = new();
    private Field[,] World = new Field[42, 22];

    public GameWorld()
    {
        var ctx = new DungeonBuildContext(World);

        //use some strategies or create your own from the building blocks
        var strategy = new DungeonGrounds();
        strategy.Build(ctx);
        

        //dont change things below
        _worldFeatures = ctx.Features;
        _worldFeatures.Add(GameObjects.Quitting);
        World[1, 1].TryAddHero(ref Player, 1, 1);
    }

    public IReadOnlyCollection<GameObjects> WorldFeatures { get=>_worldFeatures.ToList().AsReadOnly(); }

    public (int x, int y) MoveHero(Direction dir)
    {   
        var xplus = 0;
        var yplus = 0;
        switch (dir)
        {
            case Direction.Up:
                yplus = -1;
                break;
            case Direction.Down:
                yplus = 1;
                break;
            case Direction.Left:
                xplus = -1;
                break;
            case Direction.Right:
                xplus = 1;
                break;
        }


        var oldX =  Player.PosX;
        var oldY =  Player.PosY;
        var newX = oldX + xplus;
        var newY = oldY + yplus;
        if (!(newY < 0 || newY >= World.GetLength(0) || newX < 0 
            || newX >= World.GetLength(1)) &&
            World[newY, newX].TryAddHero(ref Player, newX, newY))
        {
             World[oldY, oldX].RemoveHero();
             return(newX, newY);
            
        }

        return (-1, -1);
    }

    public (IItem item, bool result) DropItem(int cnt)
    {
        cnt--;
        var which = Player.inventory.Items[cnt];
        if ( Player.hands.Left == which)
             Player.hands.TryRemove(Hand.Left,  Player);
        else if ( Player.hands.Right == which)
             Player.hands.TryRemove(Hand.Right,  Player);

        var ret =  World[ Player.PosY,  Player.PosX].TryAddItem(which);
        if (ret)
        {
            Player.inventory.Remove(which);
            
            return (which, true);
        }
      
        return (which, false);
        

        
    }

    public int CntInventoryItems()
    {
        return  Player.inventory.Items.Count;
        
    }

    public bool isTwoHandedEquipped()
    {
        return  Player.hands.isTwoHandedEquipped;
    }
    

    public int CntFieldItems()
    {
        return  World[ Player.PosY,  Player.PosX].Items.Count;
    }
    public (IItem? item, bool result) PickupItem(int cnt)
    {
        cnt--;
        if (cnt >= World[PlayerPosY, PlayerPosX].Items.Count)
        {
            return (null, false);
        }
        var item =  World[ Player.PosY,  Player.PosX].Items[cnt];
        if ( World[ Player.PosY,  Player.PosX]
                .TryTakeItem( World[ Player.PosY,  Player.PosX].Items[cnt]) &&
            item.OnPickup( Player))
        {
           
            return (item, true);
        }
        
        World[ Player.PosY,  Player.PosX]
                .TryAddItem(item);
          
        return (item, false);
           
        
    }

    public IItem? FreeHerosHand(Hand hand)
    {
        
        IInventoryItem? item = Player.hands.TryRemove(hand, Player);

        return item;
    }
    public IItem? EquipItem(int cnt, Hand side)
    {   
        cnt--;
        if (Player.inventory.Items[cnt].isTwoHanded)
        {
            side = Hand.Left;
        }
        if ( Player.hands.TryEquip( Player.inventory.Items[ cnt], side,  Player))
        {
            return Player.inventory.Items[cnt];
        }
        
        return null;
    }

    public IInventoryItem GetInventoryItem(int cnt)
    {
        cnt--;
        return Player.inventory.Items[cnt];
    }

    public int Width { get=>World.GetLength(0); }
    public int Height { get=>World.GetLength(1); }
    public char GetGlyphAt(int x, int y)
    {
        return World[y, x].Glyph;
    }

    public int PlayerPosX { get => Player.PosX;  }
    public int PlayerPosY { get => Player.PosY;  }
    public IReadOnlyCollection<IItem> GetItemsAt(int x, int y)
    {
        return  World[y, x].Items.AsReadOnly();
    }

    public IOccupant? GetOccupantAt(int x, int y)
    {
        return World[y, x].Occupant;
    }

    public Hands GetHands()
    {
        return new Hands(Player.hands);
    }

    public (IReadOnlyCollection<IInventoryItem>, int) GetInventoryItems()
    {
        return (Player.inventory.Items.AsReadOnly(), Player.inventory.Capacity);
    }

    public HeroStats GetHeroStats()
    {
        return Player.stats;
    }
}
