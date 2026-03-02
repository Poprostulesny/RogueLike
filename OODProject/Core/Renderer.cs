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
    public Renderer()
    {
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

    private int msg_cnt = 0;
    private int prev_msg_cnt = 0;
    private GameWorld gameEngine = new GameWorld();
    private StringBuilder DisplayMessage = new StringBuilder();
 
    public void Play()
    {
        gameEngine.Play();
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
        string[] stats = gameEngine.Player.stats.DisplayStats();
        foreach (string s in stats)
        {
            if (cnt >= display.Count)
            {
                display.Add(new StringBuilder());
            }
            display[cnt].Append(s);
            cnt++;
        }
        string[] inventory = gameEngine.Player.inventory.DisplayInventory();
        foreach (string s in inventory)
        {
            if (cnt >= display.Count)
            {
                display.Add(new StringBuilder());
            }
            display[cnt].Append(s);
            cnt++;
        }
        display[cnt].Append(gameEngine.Player.hands.DisplayHands());
        cnt++;
        string[] field = gameEngine.DisplayField();

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


    

    
    
    
    
}
