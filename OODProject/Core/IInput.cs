using System.Text;

namespace OODProject;

public enum InputDevice
{
    Keyboard,
}

public enum HandChoice
{
    Left,
    Right,
}

public enum ConfirmChoice
{
    Confirm,
}

public readonly record struct InputCode(InputDevice Device, int Code)
{
    public static InputCode FromConsoleKey(ConsoleKey key) => new(InputDevice.Keyboard, (int)key);
}

public class BiMap<TLeft, TRight> where TLeft : notnull where TRight : notnull
{
    private readonly Dictionary<TLeft, TRight> _forward = new();
    private readonly Dictionary<TRight, List<TLeft>> _reverse = new();

    public void Add(TLeft left, TRight right)
    {
        _forward[left] = right;
        if (!_reverse.TryGetValue(right, out var list))
        {
            list = [];
            _reverse[right] = list;
        }
        list.Add(left);
    }

    public void Remove(TLeft left)
    {
        if (_forward.Remove(left, out var right))
            _reverse[right].Remove(left);
    }

    public bool TryGetByLeft(TLeft key, out TRight value) => _forward.TryGetValue(key, out value);

    public bool TryGetByRight(TRight key, out IReadOnlyList<TLeft> values)
    {
        if (_reverse.TryGetValue(key, out var list) && list.Count > 0)
        {
            values = list;
            return true;
        }

        values = Array.Empty<TLeft>();
        return false;
    }
}

public abstract class IInput
{
    internal IInputPrimitives? _gameWorld;
    internal Dictionary<GameObjects, string> description = new();
    public Dictionary<GameObjects, string> Description => description;

    public BiMap<InputCode, HandChoice> Hands = new();
    public BiMap<InputCode, ConfirmChoice> ConfirmChoices = new();
    public BiMap<InputCode, int> Numbers = new();

    public record struct BoundAction(InputObject Handler, object? Arg);
    internal Dictionary<InputCode, BoundAction> PrimaryActionDictionary = new();

    public abstract void Initialize(IInputPrimitives gameWorld);
    public abstract bool TakeInput();

    public abstract class InputObject
    {
        protected readonly InputContext ctx;

        protected InputObject(InputContext ctx)
        {
            this.ctx = ctx;
        }

        public abstract void Action(object? arg);
        public abstract string GuideDescription { get; }
        public abstract List<GameObjects> AssociatedGameObjects { get; }
    }

    public class InputContext
    {
        public readonly IInputPrimitives Game;
        public readonly BiMap<InputCode, HandChoice> Hands;
        public readonly BiMap<InputCode, ConfirmChoice> ConfirmChoices;
        public readonly BiMap<InputCode, int> Numbers;

        public InputContext(IInputPrimitives gameWorld, BiMap<InputCode, HandChoice> hands,
            BiMap<InputCode, ConfirmChoice> confirmChoices, BiMap<InputCode, int> numbers)
        {
            Game = gameWorld;
            Hands = hands;
            ConfirmChoices = confirmChoices;
            Numbers = numbers;
        }
    }
}

public class KeyboardInput : IInput
{
    bool quitflag=true;
    private InputContext? ctx;
    private readonly BoolRef quitRef = new();

    private class BoolRef
    {
        public bool Value=true;
    }

    private void InsertDefaults()
    {
        Hands = new();
        ConfirmChoices = new();
        Numbers = new();

        Hands.Add(InputCode.FromConsoleKey(ConsoleKey.L), HandChoice.Left);
        Hands.Add(InputCode.FromConsoleKey(ConsoleKey.R), HandChoice.Right);

        ConfirmChoices.Add(InputCode.FromConsoleKey(ConsoleKey.B), ConfirmChoice.Confirm);
        ConfirmChoices.Add(InputCode.FromConsoleKey(ConsoleKey.Enter), ConfirmChoice.Confirm);

        Numbers.Add(InputCode.FromConsoleKey(ConsoleKey.D0), 0);
        Numbers.Add(InputCode.FromConsoleKey(ConsoleKey.D1), 1);
        Numbers.Add(InputCode.FromConsoleKey(ConsoleKey.D2), 2);
        Numbers.Add(InputCode.FromConsoleKey(ConsoleKey.D3), 3);
        Numbers.Add(InputCode.FromConsoleKey(ConsoleKey.D4), 4);
        Numbers.Add(InputCode.FromConsoleKey(ConsoleKey.D5), 5);
        Numbers.Add(InputCode.FromConsoleKey(ConsoleKey.D6), 6);
        Numbers.Add(InputCode.FromConsoleKey(ConsoleKey.D7), 7);
        Numbers.Add(InputCode.FromConsoleKey(ConsoleKey.D8), 8);
        Numbers.Add(InputCode.FromConsoleKey(ConsoleKey.D9), 9);
    }

