using System.Text;

namespace OODProject;

public static class MessageBus
{
    public static event Action<string>? Message;
    public static event Action? ClearMessage;
    public static void Send(string message) => Message?.Invoke(message);
    
    public static void Clear()=>ClearMessage?.Invoke();
}

public class Renderer
{
    public Renderer(IInput input)
    {
        gameEngine = new GameWorld();
        _input = input;
        _input.Initialize(ref gameEngine);
        Console.SetBufferSize(Console.WindowWidth, Console.WindowHeight);
        MessageBus.Message += msg =>
        {
            DisplayMessage.AppendLine(msg);
            msg_cnt++;
            DisplayWorldState();
        };
        MessageBus.ClearMessage += () =>
        {   
            prev_msg_cnt=msg_cnt;
            msg_cnt = 0;
            DisplayMessage.Clear();
            
        };
    }

    private IInput _input;
    private int msg_cnt = 0;
    private int prev_msg_cnt = 0;
    private GameWorld gameEngine; 
    private StringBuilder DisplayMessage = new StringBuilder();
    
    public void Play()
    {
        
            Console.WriteLine("Guide:\nYou can control the movement of the Hero by either WSAD or arrows.\nH - Freeing hands\nE - Picking up items\nI - Taking an item into your hands\nX - Dropping item from the inventory\nEscape - Quitting the game\n\nIf you understand press Y to start the game");
            ConsoleKeyInfo key = Console.ReadKey(true);
            while (key.Key != ConsoleKey.Escape && key.Key != ConsoleKey.Y)
            {
                
                key = Console.ReadKey(true);
            
            }
            if (key.Key == ConsoleKey.Escape)
            {
                return;
            }
            Console.Clear();
            MessageBus.Send("Game Start");
            var t = DateTime.Now;
        
            while (_input.TakeInput())
            {   
                MessageBus.Clear(); 
                var dif =  DateTime.Now - t;
                t = DateTime.Now;
                if (dif.TotalMilliseconds < 100 )
                {
                    Thread.Sleep(100-(int)dif.TotalMilliseconds);
                }
            
           
            }
       
        
        
    }

    
    private void DisplayWorldState()
    {
        Console.OutputEncoding = Encoding.UTF8;
        int width = Console.WindowWidth;
        int height = Console.WindowHeight;
        List<StringBuilder> display = new List<StringBuilder>();
        for (int i = 0; i < 42; i++)
        {
            display.Add(new StringBuilder());
            for (int y = 0; y < 22; y++)
            {
               
                display[i].Append(gameEngine.World[i,y].Glyph);
                if (gameEngine.World[i, y].Glyph != '█' && gameEngine.World[i, y].Glyph != ' ')
                {
                    display[i].Append(' ');
                }
                else
                {
                    display[i].Append(gameEngine.World[i,y].Glyph);
                }
                
            }
            display[i].Append('|');
            
        }

        int cnt = 0;
        string[] stats = DisplayStats();
        foreach (string s in stats)
        {
            if (cnt >= display.Count)
            {
                display.Add(new StringBuilder());
            }
            display[cnt].Append(s);
            cnt++;
        }
        string[] inventory = DisplayInventory();
        foreach (string s in inventory)
        {
            if (cnt >= display.Count)
            {
                display.Add(new StringBuilder());
            }
            display[cnt].Append(s);
            cnt++;
        }
        display[cnt].Append(DisplayHands());
        cnt++;
        string[] field = DisplayField(gameEngine.Player.PosX, gameEngine.Player.PosY);

        foreach (string s in field)
        {   
            if (cnt >= display.Count)
            {
                display.Add(new StringBuilder());
            }
            display[cnt].Append(s);
            cnt++;
        }
        display.Add(new StringBuilder("H - Freeing hands|E - Picking up items|I - Taking an item into your hands|X - Dropping item from the inventory|Escape - Quitting the game"));
        foreach (var s in DisplayMessage.ToString().Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries))
        {
            display.Add(new StringBuilder(s));
        }
        
        while (display.Count < height)
        {
            display.Add(new StringBuilder());
        }
        StringBuilder buffer =  new StringBuilder(height * width);
        for (int i = 0; i < height; i++)
        {
            string line = display[i].ToString();
            if (line.Length > width - 1)
            {
                line = line.Substring(0, width - 1);
            }
            buffer.Append(line.PadRight(width - 1));
            if (i < height - 1)
            {
                buffer.Append('\n');
            }
        }
        
        
        
        Console.SetCursorPosition(0, 0);
        Console.Write(buffer.ToString());
        
    }
    
    public string[] DisplayInventory()
    {
        string[] display = new string[gameEngine.Player.inventory.Capacity+1];
        display[0] = "Inventory:";
        int cnt = 1;
        foreach(var item in gameEngine.Player.inventory.Items)
        {
            display[cnt] = new string($"{cnt.ToString()}. {item.Name}");
            cnt++;
        }

        while (cnt < gameEngine.Player.inventory.Capacity+ + 1)
        {
            display[cnt] = "";
            cnt++;
        }

        return display;
    }
    public string[] DisplayField(int x, int y)
    {
        List<string> disp = new List<string>();
        disp.Add(new string("Player stands on an Empty Field:"));
        if (gameEngine.World[y,x].isOccupied)
        {
            disp.Add(new string($"Enemy: {gameEngine.World[y,x].Occupant.Name}: {gameEngine.World[y,x].Occupant.Description}"));
        }

        if (gameEngine.World[y,x].Items.Count != 0)
        {
            disp.Add(new string("Items:"));
            int cnt = 1;
            foreach (IItem item in gameEngine.World[y,x].Items)
            {
                disp.Add(new string($"- {cnt.ToString()}. {item.Name}: {item.Description}"));
                cnt++;
            }
        }
        return disp.ToArray();
        
    }

    
    public string DisplayHands()
    {
        if (gameEngine.Player.hands.isTwoHandedEquipped)
        {
            return $"Both Hands: {gameEngine.Player.hands.Left.Name}";
        }

        if (gameEngine.Player.hands.Left == null && gameEngine.Player.hands.Right == null)
        {
            return "Left: None | Right: None";
        }

        if (gameEngine.Player.hands.Left == null)
        {
            return $"Left: None | Right: {gameEngine.Player.hands.Right.Name}";
        }

        if (gameEngine.Player.hands.Right == null)
        {
            return $"Left: {gameEngine.Player.hands.Left.Name} | Right: None";
        }
        return $"Left: {gameEngine.Player.hands.Left.Name} | Right: {gameEngine.Player.hands.Right.Name}";
    }
    public string[] DisplayStats()
    {
        string[] stats = new string[8];
        stats[0] = "Hero";
        stats[1] = $"Strength: {gameEngine.Player.stats.Strength}";
        stats[2] = $"Agility: {gameEngine.Player.stats.Agility}";
        stats[3] = $"Wisdom: {gameEngine.Player.stats.Wisdom}";
        stats[4] = $"Persuasion: {gameEngine.Player.stats.Persuasion}";
        stats[5] = $"Health: {gameEngine.Player.stats.Health}";
        stats[6] = $"Defense: {gameEngine.Player.stats.Defense}";
        stats[7] = $"Gold: {gameEngine.Player.stats.Gold} | Coins: {gameEngine.Player.stats.Coins}";
        
        return stats;
    }
    
    
    
    
}
