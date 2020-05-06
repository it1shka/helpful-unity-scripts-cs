using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

#region Master Generator
public class DungeonGeneratorPro : MonoBehaviour
{
    public Vector2 tileSize = new Vector2(1f,1f);
    public GameObject spaceTile, wallTile;
    public Vector2Int mapSize = new Vector2Int(100,100);

    public enum Algorithms {
        Tunneling, RandomWalk, CellularAutomata, BSPTree
    }

    public Algorithms algorithm = Algorithms.Tunneling;

    //tunneling
    public Vector2Int minRoomSize = new Vector2Int(7,7), 
        maxRoomSize = new Vector2Int(15,15);
    public int roomsAmount = 10;
    public bool connectAll = false;

    //randomwalk
    public int steps = 1500;

    //cellular automata
    [Range(0,1)]public float wallPercentage = .47f;
    public int depth = 5;

    //bsp tree
    public int maxLeafSize = 20;
    public int minLeafSize = 6;


    private int[,] map;
    private List<GameObject> mapObjects;
    private void Start()
    {
        mapObjects = new List<GameObject>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            MakeLevel();
    }

    public void MakeLevel()
    {
        ClearMap();
        GenerateDungeon();
        Visualize();
    }

    private void GenerateDungeon()
    {
        switch (algorithm) {
            case Algorithms.Tunneling:
                map = new TunnelingAlgorithm(mapSize, minRoomSize,
                    maxRoomSize, roomsAmount, connectAll).Generate();
                break;
            case Algorithms.RandomWalk:
                map = new RandomWalk(mapSize, steps).Generate();
                break;
            case Algorithms.CellularAutomata:
                map = new CellularAutomata(mapSize,
                    wallPercentage, depth).Generate();
                break;
            case Algorithms.BSPTree:
                map = new BSPTree(mapSize, 
                    maxLeafSize, minLeafSize).Generate();
                break;
        }
    }

    private void Visualize()
    {
        for (var i = 0; i < mapSize.x; i++)
            for (var j = 0; j < mapSize.y; j++)
                mapObjects.Add(Instantiate(map[i, j] == 0 ? wallTile : spaceTile, 
                    new Vector2(tileSize.x * i, tileSize.y * j), Quaternion.identity));
    }

    private void ClearMap()
    {
        foreach (var e in mapObjects)
            Destroy(e);
        mapObjects.Clear();
    }
}
#endregion

#region Custom Inspector
#if UNITY_EDITOR
[CustomEditor(typeof(DungeonGeneratorPro))]
public class MyCustomEditor : Editor
{
    //add something to make custom inspector
}
#endif
#endregion

#region Tunneling Algorithm
public class TunnelingAlgorithm {

    private Vector2Int mapSize;
    private Vector2Int minRoomSize, maxRoomSize;
    private int roomsAmount;
    private bool connectAll;

    private List<Room> rooms;
    private int[,] map;
    public TunnelingAlgorithm(Vector2Int mapSize, Vector2Int minRoomSize, Vector2Int maxRoomSize, 
        int roomsAmount, bool connectAll)
    {
        this.mapSize = mapSize;
        this.minRoomSize = minRoomSize;
        this.maxRoomSize = maxRoomSize;
        this.roomsAmount = roomsAmount;
        this.connectAll = connectAll;
    }

    public int[,] Generate()
    {
        map = new int[mapSize.x, mapSize.y];
        rooms = new List<Room>();
        BuildRooms();
        BuildTunnels();

        return map;
    }

    private void BuildRooms()
    {
        for(var i=0; i<roomsAmount; i++)
        {
            var newSize = new Vector2Int(
                Random.Range(minRoomSize.x, maxRoomSize.x), 
                Random.Range(minRoomSize.y, maxRoomSize.y));

            var newPosition = new Vector2Int(
                Random.Range(1, mapSize.x - newSize.x - 1), 
                Random.Range(1, mapSize.y - newSize.y - 1));

            var newRoom = new Room(newPosition, newSize);
            var intersects = false;
            foreach(var r in rooms)
            {
                if(!CheckIntersection(newRoom, r))
                {
                    intersects = true;
                    break;
                }
            }

            if (!intersects)
            {
                rooms.Add(newRoom);
                BuildRoom(newRoom);
            }
        }


    }

