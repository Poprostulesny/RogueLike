using OODProject.Objects;

namespace OODProject.Core;

public interface IDungeonStrategy
{
    void Build(DungeonBuildContext ctx);
}

public sealed class DungeonBuildContext(
    Field[,] world,
    List<Func<IItem>>? items = null,
    List<Func<IInventoryItem>>? weapons = null,
    int itemAmount = 0,
    int weaponAmount = 0)
{
    public int ItemAmount = itemAmount;
    public List<Func<IItem>>? Items = items;
    public int WeaponAmount = weaponAmount;
    public List<Func<IInventoryItem>>? Weapons = weapons;
    public Field[,] World = world;

    public SortedSet<GameObjects> Features { get; } = new();

    public void AddFeature(GameObjects feature)
    {
        Features.Add(feature);
    }
}

public static class WorldGeneratorUtils
{
    public static Direction GetRandomDirection(Random rand)
    {
        var rnd = rand.Next(0, 4);
        return (Direction)rnd;
    }

    public static void NewXy(Direction dir, ref int x, ref int y)
    {
        switch (dir)
        {
            case Direction.Up:
                y = y - 1;
                break;
            case Direction.Down:
                y = y + 1;
                break;
            case Direction.Left:
                x = x - 1;
                break;
            case Direction.Right:
                x = x + 1;
                break;
        }
    }

    public static void EnsureConnectedness(Field[,] world)
    {
        var clrList = new List<List<(int, int)>>();
        world[1, 1] = new EmptyField();
        int[,] visited;
        while (true)
        {
            visited = new int[world.GetLength(0), world.GetLength(1)];
            var enterableFields = new List<(int, int)>();
            
            for (var y = 0; y < world.GetLength(0); y++)
            for (var x = 0; x < world.GetLength(1); x++)
                if (world[y, x].CanBeEntered)
                {
                   
                    enterableFields.Add((y, x));
                }
                else
                {
                    visited[y, x] = int.MaxValue;
                }


            clrList.Clear();
            var clrCnt = 0;
            foreach (var (y, x) in enterableFields)
                if (visited[y, x] == 0)
                {
                    clrCnt++;
                    clrList.Add(new List<(int, int)>());
                    Dfs(clrCnt, y, x, clrList[clrCnt - 1]);
                }

            if (clrCnt == 1) return;

            FindClosestAndConnect(1, 2);
        }


        void FindClosestAndConnect(int clra, int clrb)
        {
            var mindist = int.MaxValue;
            (int ya, int xa, int yb, int xb) minpair = (0, 0, 0, 0);
            foreach (var (ya, xa) in clrList[clra - 1])
            foreach (var (yb, xb) in clrList[clrb - 1])
            {
                var dist = Math.Abs(yb - ya) + Math.Abs(xb - xa);
                if (dist < mindist)
                {
                    mindist = dist;
                    minpair = (ya, xa, yb, xb);
                }
            }

            AddPath(minpair.xa, minpair.ya, minpair.xb, minpair.yb, Random.Shared.Next(1, 3), world);
        }

        void Dfs(int clr, int y, int x, List<(int, int)> clrList)
        {
            visited[y, x] = clr;
            clrList.Add((y, x));
            for (var i = 0; i < 4; i++)
            {
                int nx = x, ny = y;
                NewXy((Direction)i, ref nx, ref ny);
                if (visited[ny, nx] == 0) Dfs(clr, ny, nx, clrList);
            }
        }
    }

    public static void AddPath(int startX, int startY, int endX, int endY, int shape, Field[,] world)
    {
        var x = startX;
        var y = startY;
        var yIncrement = endY > startY ? 1 : -1;
        var xIncrement = endX > startX ? 1 : -1;
        if (shape == 1)
        {
            while (x != endX)
            {
                world[y, x] = new EmptyField();
                x += xIncrement;
            }

            while (y != endY)
            {
                world[y, x] = new EmptyField();
                y += yIncrement;
            }
        }
        else
        {
            while (y != endY)
            {
                world[y, x] = new EmptyField();
                y += yIncrement;
            }

            while (x != endX)
            {
                world[y, x] = new EmptyField();
                x += xIncrement;
            }
        }
    }

