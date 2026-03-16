using System.Text;

namespace OODProject;

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
    private readonly StringBuilder DisplayMessage = new();
    private readonly IGameWorldView gameEngine;
    


    public Renderer(IInput input)
    {
        MessageBus.Message += msg =>
        {
            DisplayMessage.AppendLine(msg);
            DisplayWorldState();
        };
        MessageBus.ClearMessage += () => { DisplayMessage.Clear(); };
        var g = new GameWorld();    
        gameEngine = g;
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

        while (_input.TakeInput())
        {
            MessageBus.Clear();
            var dif = DateTime.Now - t;
            t = DateTime.Now;
            if (dif.TotalMilliseconds < 100) Thread.Sleep(100 - (int)dif.TotalMilliseconds);
        }
    }


    private void DisplayWorldState()
    {
        Console.OutputEncoding = Encoding.UTF8;
        var width = Console.WindowWidth;
        var height = Console.WindowHeight;
        var display = new List<StringBuilder>();
        for (var i = 0; i < gameEngine.Width; i++)
        {
            display.Add(new StringBuilder());
            for (var y = 0; y < gameEngine.Height; y++)
            {
                display[i].Append(gameEngine.GetGlyphAt(y,i));
                if (gameEngine.GetGlyphAt(y,i) != '█' && gameEngine.GetGlyphAt(y,i) != ' ')
                    display[i].Append(' ');
                else
                    display[i].Append(gameEngine.GetGlyphAt(y,i));
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
        var field = DisplayField(gameEngine.PlayerPosX, gameEngine.PlayerPosY);

        foreach (var s in field)
        {
            if (cnt >= display.Count) display.Add(new StringBuilder());
            display[cnt].Append(s);
            cnt++;
        }

        display.Add(DisplayGuide());
        foreach (var s in DisplayMessage.ToString().Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries))
            display.Add(new StringBuilder(s));

        while (display.Count < height) display.Add(new StringBuilder());
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

    public string[] DisplayInventory()
    {
        (var inventory, var capacity) = gameEngine.GetInventoryItems();
        var display = new string[capacity + 1];
        display[0] = "Inventory:";
        var cnt = 1;
        foreach (var item in inventory)
        {
            display[cnt] = new string($"{cnt.ToString()}. {item.Name}");
            cnt++;
        }

        while (cnt < capacity  +1)
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
        var occupant = gameEngine.GetOccupantAt(x, y);
        if (occupant != null) 
            disp.Add(new string(
                $"Enemy: {occupant.Name}: {occupant.Description}"));
        var items = gameEngine.GetItemsAt(x, y);
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


    public string DisplayHands()
    {   
        var hands = gameEngine.GetHands();
        if ( hands.isTwoHandedEquipped) return $"Both Hands: { hands.Left.Name}";

        if ( hands.Left == null &&  hands.Right == null)
            return "Left: None | Right: None";

        if ( hands.Left == null) return $"Left: None | Right: { hands.Right.Name}";

        if ( hands.Right == null) return $"Left: { hands.Left.Name} | Right: None";
        return $"Left: { hands.Left.Name} | Right: { hands.Right.Name}";
    }

    public string[] DisplayStats()
    {
        var s = gameEngine.GetHeroStats();
        var stats = new string[8];
        stats[0] = "Hero";
        stats[1] = $"Strength: {s.Strength}";
        stats[2] = $"Agility: {s.Agility}";
        stats[3] = $"Wisdom: {s.Wisdom}";
        stats[4] = $"Persuasion: {s.Persuasion}";
        stats[5] = $"Health: {s.Health}";
        stats[6] = $"Defense: {s.Defense}";
        stats[7] = $"Gold: {s.Gold} | Coins: {s.Coins}";

        return stats;
    }

    public StringBuilder DisplayGuide()
    {
        var guide = new StringBuilder();
        var i = 0;
        foreach (var worldObject in gameEngine.WorldFeatures)
        {
            if (i != 0) guide.Append("|");
            guide.Append(_input.Description[worldObject]);
            i++;
        }

        
        return guide;
    }
}