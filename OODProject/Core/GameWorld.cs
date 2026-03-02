using System.Text;

namespace OODProject;


public class GameWorld
{
    public Field[,] World = new Field[42,22];
    public Hero Player = new Hero();
    private List<(int x, int y)> free_spaces;
   
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
    


   

   
}
