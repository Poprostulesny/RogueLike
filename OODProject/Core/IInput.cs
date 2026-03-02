using System.Text;

namespace OODProject;

public  interface  IInput
{
   public bool TakeInput();
   public void Initialize(ref GameWorld gameWorld);
}

public class KeyboardInput() : IInput
{
    private GameWorld _gameWorld;
    private WorldUtils utils;

    public void  Initialize(ref GameWorld gameWorld)
    {
    _gameWorld = gameWorld;
     utils = new WorldUtils(ref gameWorld);
    }
    public bool TakeInput()
    {
        
        ConsoleKeyInfo key = Console.ReadKey(true);
        switch (key.Key)
        {
            case ConsoleKey.UpArrow:
                utils.MoveHero(Direction.Up);
                break;
            case ConsoleKey.DownArrow:
                utils.MoveHero(Direction.Down);
                break;
            case ConsoleKey.LeftArrow:
                utils.MoveHero(Direction.Left);
                break;
            case ConsoleKey.RightArrow:
                utils.MoveHero(Direction.Right);
                break;
            case ConsoleKey.W:
                utils.MoveHero(Direction.Up);
                break;
            case ConsoleKey.S:
                utils.MoveHero(Direction.Down);
                break;
            case ConsoleKey.A:
                utils.MoveHero(Direction.Left);
                break;
            case ConsoleKey.D:
                utils.MoveHero(Direction.Right);
                break;
            case ConsoleKey.E:
                TryPickupItem();
                break;
            case ConsoleKey.I:
                ChooseItemToEquip();
                break;
            case ConsoleKey.X:
                ChooseItemToDrop();
                break;
            case ConsoleKey.H:
                ChooseHandToFree();
                break;
            case ConsoleKey.Escape:
                MessageBus.Send("If you are sure you want to quit the game press Escape");
                var inp = Console.ReadKey(true);
                if (inp.Key == ConsoleKey.Escape)
                {
                    return false;
                }
                break;
            
                
        }
        void ChooseHandToFree()
        {
            MessageBus.Send("Choose hand to drop by pressing L or R");
            ConsoleKeyInfo key = Console.ReadKey(true);
            Hand hand; 
            if (key.Key == ConsoleKey.R)
            {
                hand = Hand.Right;
            }
            else if (key.Key == ConsoleKey.L)
            {
                hand = Hand.Left;
            }
            else
            {
                MessageBus.Send("Invalid choice");
                return;
            }
            
            IInventoryItem? item;
            if ((item  = _gameWorld.Player.hands.TryRemove(hand, _gameWorld.Player) )== null)
            {
                MessageBus.Send("You don't have anything in this hand");
            }
            else
            {
                MessageBus.Send($"{item.Name} freed from hands");
            }
        }

        return true;
    }
    void ChooseItemToDrop()
    {
        MessageBus.Send("Choose item number to drop");
        int cnt = _gameWorld.Player.inventory.Items.Count;
        if (cnt == 0)
        {
            MessageBus.Send("You don't have anything to drop");
            return;
        }

        int num = KeyboardUtils.ReadNumber();
        if ( num < 1 || num > cnt)
        {
            MessageBus.Send("Invalid choice");
            return;
        }

        num -= 1;

        utils.DropItem(_gameWorld.Player.inventory.Items[num]);


    }
    void ChooseItemToEquip()
    {
        MessageBus.Send("Choose item number to equip");
        int cnt = _gameWorld.Player.inventory.Items.Count;
        if (cnt == 0)
        {
            MessageBus.Send("You don't have anything to equip");
            return;
        }

        int num = KeyboardUtils.ReadNumber();
       
        if ( num < 1 || num > cnt)
        {  
            MessageBus.Send("Invalid choice");
            return;
        }

        num -= 1;
        

        if (_gameWorld.Player.inventory.Items[num].isTwoHanded)
        {
            if (_gameWorld.Player.hands.TryEquip(_gameWorld.Player.inventory.Items[num], Hand.Left, _gameWorld.Player))
            {
                MessageBus.Send($"{_gameWorld.Player.inventory.Items[num].Name} succesfully equipped");
            }
            else
            {
                MessageBus.Send($"Empty your hands before equipping");
            }
        }
        else
        {
            MessageBus.Send("Choose hand to equip by pressing L or R");
            ConsoleKeyInfo key = Console.ReadKey(true);
            Hand hand; 
            if (key.Key == ConsoleKey.R)
            {
                hand = Hand.Right;
            }
            else if (key.Key == ConsoleKey.L)
            {
                hand = Hand.Left;
            }
            else
            {
                MessageBus.Send("Invalid choice");
                return;
            }

            if (_gameWorld.Player.hands.TryEquip(_gameWorld.Player.inventory.Items[num], hand, _gameWorld.Player))
            {
                MessageBus.Send($"{_gameWorld.Player.inventory.Items[num].Name} equipped");
            }
            else
            {
                MessageBus.Send($"Empty your hands before equipping");
            }
        }
        
    }
    void TryPickupItem()    
    {
        if (_gameWorld.World[_gameWorld.Player.PosY, _gameWorld.Player.PosX].Items.Count <= 0)
        {
            MessageBus.Send("You don't have anything to pickup");
            return;
        }

        if (_gameWorld.World[_gameWorld.Player.PosY, _gameWorld.Player.PosX].Items.Count == 1)
        {   var it = _gameWorld.World[_gameWorld.Player.PosY, _gameWorld.Player.PosX].Items[0];
            if(_gameWorld.World[_gameWorld.Player.PosY,_gameWorld.Player.PosX].TryTakeItem(_gameWorld.World[_gameWorld.Player.PosY,_gameWorld.Player.PosX].Items[0])==true)
            {   it.OnPickup(_gameWorld.Player); 
                MessageBus.Send("Item picked up successfully");
            }
            else
            {
                MessageBus.Send($"You can't pick it up");
            }
            return;
        }
        MessageBus.Send("Choose item number to pick up");
        int cnt = KeyboardUtils.ReadNumber();
        if (cnt < 1 || cnt >= _gameWorld.World[_gameWorld.Player.PosY, _gameWorld.Player.PosX].Items.Count)
        {
            MessageBus.Send("Invalid choice");
        }

        cnt--;

        var item = _gameWorld.World[_gameWorld.Player.PosY, _gameWorld.Player.PosX].Items[cnt];
        if(_gameWorld.World[_gameWorld.Player.PosY,_gameWorld.Player.PosX].TryTakeItem(_gameWorld.World[_gameWorld.Player.PosY,_gameWorld.Player.PosX].Items[cnt])==true)
        {   item.OnPickup(_gameWorld.Player); 
            MessageBus.Send("Item picked up successfully");
        }
        else
        {
            MessageBus.Send($"You can't pick it up");
        }

        return;
    }
}