    public static void CloseBorders(Field[,] world)
    {
        var height = world.GetLength(0);
        var width = world.GetLength(1);
        for (var y = 0; y < height; y++)
        {
            world[y, 0] = new NonEnterableField();
            world[y, width - 1] = new NonEnterableField();
        }

        for (var x = 0; x < width; x++)
        {
            world[0, x] = new NonEnterableField();
            world[height - 1, x] = new NonEnterableField();
        }
    }
}

public sealed class DungeonBuilder
{
    private readonly DungeonBuildContext _context;

    private DungeonBuilder(DungeonBuildContext context)
    {
        _context = context;
    }

    public static DungeonBuilder CreateFilledDungeon(DungeonBuildContext ctx)
    {
        var world = ctx.World;
        for (var y = 0; y < world.GetLength(0); y++)
        for (var x = 0; x < world.GetLength(1); x++)
            world[y, x] = new NonEnterableField();

        return new DungeonBuilder(ctx);
    }

    public static DungeonBuilder CreateEmptyDungeon(DungeonBuildContext ctx)
    {
        var world = ctx.World;
        for (var y = 0; y < world.GetLength(0); y++)
        for (var x = 0; x < world.GetLength(1); x++)
            world[y, x] = new EmptyField();

        WorldGeneratorUtils.CloseBorders(world);
        ctx.AddFeature(GameObjects.Movement);
        return new DungeonBuilder(ctx);
    }

    public DungeonBuilder DrunkardsWalk()
    {
        var ctx = _context;
        var world = ctx.World;
        ctx.AddFeature(GameObjects.Movement);
        var curx = 1;
        var cury = 1;
        world[1, 1] = new EmptyField();
        var rand = new Random(DateTime.Now.Millisecond);


        for (var i = 0; i < 20 * 40 * 3; i++)
        {
            int newX;
            int newY;
            do
            {
                newX = curx;
                newY = cury;
                var dir = WorldGeneratorUtils.GetRandomDirection(rand);
                WorldGeneratorUtils.NewXy(dir, ref newX, ref newY);
            } while (newX < 1 || newX > 20 || newY < 1 || newY > 40);

            world[newY, newX] = new EmptyField();
            curx = newX;
            cury = newY;
        }

        return this;
    }

    /*
        code from https://github.com/munificent/hauberk/blob/db360d9efa714efb6d937c31953ef849c7394a39/lib/src/content/dungeon.dart
        translated into c#
        */

