using System.Text;

namespace OODProject.Core;

public static class MessageBus
{
    public static event Action<string>? Message;

    public static event Action? ClearMessage;

    public static void Send(string message)
    {
        Message?.Invoke(message);
    }

    public static void Clear()
    {
        ClearMessage?.Invoke();
    }
}

public class Renderer
{
    private readonly IInput _input;
    private readonly StringBuilder _displayMessage = new();
    private readonly IGameWorldView _gameEngine;
    private GameState _state = GameState.Play;
    private enum GameState
    {
        Play,
        Remap
    }


    public Renderer(IInput input)
    {
        MessageBus.Message += msg =>
        {
            _displayMessage.AppendLine(msg);
            if (_state == GameState.Play)
            {
                DisplayWorldState();
            }
            else
            {
                DisplayRemap();
            }
        };
        MessageBus.ClearMessage += () => { _displayMessage.Clear(); };
        var g = new GameWorld();
        _gameEngine = g;
        _input = input;
        _input.Initialize(g);
        Console.SetBufferSize(Console.WindowWidth, Console.WindowHeight);
    }

    public void Play()
    {
        Console.WriteLine(
            "Guide:\nYou can control the movement of the Hero by either WSAD or arrows.\nH - Freeing hands\nE - Picking up items\nI - Taking an item into your hands\nX - Dropping item from the inventory\nB - Quitting the game\n\nIf you understand press Y to start the game, you will always have access to the guide at the bottom of your screen");
        var key = Console.ReadKey(true);
        while (key.Key != ConsoleKey.Escape && key.Key != ConsoleKey.Y) key = Console.ReadKey(true);
        if (key.Key == ConsoleKey.Escape) return;
        Console.Clear();
        MessageBus.Send("Game Start");
        var t = DateTime.Now;
        InputReturn val;
        while ((val = _input.TakeInput()) != InputReturn.Stop)
        {
            if (val == InputReturn.Remap)
            {
                _state = GameState.Remap;
                do
                {
                    MessageBus.Clear();
                } while (_input.RemapKeys());

                _state = GameState.Play;
               
                continue;
            }
            MessageBus.Clear();
            var dif = DateTime.Now - t;
            t = DateTime.Now;
            if (dif.TotalMilliseconds < 100) Thread.Sleep(100 - (int)dif.TotalMilliseconds);
        }
    }

