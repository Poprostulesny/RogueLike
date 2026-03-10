namespace OODProject;

public class GameWorld
{
   
    public Hero Player = new();

    
    public Field[,] World = new Field[42, 22];

    public GameWorld()
    {
        /*WorldGenerator.MazeWithRooms(World);
        CreateEmptyBlockList();
        World[1, 1].TryAddHero(ref Player, 1, 1);
        AddItemRandomly(new Gold(10));
        AddItemRandomly(new Coins(10));
        AddItemRandomly(new Broomstick());
        AddItemRandomly(new DragonSlayerSword());
        AddItemRandomly(new Teapot());
        AddItemRandomly(new BrokenSword());
        AddItemRandomly(new RustySword());*/
        WorldGeneratorNew.FilledDungeon(World);
        //WorldGeneratorNew.AddPaths(World, 10);
        WorldGeneratorNew.AddChambers(World);
        WorldGeneratorNew.AddPaths(World, 10);
        WorldGeneratorNew.AddCentralRoom(World, 7, 10);
        List<IItem> items = new List<IItem>()
        {
            new Gold(10),
            new Coins(10),
            new Broomstick(),
            new Teapot(),
            new BrokenSword()
        };
        
        List<IInventoryItem> weapons = new List<IInventoryItem>()
        {
            new DragonSlayerSword(),
            new RustySword(),
            new Shield()
        };
        WorldGeneratorNew.AddItems(World, items, 15);
        WorldGeneratorNew.AddWeapons(World, weapons, 10);
        World[1, 1].TryAddHero(ref Player, 1, 1);
        
    }

 
}