namespace OODProject;

public interface IDungeonBuildStep
{
    void Apply(DungeonBuildContext ctx);
}

public interface IDungeonBaseBuildStep : IDungeonBuildStep
{
}

public interface IDungeonStrategy
{
    void Build(DungeonBuildContext ctx);
}

public sealed class DungeonBuildContext(
    Field[,] world,
    List<Func<IItem>>? items = null,
    List<Func<IInventoryItem>>? weapons = null,
    int _item_amount = 0,
    int _weapon_amount = 0)
{
    public int item_amount = _item_amount;
    public List<Func<IItem>>? Items = items;
    public int weapon_amount = _weapon_amount;
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

    public static void NewXY(Direction dir, ref int x, ref int y)
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

    public static void EnsureConnectedness(Field[,] World)
    {
        var clr_list = new List<List<(int, int)>>();
        World[1, 1] = new EmptyField();
        int[,] visited;
        while (true)
        {
            visited = new int[World.GetLength(0), World.GetLength(1)];
            var EnterableFields = new List<(int, int)>();
            var empty_cnt = 0;
            for (var y = 0; y < World.GetLength(0); y++)
            for (var x = 0; x < World.GetLength(1); x++)
                if (World[y, x].CanBeEntered)
                {
                    empty_cnt++;
                    EnterableFields.Add((y, x));
                }
                else
                {
                    visited[y, x] = int.MaxValue;
                }


            clr_list.Clear();
            var clr_cnt = 0;
            foreach (var (y, x) in EnterableFields)
                if (visited[y, x] == 0)
                {
                    clr_cnt++;
                    clr_list.Add(new List<(int, int)>());
                    dfs(clr_cnt, y, x, clr_list[clr_cnt - 1]);
                }

            if (clr_cnt == 1) return;

            find_closest_and_connect(1, 2);
        }


        void find_closest_and_connect(int clra, int clrb)
        {
            var mindist = int.MaxValue;
            (int ya, int xa, int yb, int xb) minpair = (0, 0, 0, 0);
            foreach (var (ya, xa) in clr_list[clra - 1])
            foreach (var (yb, xb) in clr_list[clrb - 1])
            {
                var dist = Math.Abs(yb - ya) + Math.Abs(xb - xa);
                if (dist < mindist)
                {
                    mindist = dist;
                    minpair = (ya, xa, yb, xb);
                }
            }

            AddPath(minpair.xa, minpair.ya, minpair.xb, minpair.yb, Random.Shared.Next(1, 3), World);
        }

        void dfs(int clr, int y, int x, List<(int, int)> clr_list)
        {
            visited[y, x] = clr;
            clr_list.Add((y, x));
            for (var i = 0; i < 4; i++)
            {
                int nx = x, ny = y;
                NewXY((Direction)i, ref nx, ref ny);
                if (visited[ny, nx] == 0) dfs(clr, ny, nx, clr_list);
            }
        }
    }

    public static void AddPath(int startX, int startY, int endX, int endY, int shape, Field[,] World)
    {
        var x = startX;
        var y = startY;
        var y_increment = endY > startY ? 1 : -1;
        var x_increment = endX > startX ? 1 : -1;
        if (shape == 1)
        {
            while (x != endX)
            {
                World[y, x] = new EmptyField();
                x += x_increment;
            }

            while (y != endY)
            {
                World[y, x] = new EmptyField();
                y += y_increment;
            }
        }
        else
        {
            while (y != endY)
            {
                World[y, x] = new EmptyField();
                y += y_increment;
            }

            while (x != endX)
            {
                World[y, x] = new EmptyField();
                x += x_increment;
            }
        }
    }

    public static void CloseBorders(Field[,] World)
    {
        var height = World.GetLength(0);
        var width = World.GetLength(1);
        for (var y = 0; y < height; y++)
        {
            World[y, 0] = new NonEnterableField();
            World[y, width - 1] = new NonEnterableField();
        }

        for (var x = 0; x < width; x++)
        {
            World[0, x] = new NonEnterableField();
            World[height - 1, x] = new NonEnterableField();
        }
    }
}

public sealed class DrunkardsWalk : IDungeonBuildStep
{
    public void Apply(DungeonBuildContext ctx)
    {
        var World = ctx.World;
        ctx.AddFeature(GameObjects.Movement);
        var curx = 1;
        var cury = 1;
        World[1, 1] = new EmptyField();
        var rand = new Random(DateTime.Now.Millisecond);


        for (var i = 0; i < 20 * 40 * 3; i++)
        {
            var new_x = curx;
            var new_y = cury;
            do
            {
                new_x = curx;
                new_y = cury;
                var dir = WorldGeneratorUtils.GetRandomDirection(rand);
                WorldGeneratorUtils.NewXY(dir, ref new_x, ref new_y);
            } while (new_x < 1 || new_x > 20 || new_y < 1 || new_y > 40);

            World[new_y, new_x] = new EmptyField();
            curx = new_x;
            cury = new_y;
        }
    }
}

