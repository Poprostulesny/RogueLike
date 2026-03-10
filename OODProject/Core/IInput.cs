using System.Text;

namespace OODProject;

public interface IInput
{
    public bool TakeInput();
    public void Initialize(ref GameWorld gameWorld);
    public Dictionary<InputTypes,string> InputDict { get; }
}
public enum InputTypes
{
    Movement,
    PickupItem,
    DropItem,
    EquipItem,
    FreeHand,
    Quit
}
public class KeyboardInput : IInput
{
    private GameWorld? _gameWorld;
    private readonly Dictionary<ConsoleKey, Action> dict = new();
    private Dictionary<InputTypes, string> _InputDict = new();
    public Dictionary<InputTypes, string> InputDict { get => _InputDict; }
    private bool quitflag = true;
    private WorldUtils? utils;

    public void Initialize(ref GameWorld gameWorld)
    {
        _gameWorld = gameWorld;
        utils = new WorldUtils(ref gameWorld);

        dict.Add(ConsoleKey.UpArrow, () => utils.MoveHero(Direction.Up));
        dict.Add(ConsoleKey.DownArrow, () => utils.MoveHero(Direction.Down));
        dict.Add(ConsoleKey.LeftArrow, () => utils.MoveHero(Direction.Left));
        dict.Add(ConsoleKey.RightArrow, () => utils.MoveHero(Direction.Right));
        dict.Add(ConsoleKey.W, () => utils.MoveHero(Direction.Up));
        dict.Add(ConsoleKey.S, () => utils.MoveHero(Direction.Down));
        dict.Add(ConsoleKey.A, () => utils.MoveHero(Direction.Left));
        dict.Add(ConsoleKey.D, () => utils.MoveHero(Direction.Right));
        dict.Add(ConsoleKey.E, () => TryPickupItem());
        dict.Add(ConsoleKey.I, () => ChooseItemToEquip());
        dict.Add(ConsoleKey.X, () => ChooseItemToDrop());
        dict.Add(ConsoleKey.H, () => ChooseHandToFree());
        dict.Add(ConsoleKey.B, () => Quit());

        InputDict.Add(InputTypes.Movement, "To move the Hero use WASD or arrows");
        InputDict.Add(InputTypes.FreeHand, "H - Free Hero's hands");
        InputDict.Add(InputTypes.PickupItem, "E - Pick up item from the ground");
        InputDict.Add(InputTypes.EquipItem, "I - Equip item from inventory");
        InputDict.Add(InputTypes.DropItem, "X - Drop item from inventory");
        InputDict.Add(InputTypes.Quit, "B - Quit the game");

    }

    public bool TakeInput()
    {
        var key = Console.ReadKey(true);

        if (dict.ContainsKey(key.Key))
            dict[key.Key]();
        else
            MessageBus.Send("Invalid input");


        return quitflag;
    }

    public void Quit()
    {
        MessageBus.Send("If you are sure you want to quit the game press B");
        var inp = Console.ReadKey(true);
        if (inp.Key == ConsoleKey.B) quitflag = false;
    }

    private void ChooseHandToFree()
    {
        MessageBus.Send("Choose hand to drop by pressing L or R");
        var key = Console.ReadKey(true);
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
        if ((item = _gameWorld.Player.hands.TryRemove(hand, _gameWorld.Player)) == null)
            MessageBus.Send("You don't have anything in this hand");
        else
            MessageBus.Send($"{item.Name} freed from hands");
    }

    private void ChooseItemToDrop()
    {
       
        var cnt = _gameWorld.Player.inventory.Items.Count;
        if (cnt == 0)
        {
            MessageBus.Send("You don't have anything to drop");
            return;
        }
        MessageBus.Send("Choose item number to drop");
        var num = KeyboardUtils.ReadNumber();
        if (num < 1 || num > cnt)
        {
            MessageBus.Send("Invalid choice");
            return;
        }

        num -= 1;
        var msg = _gameWorld.Player.inventory.Items[num].Name;
        utils.DropItem(_gameWorld.Player.inventory.Items[num]);
        MessageBus.Send($"You have dropped {msg}");
    }