    public override void Initialize(IInputPrimitives gameWorld)
    
    {
        _gameWorld = gameWorld;

        InsertDefaults();
        PrimaryActionDictionary.Clear();

        ctx = new InputContext(gameWorld, Hands, ConfirmChoices, Numbers);

        var move = new MoveAction(ctx);
        var quit = new QuitAction(ctx);
        var freeHand = new FreeHandAction(ctx);
        var drop = new ChooseInventoryItemAction(ctx);
        var equip = new ChooseItemToEquipAction(ctx);
        var pickup = new TryPickupItemAction(ctx);

        PrimaryActionDictionary.Add(InputCode.FromConsoleKey(ConsoleKey.UpArrow), new BoundAction(move, Direction.Up));
        PrimaryActionDictionary.Add(InputCode.FromConsoleKey(ConsoleKey.DownArrow), new BoundAction(move, Direction.Down));
        PrimaryActionDictionary.Add(InputCode.FromConsoleKey(ConsoleKey.LeftArrow), new BoundAction(move, Direction.Left));
        PrimaryActionDictionary.Add(InputCode.FromConsoleKey(ConsoleKey.RightArrow), new BoundAction(move, Direction.Right));

        PrimaryActionDictionary.Add(InputCode.FromConsoleKey(ConsoleKey.W), new BoundAction(move, Direction.Up));
        PrimaryActionDictionary.Add(InputCode.FromConsoleKey(ConsoleKey.S), new BoundAction(move, Direction.Down));
        PrimaryActionDictionary.Add(InputCode.FromConsoleKey(ConsoleKey.A), new BoundAction(move, Direction.Left));
        PrimaryActionDictionary.Add(InputCode.FromConsoleKey(ConsoleKey.D), new BoundAction(move, Direction.Right));

        PrimaryActionDictionary.Add(InputCode.FromConsoleKey(ConsoleKey.E), new BoundAction(pickup, null));
        PrimaryActionDictionary.Add(InputCode.FromConsoleKey(ConsoleKey.I), new BoundAction(equip, null));
        PrimaryActionDictionary.Add(InputCode.FromConsoleKey(ConsoleKey.X), new BoundAction(drop, null));
        PrimaryActionDictionary.Add(InputCode.FromConsoleKey(ConsoleKey.H), new BoundAction(freeHand, null));
        PrimaryActionDictionary.Add(InputCode.FromConsoleKey(ConsoleKey.B), new BoundAction(quit, quitRef));
        PrimaryActionDictionary.Add(InputCode.FromConsoleKey(ConsoleKey.Enter), new BoundAction(quit, quitRef));

        description.Clear();
       
        description.Add(GameObjects.Movement, "WASD or arrows - Movement");
        description.Add(GameObjects.Item,
            "I - choosing item to equip into the hands|H - Freeing hands|E - Picking up item|X - Dropping an item");
        description.Add(GameObjects.Quitting, "B/Enter - Quitting");
    }

    public override bool TakeInput()
    {
        var key = Console.ReadKey(true);
        var code = InputCode.FromConsoleKey(key.Key);

        if (PrimaryActionDictionary.TryGetValue(code, out var bound))
        {
            

            bound.Handler.Action(bound.Arg);

            quitflag = quitRef.Value;
        }
        else
        {
            MessageBus.Send("Invalid input");
        }

        return quitflag;
    }

