
namespace OODProject;
public static class WorldGenerator
{   
    private static Direction GetRandomDirection(Random rand)
    {
        int rnd = rand.Next(0, 4);
        return (Direction)rnd;
    }

    private static void NewXY(Direction dir, ref int x,ref int y)
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
    /*
        code from https://github.com/munificent/hauberk/blob/db360d9efa714efb6d937c31953ef849c7394a39/lib/src/content/dungeon.dart
        translated into c#
        i dont rly understand whats going on but it works
        my additions are forcing room of size 3 at 1,1 and ensuring it is connected
        */ 
    public static void MazeWithRooms(ref Field[,] World)
    {
        int height = World.GetLength(0);
        int width = World.GetLength(1);

        var rand = new Random(System.DateTime.Now.Millisecond);

       
        var open = new bool[height, width];
        var regions = new int[height, width];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                regions[y, x] = -1;
            }
        }

        int currentRegion = -1;
        var rooms = new List<(int x, int y, int w, int h)>();

        int numRoomTries = 50;
        int roomExtraSize = 0;
        int extraConnectorChance = 20;
        int windingPercent = 90;

        void StartRegion() => currentRegion++;

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
                {
                    if (CanCarve(cell.x, cell.y, dir.dx, dir.dy)) unmade.Add(dir);
                }

                if (unmade.Count > 0)
                {
                    (int dx, int dy) dir;
                    if (lastDir.HasValue && unmade.Contains(lastDir.Value) && rand.Next(0, 100) > windingPercent)
                    {
                        dir = lastDir.Value;
                    }
                    else
                    {
                        dir = unmade[rand.Next(0, unmade.Count)];
                    }

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
            for (int ry = 1; ry < 1 + 3; ry++)
            {
                for (int rx = 1; rx < 1 + 3; rx++)
                {
                    Carve(rx, ry);
                }
            }
            
            for (int i = 0; i < numRoomTries; i++)
            {
                
                int size = rand.Next(1, 3 + roomExtraSize + 1) * 2 + 1;
                int rectangularity = rand.Next(0, 1 + size / 2 + 1) * 2;
                int roomW = size;
                int roomH = size;
                if (rand.Next(0, 2) == 0)
                {
                    roomW += rectangularity;
                }
                else
                {
                    roomH += rectangularity;
                }

                int maxX = (width - roomW - 1) / 2;
                int maxY = (height - roomH - 1) / 2;
                if (maxX <= 0 || maxY <= 0) continue;
        
                int x = rand.Next(0, maxX) * 2 + 1;
                int y = rand.Next(0, maxY) * 2 + 1;

                bool overlaps = false;
                foreach (var other in rooms)
                {
                    
                    int ax1 = x - 1;
                    int ay1 = y - 1;
                    int ax2 = x + roomW;
                    int ay2 = y + roomH;

                    int bx1 = other.x - 1;
                    int by1 = other.y - 1;
                    int bx2 = other.x + other.w;
                    int by2 = other.y + other.h;

                    if (ax1 <= bx2 && ax2 >= bx1 && ay1 <= by2 && ay2 >= by1)
                    {
                        overlaps = true;
                        break;
                    }
                }

                if (overlaps) continue;

                rooms.Add((x, y, roomW, roomH));
    
                StartRegion();
                for (int ry = y; ry < y + roomH; ry++)
                {
                    for (int rx = x; rx < x + roomW; rx++)
                    {
                        Carve(rx, ry);
                    }
                }
            }
        }

        void ConnectRegions()
        {
            var connectorRegions = new Dictionary<(int x, int y), HashSet<int>>();
            for (int y = 1; y <= height - 2; y++)
            {
                for (int x = 1; x <= width - 2; x++)
                {
                    if (open[y, x]) continue;
                    var regionsHere = new HashSet<int>();
                    foreach (var dir in new (int dx, int dy)[] { (0, -1), (0, 1), (-1, 0), (1, 0) })
                    {
                        int r = regions[y + dir.dy, x + dir.dx];
                        if (r != -1) regionsHere.Add(r);
                    }

                    if (regionsHere.Count < 2) continue;
                    connectorRegions[(x, y)] = regionsHere;
                }
            }

            var connectors = connectorRegions.Keys.ToList();
            var merged = new Dictionary<int, int>();
            var openRegions = new HashSet<int>();
            for (int i = 0; i <= currentRegion; i++)
            {
                merged[i] = i;
                openRegions.Add(i);
            }

            while (openRegions.Count > 1 && connectors.Count > 0)
            {
                var connector = connectors[rand.Next(0, connectors.Count)];
                open[connector.y, connector.x] = true;

                var regionsHere = connectorRegions[connector].Select(r => merged[r]).ToList();
                int dest = regionsHere[0];
                var sources = regionsHere.Skip(1).ToList();

                for (int i = 0; i <= currentRegion; i++)
                {
                    if (sources.Contains(merged[i])) merged[i] = dest;
                }

                openRegions.RemoveWhere(r => sources.Contains(r));

                connectors.RemoveAll(pos =>
                {
                    if (Math.Abs(connector.x - pos.x) + Math.Abs(connector.y - pos.y) < 2) return true;

                    var mapped = connectorRegions[pos].Select(r => merged[r]).ToHashSet();
                    if (mapped.Count > 1) return false;

                    if (rand.Next(0, extraConnectorChance) == 0)
                    {
                        open[pos.y, pos.x] = true;
                    }

                    return true;
                });
            }
        }

        void RemoveDeadEnds()
        {
            bool done = false;
            while (!done)
            {
                done = true;
                for (int y = 1; y <= height - 2; y++)
                {
                    for (int x = 1; x <= width - 2; x++)
                    {
                        if (!open[y, x]) continue;
                        if (x >= 1 && x <= 3 && y >= 1 && y <= 3) continue; // keep start room open

                        int exits = 0;
                        foreach (var dir in new (int dx, int dy)[] { (0, -1), (0, 1), (-1, 0), (1, 0) })
                        {
                            if (open[y + dir.dy, x + dir.dx]) exits++;
                        }

                        if (exits != 1) continue;

                        done = false;
                        open[y, x] = false;
                    }
                }
            }
        }

        void CloseBorders()
        {
            for (int y = 0; y < height; y++)
            {
                open[y, 0] = false;
                open[y, width - 1] = false;
            }

            for (int x = 0; x < width; x++)
            {
                open[0, x] = false;
                open[height - 1, x] = false;
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
                var dirs = new (int dx, int dy)[]{ (0, -1), (0, 1), (-1, 0), (1, 0) };
                foreach (var dir in dirs)
                {
                    int nx = cx + dir.dx;
                    int ny = cy + dir.dy;
                    if (nx < 1 || nx > width - 2 || ny < 1 || ny > height - 2)
                    {
                        continue;
                    }

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
                int cnt = 0;
                for (int y = 1; y <= height - 2; y++)
                {
                    for (int x = 1; x <= width - 2; x++)
                    {
                        if (open[y, x]) cnt++;
                    }
                }
                return cnt;
            }

            int openCount = CountOpen();
            if (openCount <= 1) return;
            //while we can not reach some components
            //connectivity of other components should be ensured by the algorithm but better safe than sorry
            while (true)
            {
                var visited = FloodFromStart();
                var visitedCells = new List<(int x, int y)>();
                for (int y = 1; y <= height - 2; y++)
                {
                    for (int x = 1; x <= width - 2; x++)
                    {
                        if (visited[y, x]) visitedCells.Add((x, y));
                    }
                }

                int connectedCount = visitedCells.Count;
                if (connectedCount == openCount) break;

                int bestDist = int.MaxValue;
                (int x, int y) target = (1, 1);
                (int x, int y) anchor = (1, 1);

                for (int ty = 1; ty <= height - 2; ty++)
                {
                    for (int tx = 1; tx <= width - 2; tx++)
                    {
                        
                        if (!open[ty, tx] || visited[ty, tx]) continue;
                        //searching for the closest unvisited (target) cell from the visited cells (anchor)
                        foreach (var (vx, vy) in visitedCells)
                        {
                            int dist = Math.Abs(tx - vx) + Math.Abs(ty - vy);
                            if (dist < bestDist)
                            {
                                bestDist = dist;
                                target = (tx, ty);
                                anchor = (vx, vy);
                            }
                        }
                    }
                }
                //current coords
                int wx = anchor.x;
                int wy = anchor.y;
                int safety = width * height * 10;
                while ((wx != target.x || wy != target.y) && safety-- > 0)
                {
                    open[wy, wx] = true;
                    //randomly choosing shape of the tunnel
                    bool moveHorizFirst = rand.Next(0, 2) == 0;
                    if (moveHorizFirst && wx != target.x)
                    {
                        wx += Math.Sign(target.x - wx);
                    }
                    else if (wy != target.y)
                    {
                        wy += Math.Sign(target.y - wy);
                    }
                    else if (wx != target.x)
                    {
                        wx += Math.Sign(target.x - wx);
                    }
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

        for (int y = 1; y <= height - 2; y += 2)
        {
            for (int x = 1; x <= width - 2; x += 2)
            {
                if (open[y, x]) continue;
                GrowMaze(x, y);
            }
        }

        ConnectRegions();
        RemoveDeadEnds();

        // Ensure the start room stays open.
        for (int ry = 1; ry <= 3; ry++)
        {
            for (int rx = 1; rx <= 3; rx++)
            {
                open[ry, rx] = true;
            }
        }
        ConnectAllComponentsToStart();
        CloseBorders();
       
        

        
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (open[y, x])
                {
                    World[y, x] = new EmptyField();
                }
                else
                {
                    World[y, x] = new NonEnterableField();
                }
            }
        }
    }
    public static void DrunkardsWalk(ref Field[,] World)
    {
        int curx = 1;
        int cury = 1;
        Random rand = new Random(System.DateTime.Now.Millisecond);
        bool[,] visited = new bool[42, 22];
        for (int i = 0; i < 42; i++)
        {
            visited[i,0] = false;
            visited[i,21] = false;
        }

        for (int i = 0; i < 22; i++)
        {
            visited[0, i]= false;
            visited[41, i] = false;
        }
        for (int i = 0; i < 20 * 40 *3; i++)
        {
            
            
            
            int new_x = curx;
            int new_y = cury;
            do
            {
                new_x =curx;
                new_y = cury;
                Direction dir = GetRandomDirection(rand);
                NewXY(dir, ref new_x, ref new_y);
            } while (new_x < 1 || new_x > 20 || new_y < 1 || new_y > 40);
            visited[new_y, new_x] = true;
            curx = new_x;
            cury = new_y;
        }

        

        for (int y = 0; y < 42; y++)
        {
            for (int x = 0; x < 22; x++)
            {
                if (visited[y, x] == false)
                {
                    World[y, x] = new NonEnterableField();
                }
                else
                {
                    World[y, x] = new EmptyField();
                }
            }
        }
        
        
    }
}