    private bool CheckIntersection(Room r1, Room r2)
    {
        return ( (r1.position.x > r2.position.x + r2.size.x) || 
            (r1.position.x + r1.size.x < r2.position.x) ||
            (r1.position.y > r2.position.y + r2.size.y) ||
            (r1.position.y + r1.size.y < r2.position.y) );
    }

    private void BuildRoom(Room room)
    {
        for (var i = room.position.x; i < room.position.x + room.size.x; i++)
            for (var j = room.position.y; j < room.position.y + room.size.y; j++)
                map[i, j] = 1;
    }

    private void BuildTunnels()
    {
        if (connectAll)
        {
            for (var i = 0; i < rooms.Count; i++)
                for (var j = i + 1; j < rooms.Count; j++)
                    BuildTunnel(rooms[i].center, rooms[j].center);
        }
        else
        {
            for (var i = 0; i < rooms.Count - 1; i++)
                BuildTunnel(rooms[i].center, rooms[i + 1].center);
        }
    }

    #region BFS tunnels
    /*private void BuildTunnel(Vector2Int center1, Vector2Int center2)
    {
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        queue.Enqueue(center1);

        bool[,] used = new bool[mapSize.x, mapSize.y];
        Vector2Int[,] previous = new Vector2Int[mapSize.x, mapSize.y];
        
        while(queue.Count > 0)
        {
            var cur = queue.Dequeue();
            if (used[cur.x, cur.y]) continue;

            used[cur.x, cur.y] = true;

            if(cur.x + 1 < mapSize.x && !used[cur.x + 1, cur.y])
            {
                var step = new Vector2Int(cur.x + 1, cur.y);
                queue.Enqueue(step);
                previous[step.x, step.y] = cur;
                if (step == center2) break;
            }

            if (cur.x - 1 >= 0 && !used[cur.x - 1, cur.y])
            {
                var step = new Vector2Int(cur.x - 1, cur.y);
                queue.Enqueue(step);
                previous[step.x, step.y] = cur;
                if (step == center2) break;
            }

            if (cur.y + 1 < mapSize.y && !used[cur.x, cur.y + 1])
            {
                var step = new Vector2Int(cur.x, cur.y + 1);
                queue.Enqueue(step);
                previous[step.x, step.y] = cur;
                if (step == center2) break;
            }

            if (cur.y - 1 >= 0 && !used[cur.x, cur.y - 1])
            {
                var step = new Vector2Int(cur.x, cur.y - 1);
                queue.Enqueue(step);
                previous[step.x, step.y] = cur;
                if (step == center2) break;
            }
        }

        var curPos = center2;
        while (curPos != center1)
        {
            map[curPos.x, curPos.y] = 1;
            curPos = previous[curPos.x, curPos.y];
        }
    }*/
    #endregion

    #region Simple tunnels
    private void BuildTunnel(Vector2Int center1, Vector2Int center2)
    {
        int rnd = Random.Range(0, 2);
        if(rnd == 1) //horizontally
        {
            HorTunnel(center1.x, center2.x, center1.y);
            VertTunnel(center1.y, center2.y, center2.x);
        }
        else //vertically
        {

            VertTunnel(center1.y, center2.y, center2.x);
            HorTunnel(center1.x, center2.x, center1.y);
        }
    }

    private void HorTunnel(int x1, int x2, int y)
    {
        for(var x=Mathf.Min(x1,x2); x <= Mathf.Max(x1,x2); x++)
        {
            map[x, y] = 1;
        }
    }

    private void VertTunnel(int y1, int y2, int x)
    {
        for(var y=Mathf.Min(y1, y2); y<=Mathf.Max(y1, y2); y++)
        {
            map[x, y] = 1;
        }
    }
    #endregion

}

public class Room {
    public Vector2Int position, size, center;
    public Room(Vector2Int position, Vector2Int size)
    {
        this.position = position;
        this.size = size;
        this.center = new Vector2Int(
            position.x + size.x / 2, 
            position.y + size.y / 2);
    }
}
#endregion

#region Drunkard's Walk
public class RandomWalk {

    private Vector2Int mapSize;
    private int steps;

    private int[,] map;

    public RandomWalk(Vector2Int mapSize, int steps)
    {
        this.mapSize = mapSize;
        this.steps = steps;
    }