    public class MoveAction(InputContext ctx) : InputObject(ctx)
    {
        public override void Action(object? arg)
        {
            if (arg ==null)
            {
                MessageBus.Send("Invalid argument");
                return;
            }
            var dir = (Direction)arg;
            var ret = ctx.Game.MoveHero(dir);
            if (ret != (-1, -1))
                MessageBus.Send($"Hero moved to {ret.x} {ret.y}");
            else
                MessageBus.Send("Couldn't move the Hero");
        }

        public override string GuideDescription => "Move Hero Around";
        private readonly List<GameObjects> ObjectsList = new List<GameObjects>([GameObjects.Movement]);
        
        public override List<GameObjects> AssociatedGameObjects { get=>ObjectsList; }
    }

    public class QuitAction(InputContext ctx) : InputObject(ctx)
    {
        public override void Action(object? arg)
        {   if (arg ==null)
            {
                MessageBus.Send("Invalid argument");
                return;
            }
            if (!ctx.ConfirmChoices.TryGetByRight(ConfirmChoice.Confirm, out var confirmCodes) || confirmCodes.Count == 0)
            {
                ((BoolRef)arg).Value = false;
                return;
            }

            var confirmCode = confirmCodes[^1];
            MessageBus.Send($"If you are sure you want to quit the game press {((ConsoleKey)confirmCode.Code)}");
            var inp = Console.ReadKey(true);

            if (ctx.ConfirmChoices.TryGetByLeft(InputCode.FromConsoleKey(inp.Key), out var choice) &&
                choice == ConfirmChoice.Confirm)
            {
                 ((BoolRef)arg).Value = false;
            }
            else
            {
                MessageBus.Send("Quit canceled");
            }
        }
        private readonly List<GameObjects> ObjectsList = new List<GameObjects>([GameObjects.Quitting]);
        
        public override List<GameObjects> AssociatedGameObjects { get=>ObjectsList; }
        public override string GuideDescription => "Quit the game";
    }

    public class FreeHandAction(InputContext ctx) : InputObject(ctx)
    {
        public override void Action(object? arg)
        {
            if (!ctx.Hands.TryGetByRight(HandChoice.Left, out var l) ||
                !ctx.Hands.TryGetByRight(HandChoice.Right, out var r))
            {
                MessageBus.Send("Hand bindings are not configured");
                return;
            }

            MessageBus.Send(
                $"Choose hand to drop by pressing {((ConsoleKey)l[^1].Code)}[left] or {((ConsoleKey)r[^1].Code)}[right]");
            var key = Console.ReadKey(true);

            Hand? hand = null;
            if (ctx.Hands.TryGetByLeft(InputCode.FromConsoleKey(key.Key), out var choice) &&
                choice == HandChoice.Right)
            {
                hand = Hand.Right;
            }
            else if (choice == HandChoice.Left)
            {
                hand = Hand.Left;
            }

            if (hand == null)
            {
                MessageBus.Send("Invalid choice");
                return;
            }

            var item = ctx.Game.FreeHerosHand(hand.Value);
            if (item == null)
                MessageBus.Send("You don't have anything in this hand");
            else
                MessageBus.Send($"{item.Name} freed from hands");
        }
        private readonly List<GameObjects> ObjectsList = new List<GameObjects>([GameObjects.Item]);
        
        public override List<GameObjects> AssociatedGameObjects { get=>ObjectsList; }
        public override string GuideDescription => "Free Hero's hands";
    }

    public class ChooseInventoryItemAction(InputContext ctx) : InputObject(ctx)
    {
        public override void Action(object? arg)
        {
            var cnt = ctx.Game.CntInventoryItems();
            if (cnt == 0)
            {
                MessageBus.Send("You don't have anything to drop");
                return;
            }

            MessageBus.Send("Choose item number to drop");
            var num = KeyboardUtils.ReadNumber(ctx);
            if (num < 1 || num > cnt)
            {
                MessageBus.Send("Invalid choice");
                return;
            }

            var ret = ctx.Game.DropItem(num);
            if (!ret.result)
                MessageBus.Send($"You couldn't drop {ret.item.Name}");
            else
                MessageBus.Send($"You have dropped {ret.item.Name}");
        }
        private readonly List<GameObjects> ObjectsList = new List<GameObjects>([GameObjects.Item]);
        
