namespace OODProject.Core;

public enum GameObjects
{
    Item,
    Enemies,
    Movement,
    All
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
    (IReadOnlyCollection<IInventoryItemBase>, int) GetInventoryItems();
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
    public IInventoryItemBase GetInventoryItem(int cnt);
    public int CntFieldItems();
    public bool IsTwoHandedEquipped();
    
}
public class GameWorld:IGameWorldView , IInputPrimitives
{
    public readonly SortedSet<GameObjects> _worldFeatures;
    
    private Hero _player = new();
    private Field[,] _world = new Field[42, 22];

    public GameWorld()
    {
        var ctx = new DungeonBuildContext(_world);

        //use some strategies or create your own from the building blocks
        
        var strategy = new DungeonGrounds();
        strategy.Build(ctx);
        
        //dont change things below
        _worldFeatures = ctx.Features;
        _worldFeatures.Add(GameObjects.All);
        _world[1, 1].TryAddHero(ref _player, 1, 1);
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


        var oldX =  _player.PosX;
        var oldY =  _player.PosY;
        var newX = oldX + xplus;
        var newY = oldY + yplus;
        if (!(newY < 0 || newY >= _world.GetLength(0) || newX < 0 
            || newX >= _world.GetLength(1)) &&
            _world[newY, newX].TryAddHero(ref _player, newX, newY))
        {
             _world[oldY, oldX].RemoveHero();
             return(newX, newY);
            
        }

        return (-1, -1);
    }

    public (IItem item, bool result) DropItem(int cnt)
    {
        cnt--;
        var which = _player.Inventory.Items[cnt];
        if ( _player.Hands.Left == which)
             _player.Hands.TryRemove(Hand.Left,  _player);
        else if ( _player.Hands.Right == which)
             _player.Hands.TryRemove(Hand.Right,  _player);

        var ret =  _world[ _player.PosY,  _player.PosX].TryAddItem(which);
        if (ret)
        {
            _player.Inventory.Remove(which);
            
            return (which, true);
        }
      
        return (which, false);
        

        
    }

    public int CntInventoryItems()
    {
        return  _player.Inventory.Items.Count;
        
    }

    public bool IsTwoHandedEquipped()
    {
        return  _player.Hands.IsTwoHandedEquipped;
    }
    

    public int CntFieldItems()
    {
        return  _world[ _player.PosY,  _player.PosX].Items.Count;
    }
    public (IItem? item, bool result) PickupItem(int cnt)
    {
        cnt--;
        if (cnt >= _world[PlayerPosY, PlayerPosX].Items.Count)
        {
            return (null, false);
        }
        var item =  _world[ _player.PosY,  _player.PosX].Items[cnt];
        if ( _world[ _player.PosY,  _player.PosX]
                .TryTakeItem( _world[ _player.PosY,  _player.PosX].Items[cnt]) &&
            item.OnPickup( _player))
        {
           
            return (item, true);
        }
        
        _world[ _player.PosY,  _player.PosX]
                .TryAddItem(item);
          
        return (item, false);
           
        
    }

    public IItem? FreeHerosHand(Hand hand)
    {
        
        IInventoryItemBase? item = _player.Hands.TryRemove(hand, _player);

        return item;
    }
    public IItem? EquipItem(int cnt, Hand side)
    {   
        cnt--;
        if (_player.Inventory.Items[cnt].IsTwoHanded)
        {
            side = Hand.Left;
        }
        if ( _player.Hands.TryEquip( _player.Inventory.Items[ cnt], side,  _player))
        {
            return _player.Inventory.Items[cnt];
        }
        
        return null;
    }

    public IInventoryItemBase GetInventoryItem(int cnt)
    {
        cnt--;
        return _player.Inventory.Items[cnt];
    }

    public int Width { get=>_world.GetLength(0); }
    public int Height { get=>_world.GetLength(1); }
    public char GetGlyphAt(int x, int y)
    {
        return _world[y, x].Glyph;
    }

    public int PlayerPosX { get => _player.PosX;  }
    public int PlayerPosY { get => _player.PosY;  }
    public IReadOnlyCollection<IItem> GetItemsAt(int x, int y)
    {
        return  _world[y, x].Items.AsReadOnly();
    }

    public IOccupant? GetOccupantAt(int x, int y)
    {
        return _world[y, x].Occupant;
    }

    public Hands GetHands()
    {
        return new Hands(_player.Hands);
    }

    public (IReadOnlyCollection<IInventoryItemBase>, int) GetInventoryItems()
    {
        return (_player.Inventory.Items.AsReadOnly(), _player.Inventory.Capacity);
    }

    public HeroStats GetHeroStats()
    {
        return _player.Stats;
    }
}
