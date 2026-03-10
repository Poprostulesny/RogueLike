namespace OODProject;

public enum GameObjects
{
    Item,
    Enemies,
    Movement,
    Quitting
}

public class GameWorld
{
    public readonly SortedSet<GameObjects> WorldFeatures;

    public Hero Player = new();
    public Field[,] World = new Field[42, 22];

    public GameWorld()
    {
        var ctx = new DungeonBuildContext(World);

        //use some strategies or create your own from the building blocks
        var strategy = new DungeonGrounds();
        strategy.Build(ctx);
        

        //dont change things below
        WorldFeatures = ctx.Features;
        WorldFeatures.Add(GameObjects.Quitting);
        World[1, 1].TryAddHero(ref Player, 1, 1);
    }
}