using System.Text;

namespace OODProject;
//TODO
/*
 * dodac kolejny slownik do mapowania klawiszy
 * ulepszyc separacje miedzy rendererem a inputem,
 * w generatorze swiata, tworzymy obiekt konstruktora, ktorego statyczny konstruktor zwraca filled lub empty dungeon 
 */
public interface IInput
{
    public Dictionary<GameObjects, string> Description { get; }
    public bool TakeInput();
    public void Initialize(IInputPrimitives gameWorld);
}

public class KeyboardInput : IInput
{
    private readonly Dictionary<ConsoleKey, Action> dict = new();
    private IInputPrimitives? _gameWorld;
    private Dictionary<GameObjects, string> description = new();
    public Dictionary<GameObjects, string> Description { get=>description; }
    private bool quitflag = true;
    //dictionary for movement -> 
    //dictionary for primary actions = key -> primary action
    //dictionary for secondary actions -> action type -> bind

    public void Initialize(IInputPrimitives gameWorld)
    {
        _gameWorld = gameWorld;
        dict.Add(ConsoleKey.UpArrow, () => Move(Direction.Up));
        dict.Add(ConsoleKey.DownArrow, () => Move(Direction.Down));
        dict.Add(ConsoleKey.LeftArrow, () => Move(Direction.Left));
        dict.Add(ConsoleKey.RightArrow, () =>  Move(Direction.Right));
        dict.Add(ConsoleKey.W, () => Move(Direction.Up));
        dict.Add(ConsoleKey.S, () => Move(Direction.Down));
        dict.Add(ConsoleKey.A, () => Move(Direction.Left));
        dict.Add(ConsoleKey.D, () => Move(Direction.Right));
        dict.Add(ConsoleKey.E, () => TryPickupItem());
        dict.Add(ConsoleKey.I, () => ChooseItemToEquip());
        dict.Add(ConsoleKey.X, () => ChooseItemToDrop());
        dict.Add(ConsoleKey.H, () => ChooseHandToFree());
        dict.Add(ConsoleKey.B, () => Quit());
        description.Add(GameObjects.Movement, "WASD or arrows - Movement");
        description.Add(GameObjects.Item, "I - choosing item to equip into the hands|H - Freeing hands|E - Picking up item|X - Dropping an item");
        description.Add(GameObjects.Quitting, "B - Quitting");
    }

   

    private void Move(Direction dir)
    {
        var ret =  _gameWorld.MoveHero(dir);
        if (ret !=(-1, -1)) 
            MessageBus.Send($"Hero moved to {ret.x} {ret.y}");
        else
        {
            MessageBus.Send($"Couldn't move the Hero");
        }
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

    private void Quit()
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

        IItem? r;
        if(( r = _gameWorld.FreeHerosHand(hand))==null)
        {
            MessageBus.Send("You don't have anything in this hand");
        }
        else
        {
            MessageBus.Send($"{r.Name} freed from hands");
        }
        
    }

    private void ChooseItemToDrop()
    {
        var cnt = _gameWorld.CntInventoryItems();
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

        var ret = _gameWorld.DropItem(num);
        if (ret.result == false) 
        {
            MessageBus.Send($"You couldn't drop {ret.item.Name}");
        }
        else
        {
            MessageBus.Send($"You have dropped {ret.item.Name}");
        }
       
    }

    private void ChooseItemToEquip()
    {
        MessageBus.Send("Choose item number to equip");
        var cnt = _gameWorld.CntInventoryItems();
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

        Hand hand = Hand.Left;
        if (!_gameWorld.GetInventoryItem(num).isTwoHanded)
        {
            MessageBus.Send("Choose hand to equip by pressing L or R");
            var key = Console.ReadKey(true);

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
        }

        var r = _gameWorld.EquipItem(num, hand);
        if (r == null)
        {
            MessageBus.Send("Empty your hands before equipping");
            return;
        }
        
        MessageBus.Send($"{r.Name} succesfully equipped");
    }

    private void TryPickupItem()
    {   
        var cnt_field = _gameWorld.CntFieldItems();
        if (cnt_field <= 0)
        {
            MessageBus.Send("You don't have anything to pickup");
            return;
        }

        (IItem? item, bool result) ret;
        int cnt;
        if (cnt_field == 1)
        {

            cnt = 1;
        }
        else
        {
            MessageBus.Send("Choose item number to pick up");
             cnt = KeyboardUtils.ReadNumber();
            if (cnt < 1 || cnt > cnt_field)
            {
                MessageBus.Send("Invalid choice");
                return;
            }
        }

       

        ret = _gameWorld.PickupItem(cnt);
        if (ret.item == null)
        {
            MessageBus.Send("Invalid input");
        }
        else if (ret.result)
        {
            MessageBus.Send($"You have picked up {ret.item.Name}");
        }
        else
        {
            MessageBus.Send($"You couldn't pick up {ret.item.Name}");
        }
    }
}



public static class KeyboardUtils
{
    public static int ReadNumber()
    {
        var sb = new StringBuilder();
        int i;
        MessageBus.Send("Input the number, if you need, press backspace to clear the last digit.");
        while (true)
        {
            var k = Console.ReadKey(true);
            if (k.Key == ConsoleKey.Enter)
            {
                if (int.TryParse(sb.ToString(), out i)) break;
                sb.Clear();
               
            }

           
            if (char.IsDigit(k.KeyChar)) sb.Append(k.KeyChar);
            if (k.Key == ConsoleKey.Backspace && sb.Length > 0) sb.Length--;
        }


        return i;
    }
}