    public DungeonBuilder MazeWithRooms()
    {
        var ctx = _context;
        var world = ctx.World;
        ctx.AddFeature(GameObjects.Movement);
        var height = world.GetLength(0);
        var width = world.GetLength(1);

        var rand = new Random(DateTime.Now.Millisecond);


        var open = new bool[height, width];
        var regions = new int[height, width];
        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
            regions[y, x] = -1;

        var currentRegion = -1;
        var rooms = new List<(int x, int y, int w, int h)>();

        var numRoomTries = 50;
        var roomExtraSize = 0;
        var extraConnectorChance = 20;
        var windingPercent = 90;

        void StartRegion()
        {
            currentRegion++;
        }

        void Carve(int x, int y)
        {
            open[y, x] = true;
            regions[y, x] = currentRegion;
        }

        bool InInnerBounds(int x, int y)
        {
            return x >= 1 && x <= width - 2 && y >= 1 && y <= height - 2;
        }

        bool CanCarve(int x, int y, int dx, int dy)
        {
            if (!InInnerBounds(x + dx * 3, y + dy * 3)) return false;
            return !open[y + dy * 2, x + dx * 2];
        }

        void GrowMaze(int startX, int startY)
        {
            var cells = new List<(int x, int y)>();
            (int dx, int dy)? lastDir = null;

            StartRegion();
            Carve(startX, startY);
            cells.Add((startX, startY));

            while (cells.Count > 0)
            {
                var cell = cells[cells.Count - 1];
                var unmade = new List<(int dx, int dy)>();

                foreach (var dir in new (int dx, int dy)[] { (0, -1), (0, 1), (-1, 0), (1, 0) })
                    if (CanCarve(cell.x, cell.y, dir.dx, dir.dy))
                        unmade.Add(dir);

                if (unmade.Count > 0)
                {
                    (int dx, int dy) dir;
                    if (lastDir.HasValue && unmade.Contains(lastDir.Value) && rand.Next(0, 100) > windingPercent)
                        dir = lastDir.Value;
                    else
                        dir = unmade[rand.Next(0, unmade.Count)];

                    Carve(cell.x + dir.dx, cell.y + dir.dy);
                    Carve(cell.x + dir.dx * 2, cell.y + dir.dy * 2);

                    cells.Add((cell.x + dir.dx * 2, cell.y + dir.dy * 2));
                    lastDir = dir;
                }
                else
                {
                    cells.RemoveAt(cells.Count - 1);
                    lastDir = null;
                }
            }
        }

        void AddRooms()
        {
            rooms.Add((1, 1, 3, 3));
            StartRegion();
            for (var ry = 1; ry < 1 + 3; ry++)
            for (var rx = 1; rx < 1 + 3; rx++)
                Carve(rx, ry);

            for (var i = 0; i < numRoomTries; i++)
            {
                var size = rand.Next(1, 3 + roomExtraSize + 1) * 2 + 1;
                var rectangularity = rand.Next(0, 1 + size / 2 + 1) * 2;
                var roomW = size;
                var roomH = size;
                if (rand.Next(0, 2) == 0)
                    roomW += rectangularity;
                else
                    roomH += rectangularity;

                var maxX = (width - roomW - 1) / 2;
                var maxY = (height - roomH - 1) / 2;
                if (maxX <= 0 || maxY <= 0) continue;

                var x = rand.Next(0, maxX) * 2 + 1;
                var y = rand.Next(0, maxY) * 2 + 1;

                var overlaps = false;
                foreach (var other in rooms)
                {
                    var ax1 = x - 1;
                    var ay1 = y - 1;
                    var ax2 = x + roomW;
                    var ay2 = y + roomH;

                    var bx1 = other.x - 1;
                    var by1 = other.y - 1;
                    var bx2 = other.x + other.w;
                    var by2 = other.y + other.h;

                    if (ax1 <= bx2 && ax2 >= bx1 && ay1 <= by2 && ay2 >= by1)
                    {
                        overlaps = true;
                        break;
                    }
                }

                if (overlaps) continue;

                rooms.Add((x, y, roomW, roomH));

                StartRegion();
                for (var ry = y; ry < y + roomH; ry++)
                for (var rx = x; rx < x + roomW; rx++)
                    Carve(rx, ry);
            }
        }

        void ConnectRegions()
        {
            var connectorRegions = new Dictionary<(int x, int y), HashSet<int>>();
            for (var y = 1; y <= height - 2; y++)
            for (var x = 1; x <= width - 2; x++)
            {
                if (open[y, x]) continue;
                var regionsHere = new HashSet<int>();
                foreach (var dir in new (int dx, int dy)[] { (0, -1), (0, 1), (-1, 0), (1, 0) })
                {
                    var r = regions[y + dir.dy, x + dir.dx];
                    if (r != -1) regionsHere.Add(r);
                }

                if (regionsHere.Count < 2) continue;
                connectorRegions[(x, y)] = regionsHere;
            }

            var connectors = connectorRegions.Keys.ToList();
            var merged = new Dictionary<int, int>();
            var openRegions = new HashSet<int>();
            for (var i = 0; i <= currentRegion; i++)
            {
                merged[i] = i;
                openRegions.Add(i);
            }

            while (openRegions.Count > 1 && connectors.Count > 0)
            {
                var connector = connectors[rand.Next(0, connectors.Count)];
                open[connector.y, connector.x] = true;

                var regionsHere = connectorRegions[connector].Select(r => merged[r]).ToList();
                var dest = regionsHere[0];
                var sources = regionsHere.Skip(1).ToList();

                for (var i = 0; i <= currentRegion; i++)
                    if (sources.Contains(merged[i]))
                        merged[i] = dest;

                openRegions.RemoveWhere(r => sources.Contains(r));

                connectors.RemoveAll(pos =>
                {
                    if (Math.Abs(connector.x - pos.x) + Math.Abs(connector.y - pos.y) < 2) return true;

                    var mapped = connectorRegions[pos].Select(r => merged[r]).ToHashSet();
                    if (mapped.Count > 1) return false;

                    if (rand.Next(0, extraConnectorChance) == 0) open[pos.y, pos.x] = true;

                    return true;
                });
            }
        }

        void RemoveDeadEnds()
        {
            var done = false;
            while (!done)
            {
                done = true;
                for (var y = 1; y <= height - 2; y++)
                for (var x = 1; x <= width - 2; x++)
                {
                    if (!open[y, x]) continue;
                    if (x >= 1 && x <= 3 && y >= 1 && y <= 3) continue; // keep the start room open

                    var exits = 0;
                    foreach (var dir in new (int dx, int dy)[] { (0, -1), (0, 1), (-1, 0), (1, 0) })
                        if (open[y + dir.dy, x + dir.dx])
                            exits++;

                    if (exits != 1) continue;

                    done = false;
                    open[y, x] = false;
                }
            }
        }


        bool[,] FloodFromStart()
        {
            var visited = new bool[height, width];
            var queue = new Queue<(int x, int y)>();
            queue.Enqueue((1, 1));
            visited[1, 1] = true;

            while (queue.Count > 0)
            {
                var (cx, cy) = queue.Dequeue();
                var dirs = new (int dx, int dy)[] { (0, -1), (0, 1), (-1, 0), (1, 0) };
                foreach (var dir in dirs)
                {
                    var nx = cx + dir.dx;
                    var ny = cy + dir.dy;
                    if (nx < 1 || nx > width - 2 || ny < 1 || ny > height - 2) continue;

                    if (open[ny, nx] && !visited[ny, nx])
                    {
                        visited[ny, nx] = true;
                        queue.Enqueue((nx, ny));
                    }
                }
            }

            return visited;
        }


        void ConnectAllComponentsToStart()
        {
            open[1, 1] = true;

            int CountOpen()
            {
                var cnt = 0;
                for (var y = 1; y <= height - 2; y++)
                for (var x = 1; x <= width - 2; x++)
                    if (open[y, x])
                        cnt++;

                return cnt;
            }

            var openCount = CountOpen();
            if (openCount <= 1) return;
            //while we can not reach some components
            //connectivity of other components should be ensured by the algorithm but better safe than sorry
            while (true)
            {
                var visited = FloodFromStart();
                var visitedCells = new List<(int x, int y)>();
                for (var y = 1; y <= height - 2; y++)
                for (var x = 1; x <= width - 2; x++)
                    if (visited[y, x])
                        visitedCells.Add((x, y));

                var connectedCount = visitedCells.Count;
                if (connectedCount == openCount) break;

                var bestDist = int.MaxValue;
                (int x, int y) target = (1, 1);
                (int x, int y) anchor = (1, 1);

                for (var ty = 1; ty <= height - 2; ty++)
                for (var tx = 1; tx <= width - 2; tx++)
                {
                    if (!open[ty, tx] || visited[ty, tx]) continue;
                    //searching for the closest unvisited (target) cell from the visited cells (anchor)
                    foreach (var (vx, vy) in visitedCells)
                    {
                        var dist = Math.Abs(tx - vx) + Math.Abs(ty - vy);
                        if (dist < bestDist)
                        {
                            bestDist = dist;
                            target = (tx, ty);
                            anchor = (vx, vy);
                        }
                    }
                }

                //current coords
                var wx = anchor.x;
                var wy = anchor.y;
                var safety = width * height * 10;
                while ((wx != target.x || wy != target.y) && safety-- > 0)
                {
                    open[wy, wx] = true;
                    //randomly choosing shape of the tunnel
                    var moveHorizFirst = rand.Next(0, 2) == 0;
                    if (moveHorizFirst && wx != target.x)
                        wx += Math.Sign(target.x - wx);
                    else if (wy != target.y)
                        wy += Math.Sign(target.y - wy);
                    else if (wx != target.x) wx += Math.Sign(target.x - wx);
                    //ensuring we stay within the boundsy
                    if (wx < 1) wx = 1;
                    if (wx > width - 2) wx = width - 2;
                    if (wy < 1) wy = 1;
                    if (wy > height - 2) wy = height - 2;

                    open[wy, wx] = true;
                }
            }
        }

        AddRooms();

        for (var y = 1; y <= height - 2; y += 2)
        for (var x = 1; x <= width - 2; x += 2)
        {
            if (open[y, x]) continue;
            GrowMaze(x, y);
        }

        ConnectRegions();
        RemoveDeadEnds();

        // Ensure the start room stays open.
        for (var ry = 1; ry <= 3; ry++)
        for (var rx = 1; rx <= 3; rx++)
            open[ry, rx] = true;

        ConnectAllComponentsToStart();


        for (var y = 1; y < height - 1; y++)
        for (var x = 1; x < width - 1; x++)
            if (open[y, x])
                world[y, x] = new EmptyField();

        WorldGeneratorUtils.EnsureConnectedness(world);
        return this;
    }