    public int[,] Generate()
    {
        if(steps > (mapSize.x - 2) * (mapSize.y - 2))
        {
            Debug.LogError("Unable to generate a map. Make the steps amount lower. ");
            return null;
        }

        map = new int[mapSize.x, mapSize.y];
        var curPoint = new Vector2Int(
            Random.Range(1, mapSize.x), 
            Random.Range(1, mapSize.y));

        map[curPoint.x, curPoint.y] = 1;
        while(steps > 0)
        {
            var rnd = Random.Range(0, 4);

            if (curPoint.x >= mapSize.x - 2 && rnd == 1) rnd = 3;
            else if (curPoint.x <= 1 && rnd == 3) rnd = 1;
            else if (curPoint.y >= mapSize.y - 2 && rnd == 0) rnd = 2;
            else if (curPoint.y <= 1 && rnd == 2) rnd = 0;

            switch (rnd) {
                case 0: //up
                    curPoint.y++;
                    break;
                case 1: //right
                    curPoint.x++;
                    break;
                case 2: //down
                    curPoint.y--;
                    break;
                case 3: //left
                    curPoint.x--;
                    break;
            }

            if (map[curPoint.x, curPoint.y] == 0)
            {
                map[curPoint.x, curPoint.y] = 1;
                steps--;
            }

        }

        return map;
    }
}

#endregion

#region Cellular Automata
public class CellularAutomata
{
    private Vector2Int mapSize;
    private float wallPercentage;
    private int depth;
    
    private int[,] map;

    public CellularAutomata(Vector2Int mapSize, float wallPercentage, int depth)
    {
        this.mapSize = mapSize;
        this.wallPercentage = wallPercentage;
        this.depth = depth;
    }

    public int[,] Generate()
    {
        map = new int[mapSize.x, mapSize.y];

        for (var i = 0; i < mapSize.x; i++)
            for (var j = 0; j < mapSize.y; j++)
                map[i, j] = Random.value < wallPercentage ? 0 : 1;

        for(var i = 0; i < depth; i++)
        {
            var newMap = new int[mapSize.x, mapSize.y];
            // minimize noise
            for (var w = 0; w < mapSize.x - 3; w++)
            {
                for(var h = 0; h < mapSize.y - 3; h++)
                {
                    //got segment
                    var walls = 0;
                    for(var w2 = w; w2 < w + 3; w2++)
                    {
                        for(var h2 = h; h2 < h + 3; h2++)
                        {
                            if (map[w2, h2] == 0) walls++;
                        }
                    }

                    if (walls >= 5)
                        newMap[w + 1, h + 1] = 0;
                    else newMap[w + 1, h + 1] = 1;

                }
            }

            map = newMap;

        }

        return map;
    }

}
#endregion

#region BSP Tree

public class BSPTree
{
    private Vector2Int mapSize;
    private int MAX_LEAF_SIZE;
    private int MIN_LEAF_SIZE;

    private int[,] map;
    public BSPTree(Vector2Int mapSize, int maxLeafSize, int minLeafSize)
    {
        this.mapSize = mapSize;
        this.MAX_LEAF_SIZE = maxLeafSize;
        this.MIN_LEAF_SIZE = minLeafSize;
    }

    public int[,] Generate() {
        map = new int[mapSize.x, mapSize.y];

        var leafs = new List<Leaf>();
        var root = new Leaf(MIN_LEAF_SIZE, 
            new Vector2Int(1,1),
            mapSize - new Vector2Int(2,2));
        leafs.Add(root);
        var didSplit = true;

        while (didSplit)
        {
            didSplit = false;
            var newLeafs = new List<Leaf>(leafs);
            foreach(var l in leafs)
            {
                if(l.child1 == null && l.child2 == null)
                {
                    if(l.size.x > MAX_LEAF_SIZE || 
                        l.size.y > MAX_LEAF_SIZE || 
                        Random.value > .25f)
                    {
                        if (l.Split())
                        {
                            newLeafs.Add(l.child1);
                            newLeafs.Add(l.child2);
                            didSplit = true;
                        }
                    }
                }
            }

            leafs = newLeafs;
        }
        root.CreateRooms();

        foreach (var l in leafs)
        {
            if (l.room != null)
                BuildRoom(l.room);
            foreach (var t in l.tunnels)
                BuildTunnel(t);
        }

        return map;
    }