public sealed class MazeWithRooms : IDungeonBuildStep
{
    /*
        code from https://github.com/munificent/hauberk/blob/db360d9efa714efb6d937c31953ef849c7394a39/lib/src/content/dungeon.dart
        translated into c#
        i dont rly understand whats going on but it works
        my additions are forcing room of size 3 at 1,1 and ensuring it is connected
        */

    public void Apply(DungeonBuildContext ctx)
    {
        var World = ctx.World;
        ctx.AddFeature(GameObjects.Movement);
        var height = World.GetLength(0);
        var width = World.GetLength(1);

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
                    if (x >= 1 && x <= 3 && y >= 1 && y <= 3) continue; // keep start room open

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
                World[y, x] = new EmptyField();

        WorldGeneratorUtils.EnsureConnectedness(World);
    }
}

public sealed class AddCentralRoom(int _w, int _h) : IDungeonBuildStep
{
    private readonly int h = _h;
    private readonly int w = _w;

    public void Apply(DungeonBuildContext ctx)
    {
        var World = ctx.World;
        var midy = World.GetLength(0) / 2;
        var midx = World.GetLength(1) / 2;
        var startx = Math.Max(0, midx - w / 2);
        var starty = Math.Max(0, midy - h / 2);
        for (var y = starty; y < Math.Min(starty + h, World.GetLength(0) - 1); y++)
        for (var x = startx; x < Math.Min(startx + w, World.GetLength(1) - 1); x++)
            World[y, x] = new EmptyField();

        WorldGeneratorUtils.EnsureConnectedness(World);
        ctx.AddFeature(GameObjects.Movement);
    }
}

public sealed class AddWeapons(List<Func<IInventoryItem>> _weapons) : IDungeonBuildStep
{
    private readonly List<Func<IInventoryItem>> Weapons = _weapons;

    public void Apply(DungeonBuildContext ctx)
    {
        var World = ctx.World;


        var EmptyPoints = new List<(int, int)>();
        for (var y = 1; y < World.GetLength(0) - 1; y++)
        for (var x = 1; x < World.GetLength(1) - 1; x++)
            if (World[y, x].CanBeEntered)
                EmptyPoints.Add((y, x));


        if (EmptyPoints.Count == 0 || Weapons.Count == 0) return;
        var random = new Random(DateTime.Now.Microsecond);
        for (var i = 0; i < ctx.weapon_amount; i++)
        {
            var point = random.Next(0, EmptyPoints.Count);
            var weapon = random.Next(0, Weapons.Count);
            var (y, x) = EmptyPoints[point];
            World[y, x].TryAddItem(Weapons[weapon]());
        }

        ctx.AddFeature(GameObjects.Item);
    }
}

public sealed class AddItems(List<Func<IItem>> _items) : IDungeonBuildStep
{
    private readonly List<Func<IItem>> items = _items;

    public void Apply(DungeonBuildContext ctx)
    {
        var World = ctx.World;

        if (items.Count == 0) return;
        var EmptyPoints = new List<(int, int)>();
        for (var y = 1; y < World.GetLength(0) - 1; y++)
        for (var x = 1; x < World.GetLength(1) - 1; x++)
            if (World[y, x].CanBeEntered)
                EmptyPoints.Add((y, x));

        if (EmptyPoints.Count == 0) return;
        var random = new Random(DateTime.Now.Microsecond);
        for (var i = 0; i < ctx.item_amount; i++)
        {
            var point = random.Next(0, EmptyPoints.Count);
            var item = random.Next(0, items.Count);
            var (y, x) = EmptyPoints[point];
            World[y, x].TryAddItem(items[item]());
        }

        ctx.AddFeature(GameObjects.Item);
    }
}

public sealed class FilledDungeon : IDungeonBaseBuildStep
{
    public void Apply(DungeonBuildContext ctx)
    {
        var World = ctx.World;
        for (var y = 0; y < World.GetLength(0); y++)
        for (var x = 0; x < World.GetLength(1); x++)
            World[y, x] = new NonEnterableField();
    }
}

public sealed class EmptyDungeon : IDungeonBaseBuildStep
{
    public void Apply(DungeonBuildContext ctx)
    {
        var World = ctx.World;
        for (var y = 0; y < World.GetLength(0); y++)
        for (var x = 0; x < World.GetLength(1); x++)
            World[y, x] = new EmptyField();

        WorldGeneratorUtils.CloseBorders(World);
        ctx.AddFeature(GameObjects.Movement);
    }
}

public sealed class AddChambers(int _amount) : IDungeonBuildStep
{
    private readonly int amount = _amount;