    private void ChooseItemToEquip()
    {
        MessageBus.Send("Choose item number to equip");
        var cnt = _gameWorld.Player.inventory.Items.Count;
        if (cnt == 0)
        {
            MessageBus.Send("You don't have anything to equip");
            return;
        }

        var num = KeyboardUtils.ReadNumber();

        if (num < 1 || num > cnt)
        {
            MessageBus.Send("Invalid choice");
            return;
        }

        num -= 1;


        if (_gameWorld.Player.inventory.Items[num].isTwoHanded)
        {
            if (_gameWorld.Player.hands.TryEquip(_gameWorld.Player.inventory.Items[num], Hand.Left, _gameWorld.Player))
                MessageBus.Send($"{_gameWorld.Player.inventory.Items[num].Name} succesfully equipped");
            else
                MessageBus.Send("Empty your hands before equipping");
        }
        else
        {
            MessageBus.Send("Choose hand to equip by pressing L or R");
            var key = Console.ReadKey(true);
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
                MessageBus.Send($"{_gameWorld.Player.inventory.Items[num].Name} equipped");
            else
                MessageBus.Send("Empty your hands before equipping");
        }
    }

    private void TryPickupItem()
    {
        
        if (_gameWorld.World[_gameWorld.Player.PosY, _gameWorld.Player.PosX].Items.Count <= 0)
        {
            MessageBus.Send("You don't have anything to pickup");
            return;
        }

        if (_gameWorld.World[_gameWorld.Player.PosY, _gameWorld.Player.PosX].Items.Count == 1)
        {
            var it = _gameWorld.World[_gameWorld.Player.PosY, _gameWorld.Player.PosX].Items[0];
            if (_gameWorld.World[_gameWorld.Player.PosY, _gameWorld.Player.PosX]
                .TryTakeItem(_gameWorld.World[_gameWorld.Player.PosY, _gameWorld.Player.PosX].Items[0])&& it.OnPickup(_gameWorld.Player))
            {
                
                
            }
            else
            {
                _gameWorld.World[_gameWorld.Player.PosY, _gameWorld.Player.PosX]
            .TryAddItem(it);
                MessageBus.Send("You can't pick it up");
            }

            return;
        }

        MessageBus.Send("Choose item number to pick up");
        var cnt = KeyboardUtils.ReadNumber();
        if (cnt < 1 || cnt >= _gameWorld.World[_gameWorld.Player.PosY, _gameWorld.Player.PosX].Items.Count)
            MessageBus.Send("Invalid choice");

        cnt--;

        var item = _gameWorld.World[_gameWorld.Player.PosY, _gameWorld.Player.PosX].Items[cnt];
        if (_gameWorld.World[_gameWorld.Player.PosY, _gameWorld.Player.PosX]
            .TryTakeItem(_gameWorld.World[_gameWorld.Player.PosY, _gameWorld.Player.PosX].Items[cnt])&& item.OnPickup(_gameWorld.Player))
        {
            
           
        }
        else
        {
            _gameWorld.World[_gameWorld.Player.PosY, _gameWorld.Player.PosX]
            .TryAddItem(item);
            MessageBus.Send("You can't pick it up");
        }
    }
}

public class WorldUtils(ref GameWorld gameWorld)
{
    private readonly GameWorld _gameWorld = gameWorld;

    public void MoveHero(Direction dir)
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


        var oldX = _gameWorld.Player.PosX;
        var oldY = _gameWorld.Player.PosY;
        var newX = oldX + xplus;
        var newY = oldY + yplus;

        if (_gameWorld.World[newY, newX].TryAddHero(ref _gameWorld.Player, newX, newY))
        {
            _gameWorld.World[oldY, oldX].RemoveHero();
            MessageBus.Send($"Hero moved to {newX} {newY}");
        }
    }

    public bool DropItem(IInventoryItem which)
    {
        if (_gameWorld.Player.hands.Left == which)
            _gameWorld.Player.hands.TryRemove(Hand.Left, _gameWorld.Player);
        else if (_gameWorld.Player.hands.Right == which)
            _gameWorld.Player.hands.TryRemove(Hand.Right, _gameWorld.Player);

        var ret = _gameWorld.World[_gameWorld.Player.PosY, _gameWorld.Player.PosX].TryAddItem(which);
        if (ret) _gameWorld.Player.inventory.Remove(which);
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
                if (int.TryParse(sb.ToString(), out i)) break;
                sb.Clear();
            }

            ;
            if (char.IsDigit(k.KeyChar)) sb.Append(k.KeyChar);
            if (k.Key == ConsoleKey.Backspace && sb.Length > 0) sb.Length--;
        }


        return i;
    }
}