    public DungeonBuilder AddCentralRoom(int w, int h)
    {
        var ctx = _context;
        var world = ctx.World;
        var midy = world.GetLength(0) / 2;
        var midx = world.GetLength(1) / 2;
        var startx = Math.Max(0, midx - w / 2);
        var starty = Math.Max(0, midy - h / 2);
        for (var y = starty; y < Math.Min(starty + h, world.GetLength(0) - 1); y++)
        for (var x = startx; x < Math.Min(startx + w, world.GetLength(1) - 1); x++)
            world[y, x] = new EmptyField();

        WorldGeneratorUtils.EnsureConnectedness(world);
        ctx.AddFeature(GameObjects.Movement);
        return this;
    }

    public DungeonBuilder AddWeapons(List<Func<IInventoryItem>> weapons)
    {
        var ctx = _context;
        var world = ctx.World;


        var emptyPoints = new List<(int, int)>();
        for (var y = 1; y < world.GetLength(0) - 1; y++)
        for (var x = 1; x < world.GetLength(1) - 1; x++)
            if (world[y, x].CanBeEntered)
                emptyPoints.Add((y, x));


        if (emptyPoints.Count == 0 || weapons.Count == 0) return this;
        var random = new Random(DateTime.Now.Microsecond);
        for (var i = 0; i < ctx.WeaponAmount; i++)
        {
            var point = random.Next(0, emptyPoints.Count);
            var weapon = random.Next(0, weapons.Count);
            var (y, x) = emptyPoints[point];
            world[y, x].TryAddItem(weapons[weapon]());
        }

        ctx.AddFeature(GameObjects.Item);
        return this;
    }