    public void Apply(DungeonBuildContext ctx)
    {
        var World = ctx.World;
        var chambers = new List<Square>();
        var rand = new Random(DateTime.Now.Microsecond);

        var retry = 0;
        while (true)
        {
            if (chambers.Count >= amount || retry >= 5) break;

            var w = rand.Next(3, 6);
            var h = rand.Next(3, 6);
            var x = rand.Next(1, World.GetLength(1) - w - 1);
            var y = rand.Next(1, World.GetLength(0) - h - 1);
            var new_chamber = new Square(x, y, x + w, y + h);
            if (!intersects(new_chamber))
            {
                chambers.Add(new_chamber);
                retry = 0;
            }
            else
            {
                retry++;
            }
        }

        foreach (var chamber in chambers)
            for (var i = chamber.y1; i < chamber.y2; i++)
            for (var j = chamber.x1; j < chamber.x2; j++)
                World[i, j] = new EmptyField();

        WorldGeneratorUtils.EnsureConnectedness(World);
        ctx.AddFeature(GameObjects.Movement);

        return;

        bool intersects(Square square)
        {
            foreach (var chamber in chambers)
            {
                var ax1 = square.x1 - 1;
                var ay1 = square.y1 - 1;
                var ax2 = square.x2;
                var ay2 = square.y2;

                var bx1 = chamber.x1 - 1;
                var by1 = chamber.y1 - 1;
                var bx2 = chamber.x2;
                var by2 = chamber.y2;

                if (ax1 <= bx2 && ax2 >= bx1 && ay1 <= by2 && ay2 >= by1)
                    return true;
            }

            return false;
        }
    }

    private struct Square(int _x1, int _y1, int _x2, int _y2)
    {
        public readonly int x1 = _x1;
        public readonly int y1 = _y1;
        public readonly int x2 = _x2;
        public readonly int y2 = _y2;
    }
}

public sealed class AddPaths(int _amount) : IDungeonBuildStep
{
    private readonly int amount = _amount;

    public void Apply(DungeonBuildContext ctx)
    {
        var World = ctx.World;
        var height = World.GetLength(0);
        var width = World.GetLength(1);
        var random = new Random(DateTime.Now.Microsecond);

        var PathCount = amount;
        for (var i = 0; i < PathCount; i++)
        {
            var s_x = random.Next(1, width - 2);
            var s_y = random.Next(1, height - 2);
            var e_x = random.Next(1, width - 1);
            var e_y = random.Next(1, height - 1);
            var shape = random.Next(1, 3);
            WorldGeneratorUtils.AddPath(s_x, s_y, e_x, e_y, shape, World);
        }

        WorldGeneratorUtils.EnsureConnectedness(World);
        ctx.AddFeature(GameObjects.Movement);
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
        var strategy = new StepSequenceStrategy(
            new FilledDungeon(),
            new AddChambers(5),
            new AddPaths(10),
            new AddCentralRoom(7, 10),
            new AddItems(DungeonStrategyDefaults.CreateItems()),
            new AddWeapons(DungeonStrategyDefaults.CreateWeapons()));

        ctx.item_amount = 15;
        ctx.weapon_amount = 10;
        strategy.Build(ctx);
    }
}

public sealed class EmptyDungeonStrategy : IDungeonStrategy
{
    public void Build(DungeonBuildContext ctx)
    {
        var strategy = new StepSequenceStrategy(
            new EmptyDungeon(),
            new AddItems(DungeonStrategyDefaults.CreateItems()),
            new AddWeapons(DungeonStrategyDefaults.CreateWeapons()));

        ctx.item_amount = 15;
        ctx.weapon_amount = 10;
        strategy.Build(ctx);
    }
}

public sealed class ExtraFunDungeon : IDungeonStrategy
{
    public void Build(DungeonBuildContext ctx)
    {
        var strategy = new StepSequenceStrategy(
            new FilledDungeon(),
            new MazeWithRooms(),
            new AddItems(DungeonStrategyDefaults.CreateItems()),
            new AddWeapons(DungeonStrategyDefaults.CreateWeapons()));

        ctx.item_amount = 15;
        ctx.weapon_amount = 10;
        strategy.Build(ctx);
    }
}

public sealed class DrunkenlyDrawnDungeon : IDungeonStrategy
{
    public void Build(DungeonBuildContext ctx)
    {
        var strategy = new StepSequenceStrategy(
            new FilledDungeon(),
            new DrunkardsWalk(),
            new AddPaths(10),
            new AddItems(DungeonStrategyDefaults.CreateItems()),
            new AddWeapons(DungeonStrategyDefaults.CreateWeapons()));

        ctx.item_amount = 15;
        ctx.weapon_amount = 10;
        strategy.Build(ctx);
    }
}

public sealed class StepSequenceStrategy : IDungeonStrategy
{
    private readonly IDungeonBaseBuildStep _base;
    private readonly IReadOnlyList<IDungeonBuildStep> _steps;

    public StepSequenceStrategy(IDungeonBaseBuildStep base_step, params IDungeonBuildStep[] steps)
    {
        _steps = steps;
        _base = base_step;
    }

    public void Build(DungeonBuildContext context)
    {
        _base.Apply(context);
        foreach (var step in _steps)
            step.Apply(context);
    }
}