    private void DisplayRemap()
    {
        Console.OutputEncoding = Encoding.UTF8;
        var width = Console.WindowWidth;
        var height = Console.WindowHeight;
        var display = new List<StringBuilder>();
        static string FormatEntry(string left, string right, int id, int leftWidth, int rightWidth) =>
            $"{left.PadRight(leftWidth)} - {right.PadRight(rightWidth)} [{id}]";

        var groupsInput = _input.PrimaryActionDictionary
            .GroupBy(entry => string.Join("|",
                entry.Value.AssociatedGameObjects
                    .Select(x => x.ToString())
                    .OrderBy(x => x)))
            .OrderBy(group => group.Key)
            .Select(group => group
                .GroupBy(entry => entry.Value.GuideDescription)
                .OrderBy(subgroup => subgroup.Key)
                .Select(subgroup => subgroup.OrderBy(entry => entry.Key.Description)));

        Console.SetCursorPosition(0, 0);
        int gid = 1;
        int id = 1;
        foreach (var group in groupsInput)
        {
             id=1;
            var actionRows = group.Select(subgroup => new
                {
                    Description = subgroup.First().Value.GuideDescription,
                    Keys = subgroup.Select((entry) => $"{entry.Key.Description}[{id++}]").ToList()
                })
                .ToList();
            var leftWidth = actionRows.Select(row => row.Description.Length).DefaultIfEmpty(0).Max();
            var s = new StringBuilder();
            foreach (var obj in group.First().First().Value.AssociatedGameObjects)
            {
                s.Append(obj.ToString());
            }

            s.Append($" [{gid++}]");
            display.Add(s);

            foreach (var row in actionRows)
            {
                display.Add(new StringBuilder($"{row.Description.PadRight(leftWidth)} - {string.Join(", ", row.Keys)}"));
            }
           
            display.Add(new StringBuilder(""));
        }

        display.Add(new StringBuilder($"Hands"));
        id = 1;
        var handsOrdered = _input.Hands.GetForward.OrderBy(val => val.Value);
        var handsLeftColumnWidth = _input.Hands.GetForward.Select(entry => entry.Value.ToString().Length).DefaultIfEmpty(0).Max();
        var handsRightColumnWidth = _input.Hands.GetForward.Select(entry => entry.Key.Description.Length).DefaultIfEmpty(0).Max();
        foreach (var obj in handsOrdered)
        {
            display.Add(new StringBuilder(FormatEntry(
                obj.Value.ToString(),
                obj.Key.Description,
                id++,
                handsLeftColumnWidth,
                handsRightColumnWidth)));

        }
        display.Add(new StringBuilder(""));
        display.Add(new StringBuilder($"Numbers"));
        var numOrdered = _input.Numbers.GetForward.OrderBy(val => val.Value);
        var numbersLeftColumnWidth = _input.Numbers.GetForward.Select(entry => entry.Value.ToString().Length).DefaultIfEmpty(0).Max();
        var numbersRightColumnWidth = _input.Numbers.GetForward.Select(entry => entry.Key.Description.Length).DefaultIfEmpty(0).Max();
        id = 1;
        foreach (var obj in numOrdered)
        {
            display.Add(new StringBuilder(FormatEntry(
                obj.Value.ToString(),
                obj.Key.Description,
                id++,
                numbersLeftColumnWidth,
                numbersRightColumnWidth)));
        }
        display.Add(new StringBuilder(""));
        display.Add(new StringBuilder($"Confirmation Choices"));
        var confOrdered = _input.ConfirmChoices.GetForward.OrderBy(val => val.Value);
        var confirmationLeftColumnWidth = _input.ConfirmChoices.GetForward.Select(entry => entry.Value.ToString().Length).DefaultIfEmpty(0).Max();
        var confirmationRightColumnWidth = _input.ConfirmChoices.GetForward.Select(entry => entry.Key.Description.Length).DefaultIfEmpty(0).Max();
        id = 1;
        foreach (var obj in confOrdered)
        {
            display.Add(new StringBuilder(FormatEntry(
                obj.Value.ToString(),
                obj.Key.Description,
                id++,
                confirmationLeftColumnWidth,
                confirmationRightColumnWidth)));
        }

        foreach (var s in _displayMessage.ToString().Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries))
            display.Add(new StringBuilder(s));
        int size = display.Count;
        DisplayBuffer(width, height, display);
        Console.SetCursorPosition(0, Math.Min(height - 1, size));

    }

    private void DisplayWorldState()
    {
        Console.OutputEncoding = Encoding.UTF8;
        var width = Console.WindowWidth;
        var height = Console.WindowHeight;
        var display = new List<StringBuilder>();
        for (var i = 0; i < _gameEngine.Width; i++)
        {
            display.Add(new StringBuilder());
            for (var y = 0; y < _gameEngine.Height; y++)
            {
                display[i].Append(_gameEngine.GetGlyphAt(y, i));
                if (_gameEngine.GetGlyphAt(y, i) != '█' && _gameEngine.GetGlyphAt(y, i) != ' ')
                    display[i].Append(' ');
                else
                    display[i].Append(_gameEngine.GetGlyphAt(y, i));
            }

            display[i].Append('|');
        }

        var cnt = 0;
        var stats = DisplayStats();
        foreach (var s in stats)
        {
            if (cnt >= display.Count) display.Add(new StringBuilder());
            display[cnt].Append(s);
            cnt++;
        }

        var inventory = DisplayInventory();
        foreach (var s in inventory)
        {
            if (cnt >= display.Count) display.Add(new StringBuilder());
            display[cnt].Append(s);
            cnt++;
        }

        display[cnt].Append(DisplayHands());
        cnt++;
        var field = DisplayField(_gameEngine.PlayerPosX, _gameEngine.PlayerPosY);

        foreach (var s in field)
        {
            if (cnt >= display.Count) display.Add(new StringBuilder());
            display[cnt].Append(s);
            cnt++;
        }

        //display.Add(DisplayGuide());
        foreach (var s in SplitLine(DisplayGuide().ToString(),width))
            display.Add(new StringBuilder(s));
        foreach (var s in _displayMessage.ToString().Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries))
            display.Add(new StringBuilder(s));
        int size = display.Count;

        DisplayBuffer(width, height, display);

        Console.SetCursorPosition(0, Math.Min(height - 1, size));
    }

    private string[] SplitLine(string line, int width)
    {
        if (line.Length < width)
        {
            return new string[] { line };
        }
        var stringarray = new List<string>();
        int lastdash = 0;
        int prevDash = 0;
        
        for (int i = 0; i < line.Length; i++)
        {
            if (line[i] == '|')
            {   
                lastdash = i;
            }

            if (i != 0 && (i-prevDash) % width == 0)
            {   
                
                stringarray.Add(line.Substring(prevDash, lastdash - prevDash).Trim());
                prevDash = lastdash + 1;

            }
        }

        if (lastdash != line.Length - 1)
        {
            stringarray.Add(line.Substring(prevDash, line.Length-prevDash).Trim());
        }
        return stringarray.ToArray();
    }
    public string[] DisplayInventory()
    {
        (var inventory, var capacity) = _gameEngine.GetInventoryItems();
        var display = new string[capacity + 1];
        display[0] = "Inventory:";
        var cnt = 1;
        foreach (var item in inventory)
        {
            display[cnt] = new string($"{cnt.ToString()}. {item.Name}");
            cnt++;
        }

        while (cnt < capacity + 1)
        {
            display[cnt] = "";
            cnt++;
        }

        return display;
    }

    public string[] DisplayField(int x, int y)
    {
        var disp = new List<string>();
        disp.Add(new string("Player stands on an Empty Field:"));
        var occupant = _gameEngine.GetOccupantAt(x, y);
        if (occupant != null)
        {   disp.Add(new string(
                $"Enemy: {occupant.Name}: {occupant.Description}"));
            disp.Add(new string($"HP: {occupant.HealthPoints} | Armor: {occupant.ArmorPoints} | Attack {occupant.AttackValue}"));
        }
        var items = _gameEngine.GetItemsAt(x, y);
        if (items.Count != 0)
        {
            disp.Add(new string("Items:"));
            var cnt = 1;
            foreach (var item in items)
            {
                disp.Add(new string($"- {cnt.ToString()}. {item.Name}: {item.Description}"));
                cnt++;
            }
        }

        return disp.ToArray();
    }

    private void DisplayBuffer(int width, int height, List<StringBuilder> display)
    {
        while (display.Count < height)
        {
            display.Add(new StringBuilder(""));
        }
        var buffer = new StringBuilder(height * width);
        for (var i = 0; i < height; i++)
        {
            var line = display[i].ToString();
            if (line.Length > width - 1) line = line.Substring(0, width - 1);
            buffer.Append(line.PadRight(width - 1));
            if (i < height - 1) buffer.Append('\n');
        }
        Console.SetCursorPosition(0, 0);
        Console.Write(buffer.ToString());
    }
    public string DisplayHands()
    {
        var hands = _gameEngine.GetHands();
        if (hands.IsTwoHandedEquipped) return $"Both Hands: {hands.Left?.Name}";

        if (hands.Left == null && hands.Right == null)
            return "Left: None | Right: None";

        if (hands.Left == null&&hands.Right!=null) return $"Left: None | Right: {hands.Right.Name}";

        if (hands.Right == null&&hands.Left!=null) return $"Left: {hands.Left.Name} | Right: None";
        return $"Left: {hands.Left?.Name} | Right: {hands.Right?.Name}";
    }

    public string[] DisplayStats()
    {
        var s = _gameEngine.GetHeroStats();
        return
        [
            "Hero",
            $"Health: {s.Health}",
            $"Defense: {s.Defense}",
            $"Strength: {s.Strength}",
            $"Dexterity: {s.Dexterity}",
            $"Agility: {s.Agility}",
            $"Wisdom: {s.Wisdom}",
            $"Luck: {s.Luck}",
            $"Aggression: {s.Aggression}",
            $"Persuasion: {s.Persuasion}",
            $"Gold: {s.Gold} | Coins: {s.Coins}"
        ];
    }

    public StringBuilder DisplayGuide()
    {
        var guide = new StringBuilder();

        var availableActions = _input.PrimaryActionDictionary
            .Where(entry =>
                entry.Value.AssociatedGameObjects.Any(gameObject =>
                    _gameEngine.WorldFeatures.Contains(gameObject)));

        var groupedActions = availableActions
            .GroupBy(entry => entry.Value.GuideDescription);

        var guideText = groupedActions
            .Select(group =>
            {
                var descriptions = group
                    .Select(entry => entry.Key.Description)
                    .Distinct();

                return $"{group.Key} - [{string.Join(", ", descriptions)}]";
            });

        guide.Append(string.Join(" | ", guideText));


        return guide;
    }
}
