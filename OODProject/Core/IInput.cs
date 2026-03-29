using System.Runtime.CompilerServices;
using System.Text;

namespace OODProject.Core;

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
    Discard
}

public enum InputReturn
{
    Continue,
    Stop,
    Remap
}

public enum AttackTypes
{
    Magical,
    Normal,
    Stealthy
}
public readonly record struct InputCode(InputDevice Device, int Code, string Description)
{
    public static InputCode FromConsoleKey(ConsoleKey key) => new(InputDevice.Keyboard, (int)key, key.ToString());
}

public class BiMap<TLeft, TRight> where TLeft : notnull where TRight : notnull
{
    private readonly Dictionary<TLeft, TRight> _forward = new();
    private readonly Dictionary<TRight, List<TLeft>> _reverse = new();
    public Dictionary<TLeft, TRight> GetForward => _forward;

    public Dictionary<TRight, List<TLeft>> GetReverse => _reverse;

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

    public bool TryGetByLeft(TLeft key, out TRight value) => _forward.TryGetValue(key, out value!);

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
    protected IInputPrimitives? GameWorld;
    protected InputContext Ctx = null!;

    public BiMap<InputCode, HandChoice> Hands = new();
    public BiMap<InputCode, ConfirmChoice> ConfirmChoices = new();
    public BiMap<InputCode, int> Numbers = new();
    public BiMap<InputCode, AttackTypes> AttackStrategies = new ();
    
    internal readonly Dictionary<InputCode, InputObject> PrimaryActionDictionary = new();
    public abstract bool RemapKeys();
    public abstract void Initialize(IInputPrimitives gameWorld);
    public abstract InputReturn TakeInput();

    public abstract class InputObject
    {
        protected readonly InputContext Ctx;

        protected InputObject(InputContext ctx)
        {
            this.Ctx = ctx;

        }



        public abstract void Action();
        public abstract string GuideDescription { get; }
        public abstract List<GameObjects> AssociatedGameObjects { get; }
    }

    public class InputContext
    {
        public readonly IInputPrimitives Game;
        public readonly BiMap<InputCode, HandChoice> Hands;
        public readonly BiMap<InputCode, ConfirmChoice> ConfirmChoices;
        public readonly BiMap<InputCode, int> Numbers;
        public readonly BiMap<InputCode, AttackTypes> AttackStrategies;
        public InputContext(IInputPrimitives gameWorld, BiMap<InputCode, HandChoice> hands,
            BiMap<InputCode, ConfirmChoice> confirmChoices, BiMap<InputCode, int> numbers, BiMap<InputCode, AttackTypes> attackStrategies)
        {
            Game = gameWorld;
            Hands = hands;
            ConfirmChoices = confirmChoices;
            Numbers = numbers;
            AttackStrategies = attackStrategies;
        }
    }
}

public class KeyboardInput : IInput
{
    private readonly StrongBox<InputReturn> _quitRef = new(InputReturn.Continue);
    
    private void InsertDefaultsSecondary()
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
        Numbers.Add(InputCode.FromConsoleKey(ConsoleKey.NumPad0), 0);
        Numbers.Add(InputCode.FromConsoleKey(ConsoleKey.NumPad1), 1);
        Numbers.Add(InputCode.FromConsoleKey(ConsoleKey.NumPad2), 2);
        Numbers.Add(InputCode.FromConsoleKey(ConsoleKey.NumPad3), 3);
        Numbers.Add(InputCode.FromConsoleKey(ConsoleKey.NumPad4), 4);
        Numbers.Add(InputCode.FromConsoleKey(ConsoleKey.NumPad5), 5);
        Numbers.Add(InputCode.FromConsoleKey(ConsoleKey.NumPad6), 6);
        Numbers.Add(InputCode.FromConsoleKey(ConsoleKey.NumPad7), 7);
        Numbers.Add(InputCode.FromConsoleKey(ConsoleKey.NumPad8), 8);
        Numbers.Add(InputCode.FromConsoleKey(ConsoleKey.NumPad9), 9);