    public DungeonBuilder AddItems(List<Func<IItem>> items)
    {
        var ctx = _context;
        var world = ctx.World;

        if (items.Count == 0) return this;
        var emptyPoints = new List<(int, int)>();
        for (var y = 1; y < world.GetLength(0) - 1; y++)
        for (var x = 1; x < world.GetLength(1) - 1; x++)
            if (world[y, x].CanBeEntered)
                emptyPoints.Add((y, x));

        if (emptyPoints.Count == 0) return this;
        var random = new Random(DateTime.Now.Microsecond);
        for (var i = 0; i < ctx.ItemAmount; i++)
        {
            var point = random.Next(0, emptyPoints.Count);
            var item = random.Next(0, items.Count);
            var (y, x) = emptyPoints[point];
            world[y, x].TryAddItem(items[item]());
        }

        ctx.AddFeature(GameObjects.Item);
        return this;
    }

    public DungeonBuilder AddChambers(int amount)
    {
        var ctx = _context;
        var world = ctx.World;
        var chambers = new List<Square>();
        var rand = new Random(DateTime.Now.Microsecond);

        var retry = 0;
        while (true)
        {
            if (chambers.Count >= amount || retry >= 5) break;

            var w = rand.Next(3, 6);
            var h = rand.Next(3, 6);
            var x = rand.Next(1, world.GetLength(1) - w - 1);
            var y = rand.Next(1, world.GetLength(0) - h - 1);
            var newChamber = new Square(x, y, x + w, y + h);
            if (!Intersects(newChamber))
            {
                chambers.Add(newChamber);
                retry = 0;
            }
            else
            {
                retry++;
            }
        }

        foreach (var chamber in chambers)
            for (var i = chamber.Y1; i < chamber.Y2; i++)
            for (var j = chamber.X1; j < chamber.X2; j++)
                world[i, j] = new EmptyField();

        WorldGeneratorUtils.EnsureConnectedness(world);
        ctx.AddFeature(GameObjects.Movement);

        return this;

        bool Intersects(Square square)
        {
            foreach (var chamber in chambers)
            {
                var ax1 = square.X1 - 1;
                var ay1 = square.Y1 - 1;
                var ax2 = square.X2;
                var ay2 = square.Y2;

                var bx1 = chamber.X1 - 1;
                var by1 = chamber.Y1 - 1;
                var bx2 = chamber.X2;
                var by2 = chamber.Y2;

                if (ax1 <= bx2 && ax2 >= bx1 && ay1 <= by2 && ay2 >= by1)
                    return true;
            }

            return false;
        }
    }