public class WorldUtils(ref GameWorld gameWorld)
{
    private GameWorld _gameWorld = gameWorld;
    public void MoveHero(Direction dir)
    {
       
        int xplus=0;
        int yplus=0;
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
        
           

        int oldX = _gameWorld.Player.PosX;
        int oldY = _gameWorld.Player.PosY;
        int newX = oldX + xplus;
        int newY = oldY + yplus;

        if (_gameWorld.World[newY, newX].TryAddHero(ref _gameWorld.Player,newX, newY))
        {
            _gameWorld.World[oldY, oldX].RemoveHero(); 
            MessageBus.Send($"Hero moved to {newX} {newY}");
        }
          
        
    }

    public bool DropItem(IInventoryItem which)
    {
        if (_gameWorld.Player.hands.Left == which )
        {
            _gameWorld.Player.hands.TryRemove(Hand.Left, _gameWorld.Player);
            
        }
        else if (_gameWorld.Player.hands.Right == which)
        {
            _gameWorld.Player.hands.TryRemove(Hand.Right, _gameWorld.Player);
        }
        
        var ret  =  _gameWorld.World[_gameWorld.Player.PosX, _gameWorld.Player.PosY].TryAddItem(which);
        if(ret)_gameWorld.Player.inventory.Remove(which);
        return ret;
    }
}

public static class KeyboardUtils
{
    public static int ReadNumber()
    {
        var sb = new StringBuilder();
        int i;
        while (true)
        {
            var k = Console.ReadKey(true);
            if (k.Key == ConsoleKey.Enter)
            {

                if (Int32.TryParse(sb.ToString(), out  i) == true)
                {
                    break;
                }
                sb.Clear();
                
            };
            if (char.IsDigit(k.KeyChar)) sb.Append(k.KeyChar);
            if (k.Key == ConsoleKey.Backspace && sb.Length > 0) sb.Length--;
          
        }

        
        return i;
    }
}