        AttackStrategies.Add(InputCode.FromConsoleKey(ConsoleKey.M), AttackTypes.Magical);
        AttackStrategies.Add(InputCode.FromConsoleKey(ConsoleKey.N), AttackTypes.Normal);
        AttackStrategies.Add(InputCode.FromConsoleKey(ConsoleKey.S), AttackTypes.Stealthy);
    }

    private void InsertDefaultsPrimary()
    {
        PrimaryActionDictionary.Clear();
        PrimaryActionDictionary.Add(InputCode.FromConsoleKey(ConsoleKey.UpArrow), new MoveAction(Ctx, Direction.Up));
        PrimaryActionDictionary.Add(InputCode.FromConsoleKey(ConsoleKey.DownArrow), new MoveAction(Ctx, Direction.Down));
        PrimaryActionDictionary.Add(InputCode.FromConsoleKey(ConsoleKey.LeftArrow), new MoveAction(Ctx, Direction.Left));
        PrimaryActionDictionary.Add(InputCode.FromConsoleKey(ConsoleKey.RightArrow), new MoveAction(Ctx, Direction.Right));

        PrimaryActionDictionary.Add(InputCode.FromConsoleKey(ConsoleKey.W), new MoveAction(Ctx, Direction.Up));
        PrimaryActionDictionary.Add(InputCode.FromConsoleKey(ConsoleKey.S), new MoveAction(Ctx, Direction.Down));
        PrimaryActionDictionary.Add(InputCode.FromConsoleKey(ConsoleKey.A), new MoveAction(Ctx, Direction.Left));
        PrimaryActionDictionary.Add(InputCode.FromConsoleKey(ConsoleKey.D), new MoveAction(Ctx, Direction.Right));

        PrimaryActionDictionary.Add(InputCode.FromConsoleKey(ConsoleKey.I), new ChooseItemToEquipAction(Ctx));
        PrimaryActionDictionary.Add(InputCode.FromConsoleKey(ConsoleKey.E), new TryPickupItemAction(Ctx));
        PrimaryActionDictionary.Add(InputCode.FromConsoleKey(ConsoleKey.X), new DropItemAction(Ctx));
        PrimaryActionDictionary.Add(InputCode.FromConsoleKey(ConsoleKey.H), new FreeHandAction(Ctx));
        PrimaryActionDictionary.Add(InputCode.FromConsoleKey(ConsoleKey.B), new QuitAction(Ctx, _quitRef));

        PrimaryActionDictionary.Add(InputCode.FromConsoleKey(ConsoleKey.R), new RemapKeysAction(Ctx, _quitRef));

        PrimaryActionDictionary.Add(InputCode.FromConsoleKey(ConsoleKey.Q), new AttackAction(Ctx, _quitRef));
    }
    public override void Initialize(IInputPrimitives gameWorld)
    {
        GameWorld = gameWorld;

        InsertDefaultsSecondary();
        Ctx = new InputContext(gameWorld, Hands, ConfirmChoices, Numbers, AttackStrategies);
        InsertDefaultsPrimary();

    }

    public override InputReturn TakeInput()
    {
        var key = Console.ReadKey(true);
        var code = InputCode.FromConsoleKey(key.Key);
        
            _quitRef.Value = InputReturn.Continue;
        
        if (PrimaryActionDictionary.TryGetValue(code, out var bound))
        {


            bound.Action();


        }
        else
        {
            MessageBus.Send("Invalid input");
        }

        return _quitRef.Value;
    }

    public override bool RemapKeys()
    {
        MessageBus.Send("To change a key binding press P for primary, H for Hands, N for Numbers, B to exit");
        ConsoleKey input;
        if ((input = Console.ReadKey(true).Key) != ConsoleKey.B)
        {   
            
            
            var groupsInput = PrimaryActionDictionary
                .GroupBy(entry => string.Join("|",
                    entry.Value.AssociatedGameObjects
                        .Select(x => x.ToString())
                        .OrderBy(x => x)))
                .OrderBy(group => group.Key)
                .Select(group => group.OrderBy(x => x.Key.Description)).ToArray();
            var numOrdered = Numbers.GetForward.OrderBy(val => val.Value).ToList();
            var handsOrdered = Hands.GetForward.OrderBy(val => val.Value).ToList();

            int id;
            bool alter;
            ConsoleKey inp;
            switch (input)
            {

                case ConsoleKey.P:
                    MessageBus.Send($"Choose the group id [1-{groupsInput.Length}");
                    int gid = KeyboardUtils.ReadNumber(Ctx) - 1;
                    if (gid < 0 || gid > groupsInput.Count())
                    {
                        return true;
                    }

                    MessageBus.Send($"Do you want to remap the key[R] or add a new bind[A]");
                    inp = Console.ReadKey(true).Key;

                    switch (inp)
                    {
                        case ConsoleKey.A:
                            alter = false;
                            break;
                        case ConsoleKey.R:
                            alter = true;
                            break;
                        default:
                            return true;

                    }

                    var g = groupsInput[gid].ToList();
                    MessageBus.Send($"Choose the bind which you want to rep id [1-{g.Count}");
                    id = KeyboardUtils.ReadNumber(Ctx) - 1;
                    if (id < 0 || id >= g.Count)
                    {
                        return true;
                    }

                    MessageBus.Send("Choose the key you want to replace/add to the given action");
                    var k = Console.ReadKey(true).Key;
                    if (PrimaryActionDictionary.ContainsKey(InputCode.FromConsoleKey(k)))
                    {
                        MessageBus.Send("You can't have two actions binded to the same key");
                        return true;
                    }

                    var o = g[id].Value;
                    if (alter)
                    {
                        PrimaryActionDictionary.Remove(g[id].Key);
                    }

                    PrimaryActionDictionary.Add(InputCode.FromConsoleKey(k), o);
                    MessageBus.Send("Added succesfully");

                    break;
                case ConsoleKey.H:
                    MessageBus.Send($"Choose the bind which you want to rep id [1-{handsOrdered.Count}");
                    id = KeyboardUtils.ReadNumber(Ctx) - 1;
                    if (id < 0 || id >= handsOrdered.Count)
                    {
                        return true;
                    }

                    MessageBus.Send($"Do you want to remap the key[R] or add a new bind[A]");
                    inp = Console.ReadKey(true).Key;

                    switch (inp)
                    {
                        case ConsoleKey.A:
                            alter = false;
                            break;
                        case ConsoleKey.R:
                            alter = true;
                            break;
                        default:
                            return true;

                    }

                    var obj = handsOrdered[id];
                    MessageBus.Send("Choose the key you want to replace/add to the given action");
                    inp = Console.ReadKey(true).Key;
                    if (Hands.TryGetByLeft(InputCode.FromConsoleKey(inp), out var jk))
                    {
                        MessageBus.Send("You can't have two actions binded to the same key");
                        return true;
                    }

                    if (alter)
                    {
                        Hands.Remove(obj.Key);
                    }


                    Hands.Add(InputCode.FromConsoleKey(inp), obj.Value);
                    MessageBus.Send("Added succesfully");
                    break;
                case ConsoleKey.N:
                    MessageBus.Send($"Choose the bind which you want to rep id [1-{numOrdered.Count}");
                    id = KeyboardUtils.ReadNumber(Ctx) - 1;
                    if (id < 0 || id >= numOrdered.Count)
                    {
                        return true;
                    }

                    MessageBus.Send($"Do you want to remap the key[R] or add a new bind[A]");
                    inp = Console.ReadKey(true).Key;

                    switch (inp)
                    {
                        case ConsoleKey.A:
                            alter = false;
                            break;
                        case ConsoleKey.R:
                            alter = true;
                            break;
                        default:
                           
                            return true;

                    }

                    var oj = numOrdered[id];
                    MessageBus.Send("Choose the key you want to replace/add to the given action");
                    inp = Console.ReadKey(true).Key;
                    if (Numbers.TryGetByLeft(InputCode.FromConsoleKey(inp), out var kk))
                    {
                        MessageBus.Send("You can't have two actions binded to the same key");
                        
                        return true;
                    }

                    if (alter)
                    {
                        Numbers.Remove(oj.Key);
                    }


                    Numbers.Add(InputCode.FromConsoleKey(inp), oj.Value);
                    MessageBus.Send("Added succesfully");
                    
                    return true;
                    break;
                
            }
            
        }
        MessageBus.Clear();
        return false;

    }

    public class AttackAction(InputContext ctx, StrongBox<InputReturn> arg) : InputObject(ctx)
    {   
        private StrongBox<InputReturn> Quit = arg;
        public override void Action()
        {
            if (!ctx.Game.IsEnemy())
            {
                MessageBus.Send("No enemy to attack");
                return;
            } 
            Hand handchoice;

            (bool l, bool r, bool db) = ctx.Game.HandStatus();
            if (db)
            {
                handchoice = Hand.Left;
            }
            else if (!l && !r)
            {
                MessageBus.Send("No weapon equipped");
                return;
            }
            else if (l ^ r)
            {
                handchoice = l ? Hand.Left : Hand.Right;
            }
            else
            {    if (!Ctx.Hands.TryGetByRight(HandChoice.Left, out var left) ||
                     !Ctx.Hands.TryGetByRight(HandChoice.Right, out var right))
                {
                    MessageBus.Send("Hand bindings are not configured");
                    return;
                }
                MessageBus.Send(
                    $"Choose weapon to attack with by pressing {((ConsoleKey)left[^1].Code)}[left] or {((ConsoleKey)right[^1].Code)}[right]");

                var k = Console.ReadKey(true).Key;
                if (Ctx.Hands.TryGetByLeft(InputCode.FromConsoleKey(k), out var choice) &&
                    choice == HandChoice.Right)
                {
                    handchoice = Hand.Right;
                }
                else if (choice == HandChoice.Left)
                {
                    handchoice = Hand.Left;
                }
                else
                {
                    MessageBus.Send("Invalid Input");
                    return;
                }
            }

            Ctx.AttackStrategies.TryGetByRight(AttackTypes.Magical, out var m);
            Ctx.AttackStrategies.TryGetByRight(AttackTypes.Normal, out var n);
            Ctx.AttackStrategies.TryGetByRight(AttackTypes.Stealthy, out var s);
            MessageBus.Send(
                $"Choose strategy by pressing Magical: {((ConsoleKey)m[^1].Code)}, Stealthy: {((ConsoleKey)s[^1].Code)}, Normal: {((ConsoleKey)n[^1].Code)}");
            var key = Console.ReadKey(true).Key;
            AttackStrategy strategy;
            if (!ctx.AttackStrategies.TryGetByLeft(InputCode.FromConsoleKey(key), out var strat))
            {
                return;
            }
            switch (strat)
            {
                case AttackTypes.Magical:
                    strategy = new MagicStrategy();
                    break;
                case AttackTypes.Normal:
                    strategy= new NormalStrategy();
                    break;
                case AttackTypes.Stealthy:
                    strategy = new StealthStrategy();
                    break;
                default:
                    return;
            }
            foreach (var step in ctx.Game.AttackEnemyWith(handchoice, strategy))
            {
                if (step.Success != null)
                {
                    if (step.Success == false)
                        MessageBus.Send("Attack failed");
                    else
                    {
                        if (step.EnemyKilled==true)
                        { 
                            MessageBus.Send($"Player has killed the enemy, dealing him {step.DmgEnemy} damage");
                        }
                        else if(step.HeroSurvived==false)
                        {
                            MessageBus.Send($"This was our Hero's final battle.");
                                MessageBus.Send("You lost");
                                Quit.Value = InputReturn.Stop;
                        }
                        
                    }
                        
                }
                else
                {
                    if (step.DmgEnemy != null)
                    {
                        MessageBus.Send($"Player has dealt {step.DmgEnemy} damage to the enemy");
                        
                    }
                    else if (step.DmgHero != null)
                    {
                        MessageBus.Send($"Enemy has dealt {step.DmgHero} damage to the hero");
                    }
                }
            }
        }
        private readonly List<GameObjects> _objectsList = new List<GameObjects>([GameObjects.Enemies]);
        public override string GuideDescription { get => "Attack an oponnent"; }
        public override List<GameObjects> AssociatedGameObjects { get => _objectsList; }
    }
    public class RemapKeysAction(InputContext ctx, StrongBox<InputReturn> arg) : InputObject(ctx)
    {
        private StrongBox<InputReturn> _arg = arg;
        public override void Action()
        {
            _arg.Value = InputReturn.Remap;
        }
        private readonly List<GameObjects> _objectsList = new List<GameObjects>([GameObjects.All]);
        public override string GuideDescription { get => "Remap keys"; }
        public override List<GameObjects> AssociatedGameObjects { get => _objectsList; }
    }
    public class MoveAction(InputContext ctx, Direction arg) : InputObject(ctx)
    {
        private Direction _arg = arg;
        public override void Action()
        {
           
            var dir = _arg;
            var ret = Ctx.Game.MoveHero(dir);
            if (ret != (-1, -1))
                MessageBus.Send($"Hero moved to {ret.x} {ret.y}");
            else
                MessageBus.Send("Couldn't move the Hero");
        }

        public override string GuideDescription => $"{_arg.ToString()}";
        private readonly List<GameObjects> _objectsList = new List<GameObjects>([GameObjects.Movement]);

        public override List<GameObjects> AssociatedGameObjects { get => _objectsList; }
    }

    public class QuitAction(InputContext ctx, StrongBox<InputReturn> arg) : InputObject(ctx)
    {
        private StrongBox<InputReturn> _arg = arg;
        public override void Action()
        {
            
            if (!Ctx.ConfirmChoices.TryGetByRight(ConfirmChoice.Confirm, out var confirmCodes) || confirmCodes.Count == 0)
            {
                _arg.Value = InputReturn.Stop;
                return;
            }

            var confirmCode = confirmCodes[^1];
            MessageBus.Send($"If you are sure you want to quit the game press {((ConsoleKey)confirmCode.Code)}");
            var inp = Console.ReadKey(true);

            if (Ctx.ConfirmChoices.TryGetByLeft(InputCode.FromConsoleKey(inp.Key), out var choice) &&
                choice == ConfirmChoice.Confirm)
            {
                _arg.Value = InputReturn.Stop;
            }
            else
            {
                MessageBus.Send("Quit canceled");
            }
        }
        private readonly List<GameObjects> _objectsList = new List<GameObjects>([GameObjects.All]);

        public override List<GameObjects> AssociatedGameObjects { get => _objectsList; }
        public override string GuideDescription => "Quit the game";
    }

    public class FreeHandAction(InputContext ctx) : InputObject(ctx)
    {
        public override void Action()
        {
            if (!Ctx.Hands.TryGetByRight(HandChoice.Left, out var l) ||
                !Ctx.Hands.TryGetByRight(HandChoice.Right, out var r))
            {
                MessageBus.Send("Hand bindings are not configured");
                return;
            }

            MessageBus.Send(
                $"Choose hand to drop by pressing {((ConsoleKey)l[^1].Code)}[left] or {((ConsoleKey)r[^1].Code)}[right]");
            var key = Console.ReadKey(true);

            Hand? hand = null;
            if (Ctx.Hands.TryGetByLeft(InputCode.FromConsoleKey(key.Key), out var choice) &&
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

            var item = Ctx.Game.FreeHerosHand(hand.Value);
            if (item == null)
                MessageBus.Send("You don't have anything in this hand");
            else
                MessageBus.Send($"{item.Name} freed from hands");
        }
        private readonly List<GameObjects> _objectsList = new List<GameObjects>([GameObjects.Item]);

        public override List<GameObjects> AssociatedGameObjects { get => _objectsList; }
        public override string GuideDescription => "Free Hero's hands";
    }

    public class DropItemAction(InputContext ctx) : InputObject(ctx)
    {
        public override void Action()
        {
            var cnt = Ctx.Game.CntInventoryItems();
            if (cnt == 0)
            {
                MessageBus.Send("You don't have anything to drop");
                return;
            }

            MessageBus.Send("Choose item number to drop");
            var num = KeyboardUtils.ReadNumber(Ctx);
            if (num < 1 || num > cnt)
            {
                MessageBus.Send("Invalid choice");
                return;
            }

            var ret = Ctx.Game.DropItem(num);
            if (!ret.result)
                MessageBus.Send($"You couldn't drop {ret.item?.Name}");
            else
                MessageBus.Send($"You have dropped {ret.item?.Name}");
        }
        private readonly List<GameObjects> _objectsList = new List<GameObjects>([GameObjects.Item]);

        public override List<GameObjects> AssociatedGameObjects { get => _objectsList; }
        public override string GuideDescription => "Drop from inventory";
    }

    public class ChooseItemToEquipAction(InputContext ctx) : InputObject(ctx)
    {
        public override void Action()
        {
            var cnt = Ctx.Game.CntInventoryItems();
            if (cnt == 0)
            {
                MessageBus.Send("You don't have anything to equip");
                return;
            }

            MessageBus.Send("Choose item number to equip");
            var num = KeyboardUtils.ReadNumber(Ctx);
            if (num < 1 || num > cnt)
            {
                MessageBus.Send("Invalid choice");
                return;
            }

            Hand hand = Hand.Left;
            if (!Ctx.Game.GetInventoryItem(num).IsTwoHanded)
            {
                if (!Ctx.Hands.TryGetByRight(HandChoice.Left, out var l) ||
                    !Ctx.Hands.TryGetByRight(HandChoice.Right, out var r))
                {
                    MessageBus.Send("Hand bindings are not configured");
                    return;
                }

                MessageBus.Send(
                    $"Choose hand to equip by pressing {((ConsoleKey)l[^1].Code)}[left] or {((ConsoleKey)r[^1].Code)}[right]");

                var key = Console.ReadKey(true);
                if (Ctx.Hands.TryGetByLeft(InputCode.FromConsoleKey(key.Key), out var choice) &&
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

            var it = Ctx.Game.EquipItem(num, hand);
            if (it == null)
            {
                MessageBus.Send("Empty your hands before equipping");
                return;
            }

            MessageBus.Send($"{it.Name} successfully equipped");
        }
        private readonly List<GameObjects> _objectsList = new List<GameObjects>([GameObjects.Item]);

        public override List<GameObjects> AssociatedGameObjects { get => _objectsList; }
        public override string GuideDescription => "Equip an item from inventory";
    }

    public class TryPickupItemAction(InputContext ctx) : InputObject(ctx)
    {
        public override void Action()
        {
            var cntField = Ctx.Game.CntFieldItems();
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
                cnt = KeyboardUtils.ReadNumber(Ctx);
                if (cnt < 1 || cnt > cntField)
                {
                    MessageBus.Send("Invalid choice");
                    return;
                }
            }

            var ret = Ctx.Game.PickupItem(cnt);
            if (ret.item == null)
                MessageBus.Send("Invalid input");
            else if (ret.result)
                MessageBus.Send($"You have picked up {ret.item.Name}");
            else
                MessageBus.Send($"You couldn't pick up {ret.item.Name}");
        }
        private readonly List<GameObjects> _objectsList = new List<GameObjects>([GameObjects.Item]);

        public override List<GameObjects> AssociatedGameObjects { get => _objectsList; }
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
            if (ctx.ConfirmChoices.TryGetByLeft(InputCode.FromConsoleKey(k.Key), out var conf) && conf == ConfirmChoice.Confirm)
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