    private void BuildRoom(Room room)
    {
        for (var i = room.position.x; i < room.position.x + room.size.x; i++)
            for (var j = room.position.y; j < room.position.y + room.size.y; j++)
                map[i, j] = 1;
    }

    #region Simple tunnels
    private void BuildTunnel(Tunnel tunnel)
    {
        var center1 = tunnel.center1;
        var center2 = tunnel.center2;

        int rnd = Random.Range(0, 2);
        if (rnd == 1) //horizontally
        {
            HorTunnel(center1.x, center2.x, center1.y);
            VertTunnel(center1.y, center2.y, center2.x);
        }
        else //vertically
        {

            VertTunnel(center1.y, center2.y, center2.x);
            HorTunnel(center1.x, center2.x, center1.y);
        }
    }

    private void HorTunnel(int x1, int x2, int y)
    {
        for (var x = Mathf.Min(x1, x2); x <= Mathf.Max(x1, x2); x++)
        {
            map[x, y] = 1;
        }
    }

    private void VertTunnel(int y1, int y2, int x)
    {
        for (var y = Mathf.Min(y1, y2); y <= Mathf.Max(y1, y2); y++)
        {
            map[x, y] = 1;
        }
    }
    #endregion

}

public class Leaf
{
    private int MIN_LEAF_SIZE;
    public Vector2Int position, size;

    public Leaf child1, child2;
    public Room room;

    public List<Tunnel> tunnels;

    public Leaf(int minLeafSize, Vector2Int position, Vector2Int size) //include all
    {
        this.MIN_LEAF_SIZE = minLeafSize;
        this.position = position;
        this.size = size;

        tunnels = new List<Tunnel>();
    }

    public bool Split()
    {
        if (child1 != null || child2 != null)
            return false;

        var splitH = Random.value > .5f;
        if ((float)size.x / size.y >= 1.25f)
            splitH = false;
        else if ((float)size.y / size.x >= 1.25f)
            splitH = true;

        var max = (splitH ? size.y : size.x) - MIN_LEAF_SIZE;
        if (max <= MIN_LEAF_SIZE) return false;
        var border = Random.Range(MIN_LEAF_SIZE, max + 1);
 
        if (splitH)
        {
            child1 = new Leaf(MIN_LEAF_SIZE, position, new Vector2Int(size.x, border) );
            child2 = new Leaf(MIN_LEAF_SIZE, new Vector2Int(position.x, position.y + border), new Vector2Int(size.x, size.y - border));
        }
        else
        {
            child1 = new Leaf(MIN_LEAF_SIZE, position, new Vector2Int(border, size.y) );
            child2 = new Leaf(MIN_LEAF_SIZE, new Vector2Int(position.x + border, position.y), new Vector2Int(size.x - border, size.y));
        }
        return true;
    }

    public void CreateRooms()
    {
        if(child1 != null || child2 != null)
        {
            if (child1 != null)
                child1.CreateRooms();
            if (child2 != null)
                child2.CreateRooms();

            if (child1 != null && child2 != null)
                tunnels.Add(new Tunnel(child1.GetRoom().center, 
                    child2.GetRoom().center));
        }
        else
        {
            var roomSize = new Vector2Int(
                Random.Range(2, size.x),
                Random.Range(2, size.y)
            );
            var roomPos = new Vector2Int(
                position.x + Random.Range(1, size.x - roomSize.x),
                position.y + Random.Range(1, size.y - roomSize.y)
            );

            room = new Room(roomPos, roomSize);
            //room = new Room(position + new Vector2Int(1,1), size - new Vector2Int(1,1)); //test
        }
    }

    public Room GetRoom()
    {
        if (room != null)
            return room;
        else
        {
            Room room1 = null, room2 = null;
            if (child1 != null)
                room1 = child1.GetRoom();
            if (child2 != null)
                room2 = child2.GetRoom();

            if (room1 == null && room2 == null)
                return null;
            if (room1 == null)
                return room2;
            if (room2 == null)
                return room1;
            return Random.value > 0.5 ? room1 : room2;
        }
    }

}

public class Tunnel
{
    public Vector2Int center1, center2;
    public Tunnel(Vector2Int center1, Vector2Int center2)
    {
        this.center1 = center1;
        this.center2 = center2;
    }
}

#endregion