    private struct Square(int x1, int y1, int x2, int y2)
    {
        public readonly int X1 = x1;
        public readonly int Y1 = y1;
        public readonly int X2 = x2;
        public readonly int Y2 = y2;
    }

    public DungeonBuilder AddPaths(int amount)
    {
        var ctx = _context;
        var world = ctx.World;
        var height = world.GetLength(0);
        var width = world.GetLength(1);
        var random = new Random(DateTime.Now.Microsecond);

        var pathCount = amount;
        for (var i = 0; i < pathCount; i++)
        {
            var sX = random.Next(1, width - 2);
            var sY = random.Next(1, height - 2);
            var eX = random.Next(1, width - 1);
            var eY = random.Next(1, height - 1);
            var shape = random.Next(1, 3);
            WorldGeneratorUtils.AddPath(sX, sY, eX, eY, shape, world);
        }

        WorldGeneratorUtils.EnsureConnectedness(world);
        ctx.AddFeature(GameObjects.Movement);
        return this;
    }
}

internal static class DungeonStrategyDefaults
{
    public static List<Func<IItem>> CreateItems()
    {
        return
        [
            () => new Gold(10),
            () => new Coins(10),
            () => new Broomstick(),
            () => new Teapot(),
            () => new BrokenSword()
        ];
    }

    public static List<Func<IInventoryItem>> CreateWeapons()
    {
        return
        [
            () => new DragonSlayerSword(),
            () => new RustySword(),
            () => new Shield()
        ];
    }
}

public sealed class DungeonGrounds : IDungeonStrategy
{
    public void Build(DungeonBuildContext ctx)
    {
        ctx.ItemAmount = 15;
        ctx.WeaponAmount = 10;
        DungeonBuilder.CreateFilledDungeon(ctx)
            .AddChambers(5)
            .AddPaths(10)
            .AddCentralRoom(7, 10)
            .AddItems(DungeonStrategyDefaults.CreateItems())
            .AddWeapons(DungeonStrategyDefaults.CreateWeapons());
    }
}

public sealed class EmptyDungeonStrategy : IDungeonStrategy
{
    public void Build(DungeonBuildContext ctx)
    {
        ctx.ItemAmount = 15;
        ctx.WeaponAmount = 10;
        DungeonBuilder.CreateEmptyDungeon(ctx)
            .AddItems(DungeonStrategyDefaults.CreateItems())
            .AddWeapons(DungeonStrategyDefaults.CreateWeapons());
    }
}

public sealed class ExtraFunDungeon : IDungeonStrategy
{
    public void Build(DungeonBuildContext ctx)
    {
        ctx.ItemAmount = 15;
        ctx.WeaponAmount = 10;
        DungeonBuilder.CreateFilledDungeon(ctx)
            .MazeWithRooms()
            .AddItems(DungeonStrategyDefaults.CreateItems())
            .AddWeapons(DungeonStrategyDefaults.CreateWeapons());
    }
}

public sealed class DrunkenlyDrawnDungeon : IDungeonStrategy
{
    public void Build(DungeonBuildContext ctx)
    {
        ctx.ItemAmount = 15;
        ctx.WeaponAmount = 10;
        DungeonBuilder.CreateFilledDungeon(ctx)
            .DrunkardsWalk()
            .AddPaths(10)
            .AddItems(DungeonStrategyDefaults.CreateItems())
            .AddWeapons(DungeonStrategyDefaults.CreateWeapons());
    }
}