        public override List<GameObjects> AssociatedGameObjects { get=>ObjectsList; }
        public override string GuideDescription => "Drop from inventory";
    }

    public class ChooseItemToEquipAction(InputContext ctx) : InputObject(ctx)
    {
        public override void Action(object? arg)
        {
            var cnt = ctx.Game.CntInventoryItems();
            if (cnt == 0)
            {
                MessageBus.Send("You don't have anything to equip");
                return;
            }

            MessageBus.Send("Choose item number to equip");
            var num = KeyboardUtils.ReadNumber(ctx);
            if (num < 1 || num > cnt)
            {
                MessageBus.Send("Invalid choice");
                return;
            }

            Hand hand = Hand.Left;
            if (!ctx.Game.GetInventoryItem(num).isTwoHanded)
            {
                if (!ctx.Hands.TryGetByRight(HandChoice.Left, out var l) ||
                    !ctx.Hands.TryGetByRight(HandChoice.Right, out var r))
                {
                    MessageBus.Send("Hand bindings are not configured");
                    return;
                }

                MessageBus.Send(
                    $"Choose hand to equip by pressing {((ConsoleKey)l[^1].Code)}[left] or {((ConsoleKey)r[^1].Code)}[right]");

                var key = Console.ReadKey(true);
                if (ctx.Hands.TryGetByLeft(InputCode.FromConsoleKey(key.Key), out var choice) &&
                    choice == HandChoice.Right)
                {
                    hand = Hand.Right;
                }
                else if (choice == HandChoice.Left)
                {
                    hand = Hand.Left;
                }
                else
                {
                    MessageBus.Send("Invalid choice");
                    return;
                }
            }

            var it = ctx.Game.EquipItem(num, hand);
            if (it == null)
            {
                MessageBus.Send("Empty your hands before equipping");
                return;
            }

            MessageBus.Send($"{it.Name} successfully equipped");
        }
        private readonly List<GameObjects> ObjectsList = new List<GameObjects>([GameObjects.Item]);
        
        public override List<GameObjects> AssociatedGameObjects { get=>ObjectsList; }
        public override string GuideDescription => "Equip an item from inventory";
    }

    public class TryPickupItemAction(InputContext ctx) : InputObject(ctx)
    {
        public override void Action(object? arg)
        {
            var cntField = ctx.Game.CntFieldItems();
            if (cntField <= 0)
            {
                MessageBus.Send("You don't have anything to pickup");
                return;
            }

            int cnt;
            if (cntField == 1)
            {
                cnt = 1;
            }
            else
            {
                MessageBus.Send("Choose item number to pick up");
                cnt = KeyboardUtils.ReadNumber(ctx);
                if (cnt < 1 || cnt > cntField)
                {
                    MessageBus.Send("Invalid choice");
                    return;
                }
            }

            var ret = ctx.Game.PickupItem(cnt);
            if (ret.item == null)
                MessageBus.Send("Invalid input");
            else if (ret.result)
                MessageBus.Send($"You have picked up {ret.item.Name}");
            else
                MessageBus.Send($"You couldn't pick up {ret.item.Name}");
        }
        private readonly List<GameObjects> ObjectsList = new List<GameObjects>([GameObjects.Item]);
        
        public override List<GameObjects> AssociatedGameObjects { get=>ObjectsList; }
        public override string GuideDescription => "Pickup an item from the ground";
    }
}

public static class KeyboardUtils
{
    public static int ReadNumber(IInput.InputContext ctx)
    {
        var sb = new StringBuilder();
        int i;
        MessageBus.Send("Input the number, if you need, press backspace to clear the last digit.");
        while (true)
        {
            var k = Console.ReadKey(false);
            if (k.Key == ConsoleKey.Enter)
            {
                if (int.TryParse(sb.ToString(), out i)) break;
                sb.Clear();
            }

            if (ctx.Numbers.TryGetByLeft(InputCode.FromConsoleKey(k.Key), out var t)) sb.Append(t.ToString());
            if (k.Key == ConsoleKey.Backspace && sb.Length > 0) sb.Length--;
        }

        return i;
    }
}
