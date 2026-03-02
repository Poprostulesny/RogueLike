using System.Text;

namespace OODProject;


public class GameWorld
{
    public Field[,] World = new Field[42,22];
    public Hero Player = new Hero();
    private List<(int x, int y)> free_spaces;
    private int HeroPosX => Player.PosX;
    private int HeroPosY => Player.PosY;
    Random rand = new Random(DateTime.Now.Microsecond);
    public void AddItemRandomly(IItem item)
    {
       
        while (true)
        {
            int pick = rand.Next(0, free_spaces.Count);
            int x, y;
            (x,y) =  free_spaces[pick];
            if (World[y, x].TryAddItem(item))
            {
                break;
            }
        }
        
    }

    public void CreateEmptyBlockList()
    {
        free_spaces = new List<(int x, int y)>();
        for(int x=0; x < 22;x++)
        {
            for (int y = 0; y < 42; y++)
            {
                if (World[y, x].CanBeEntered)
                {
                    free_spaces.Add((x, y));
                }
            }
        }
        
    }
    public GameWorld()
    {
        WorldGenerator.MazeWithRooms(ref World);
        CreateEmptyBlockList();
        World[1, 1].TryAddHero(ref Player, 1,1);
        AddItemRandomly(new Gold(10));
        AddItemRandomly(new Coins(10));
        AddItemRandomly(new Broomstick());
        AddItemRandomly(new DragonSlayerSword());
        AddItemRandomly(new Teapot());
        AddItemRandomly(new BrokenSword());
        AddItemRandomly(new RustySword());
    }
    
    bool TakeInput()
    { 
        
        ConsoleKeyInfo key = Console.ReadKey(true);
        switch (key.Key)
        {
            case ConsoleKey.UpArrow:
                MoveHero(Direction.Up);
                break;
            case ConsoleKey.DownArrow:
                MoveHero(Direction.Down);
                break;
            case ConsoleKey.LeftArrow:
                MoveHero(Direction.Left);
                break;
            case ConsoleKey.RightArrow:
                MoveHero(Direction.Right);
                break;
            case ConsoleKey.W:
                MoveHero(Direction.Up);
                break;
            case ConsoleKey.S:
                MoveHero(Direction.Down);
                break;
            case ConsoleKey.A:
                MoveHero(Direction.Left);
                break;
            case ConsoleKey.D:
                MoveHero(Direction.Right);
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

        return true;
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
        if ((item  = Player.hands.TryRemove(hand, Player) )== null)
        {
            MessageBus.Send("You don't have anything in this hand");
        }
        else
        {
            MessageBus.Send($"{item.Name} freed from hands");
        }
    }
    int ReadNumber()
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
    void ChooseItemToDrop()
    {
        MessageBus.Send("Choose item number to drop");
        int cnt = Player.inventory.Items.Count;
        if (cnt == 0)
        {
            MessageBus.Send("You don't have anything to drop");
            return;
        }

        int num = ReadNumber();
        if ( num < 1 || num > cnt)
        {
            MessageBus.Send("Invalid choice");
            return;
        }

        num -= 1;
        

        if (Player.hands.Left == Player.inventory.Items[num])
        {
            Player.hands.TryRemove(Hand.Left, Player);
            
        }
        else if (Player.hands.Right == Player.inventory.Items[num])
        {
            Player.hands.TryRemove(Hand.Right, Player);
        }

        World[HeroPosY, HeroPosX].TryAddItem(Player.inventory.Items[num]);
        Player.inventory.Remove(Player.inventory.Items[num]);
        
    }
    void ChooseItemToEquip()
    {
        MessageBus.Send("Choose item number to equip");
        int cnt = Player.inventory.Items.Count;
        if (cnt == 0)
        {
            MessageBus.Send("You don't have anything to equip");
            return;
        }

        int num = ReadNumber();
       
        if ( num < 1 || num > cnt)
        {  
            MessageBus.Send("Invalid choice");
            return;
        }

        num -= 1;
        

        if (Player.inventory.Items[num].isTwoHanded)
        {
            if (Player.hands.TryEquip(Player.inventory.Items[num], Hand.Left, Player))
            {
                MessageBus.Send($"{Player.inventory.Items[num].Name} succesfully equipped");
            }
            else
            {
                MessageBus.Send($"Empty your hands before equipping");
            }
        }
        else
        {
            MessageBus.Send("Choose hand to equip by pressing l or r");
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

            if (Player.hands.TryEquip(Player.inventory.Items[num], hand, Player))
            {
                MessageBus.Send($"{Player.inventory.Items[num].Name} equipped");
            }
            else
            {
                MessageBus.Send($"Empty your hands before equipping");
            }
        }
        
    }
    void TryPickupItem()    
    {
        if (World[HeroPosY, HeroPosX].Items.Count <= 0)
        {
            MessageBus.Send("You don't have anything to pickup");
            return;
        }

        if (World[HeroPosY, HeroPosX].Items.Count == 1)
        {   var it = World[HeroPosY, HeroPosX].Items[0];
            if(World[HeroPosY,HeroPosX].TryTakeItem(World[HeroPosY,HeroPosX].Items[0])==true)
            {   it.OnPickup(Player); 
                MessageBus.Send("Item picked up successfully");
            }
            else
            {
                MessageBus.Send($"You can't pick it up");
            }
            return;
        }
        MessageBus.Send("Choose item number to pick up");
        int cnt = ReadNumber();
        if (cnt < 1 || cnt >= World[HeroPosY, HeroPosX].Items.Count)
        {
            MessageBus.Send("Invalid choice");
        }

        cnt--;

        var item = World[HeroPosY, HeroPosX].Items[cnt];
        if(World[HeroPosY,HeroPosX].TryTakeItem(World[HeroPosY,HeroPosX].Items[cnt])==true)
        {   item.OnPickup(Player); 
            MessageBus.Send("Item picked up successfully");
        }
        else
        {
            MessageBus.Send($"You can't pick it up");
        }

        return;
    }
    void MoveHero(Direction dir)
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
        
           

        int oldX = HeroPosX;
        int oldY = HeroPosY;
        int newX = oldX + xplus;
        int newY = oldY + yplus;

        if (World[newY, newX].TryAddHero(ref Player,newX, newY))
        {
            World[oldY, oldX].RemoveHero(); 
            MessageBus.Send($"Hero moved to {newX} {newY}");
        }
          
        
    }

    public string[] DisplayField()
    {
        return World[HeroPosY, HeroPosX].Display();
    }

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
        
        while (TakeInput())
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
}
