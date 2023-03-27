using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Enum type to represent each tile in the maze in a more meaningful way
[System.Serializable]
public enum MazeTile
{
    Path = 'O',
    Wall = 'X',
    Invalid = '?'
}

// Point class to represent locations in the maze
// Could have been Vector2, but I wanted the variables to match my maze
[System.Serializable]
public class Point
{
    public readonly int z;
    public readonly int x;

    public Point(int z, int x)
    {
        this.z = z;
        this.x = x;
    }
}

public class PathGeneration : MonoBehaviour
{
    private MazeTile[,] Maze;
    private List<PathedTileGeneration> Tiles;
    private int TexturePixelDepth, TexturePixelWidth;
    private int MapDepth, MapWidth;

    // Configurable values
    [SerializeField, Range(0f, 1f)] private float MinPathPercent = 0.4f; // The minimum percentage of the map that should be path
    [SerializeField] public const int PixelsPerSquare = 2; // The number of pixels on a texture that each road square takes up
    [SerializeField, Range(0f, 1f)] private float MaxPathHeight = 0.5f; // The height below which paths are allowed
    [SerializeField] private int PathStartsRemaining = 20; // The amount of tries the generation has to add new paths before giving up


    // Creates a single maze that can be applied as a path to many different tiles
    // mapDepth and mapWidth are the dimensions of the level in tiles
    // textureDepth and textureWidth are the dimensions of the texture map for a single tile
    public IEnumerator GenerateMaze(int mapDepth, int mapWidth, int textureDepth, int textureWidth, List<PathedTileGeneration> tiles)
    {
        // only allow the maze to start generating if every tile in the tiles list is configured
        yield return new WaitUntil(() => {
            foreach (PathedTileGeneration tile in tiles) {
                if (tile.GetHeightMap() == null || tile.GetTexture() == null) {
                    return false;
                }
            }
            return true;
        });

        // Saving list of all tiles for later use
        Tiles = tiles;
        MapDepth = mapDepth;
        MapWidth = mapWidth;
        TexturePixelDepth = textureDepth;
        TexturePixelWidth = textureWidth;

        // Creating a map of all walls
        Maze = new MazeTile[mapDepth * textureDepth / PixelsPerSquare, mapWidth * textureWidth / PixelsPerSquare];
        for (int z = 0; z < Maze.GetLength(0); z++)
        {
            for (int x = 0; x < Maze.GetLength(1); x++)
            {
                Maze[z, x] = MazeTile.Wall;
            }
        }

        // stack to contain all the tiles that are on the path and not yet processed
        Stack<Point> tilesToVisit = new Stack<Point>();

        // Starting the path at (1, 1)
        // Maze[1, 1] = MazeTile.Path;
        tilesToVisit.Push(new Point(1, 1));
        int totalPathTiles = 0;
        float pathPercent = (float) totalPathTiles / Maze.Length;
        while (pathPercent < MinPathPercent && PathStartsRemaining >= 0) 
        {
            // loop to generate maze using DFS approach
            while (tilesToVisit.Count > 0)
            {
                // grab point off of candidate stack
                Point current = tilesToVisit.Pop();

                // verify that the point can be on the path, change it if so
                if (TileCanBePath(current))
                {
                    Maze[current.z, current.x] = MazeTile.Path;
                    totalPathTiles++;
                }
                else
                {
                    continue; // move to next point, do not process neighbors of walls
                }

                // add the surrounding points onto the stack to keep processing the path
                List<Point> neighbors = RandomizeList(GetNeighboringTiles(current));
                
                // only include points that could potentially be path
                foreach (Point location in neighbors)
                {
                    if (TileCanBePath(location))
                    {
                        tilesToVisit.Push(location);
                    }
                }
            }

            // recalculate path percentage
            pathPercent = (float) totalPathTiles / Maze.Length;
            
            // if not enough path has been drawn, randomly put a value on the stack 
            // the algorithm will try to generate from the random point, or generate new ones
            if (pathPercent < MinPathPercent) 
            {
                tilesToVisit.Push(new Point(Random.Range(1, Maze.GetLength(0) - 1), Random.Range(1, Maze.GetLength(1) - 1)));
                PathStartsRemaining--; // prevent infinite looping
            }
        }

        Debug.Log($"Ended with {totalPathTiles} path tiles in maze ({pathPercent}% path)");
        // PrintAllTileData();
        Debug.Log(WriteMaze());
    }

    // Check that a wall tile can be replaced by a path tile
    // This will occur if a tile has 3 surrounding wall tiles
    private bool TileCanBePath(Point location)
    {
        List<Point> neighbors = GetNeighboringTiles(location);
        
        // count the number of walls surrounding the tile
        int surroundingWalls = 0;
        foreach (Point neighbor in neighbors)
        {
            if (Maze[neighbor.z, neighbor.x] == MazeTile.Wall)
            {
                surroundingWalls++;
            }
        }

        List<Point> corners = GetCornerTiles(location);

        // verify that all the corner tiles are walls (no diagonal paths)
        bool hasClosedCorners = true;
        foreach (Point corner in corners)
        {
            // if the corner is a wall, we do not need to check for a connection
            if (Maze[corner.z, corner.x] == MazeTile.Wall)
            {
                continue;
            }

            // if the corner is not a wall, it needs to have at least one adjacent neighbor 
            // also be on the path
            bool hasAdjacentNeighbor = false;
            foreach (Point neighbor in neighbors)
            {
                if ((corner.z == neighbor.z || corner.x == neighbor.x) && 
                    Maze[neighbor.z, neighbor.x] == MazeTile.Path)
                {
                    hasAdjacentNeighbor = true;
                }
            }
            
            // if one of the corners is on the path and has no adjacent neighboring path
            // adding the current tile to the path would create a diagonal path, which
            // is not allowed
            if (!hasAdjacentNeighbor)
            {
                hasClosedCorners = false;
            }
        }

        // verify that the tiles are not in the highlands height class
        int worldZ = location.z * PixelsPerSquare;
        int worldX = location.x * PixelsPerSquare;

        int localZ = worldZ % TexturePixelDepth;
        int localX = worldX % TexturePixelWidth;

        // fetching the data components from the correct tile
        PathedTileGeneration tile = Tiles[worldZ / TexturePixelDepth + worldX / TexturePixelWidth * MapDepth];
        float[,] tileHeightMap = tile.GetHeightMap();
        Texture2D tileTexture = tile.GetTexture();
        Color[] tileColorMap = tileTexture.GetPixels();

        // for some reason, the dimensions are reversed for the tile's
        // height map, texture, and color map
        float height = tileHeightMap[tileHeightMap.GetLength(0) - localZ - 1, tileHeightMap.GetLength(1) - localX - 1];

        // check if the ground is too steep for a road
        bool isMountainous = height >= MaxPathHeight;

        return surroundingWalls >= 3 && hasClosedCorners && !isMountainous;
    }

    // Fetch all of the neighboring tiles of a current location
    // Note: does not include tiles that are out of bounds
    private List<Point> GetNeighboringTiles(Point location)
    {
        List<Point> neighbors = new List<Point>();

        // fetching the four surrounding locations
        Point[] potentialNeighbors = new Point[]
        {
            new Point(location.z, location.x + 1), 
            new Point(location.z + 1, location.x), 
            new Point(location.z, location.x - 1), 
            new Point(location.z - 1, location.x)
        };
        // only save surrounding locations that are in the maze bounds
        foreach (Point potentialNeighbor in potentialNeighbors)
        {
            if (PointInMaze(potentialNeighbor))
            {
                neighbors.Add(potentialNeighbor);
            }
        }

        return neighbors;
    }

    // Fetch all of the diagonally neighboring tiles of a current location
    // Note: does not include tiles that are out of bounds
    private List<Point> GetCornerTiles(Point location)
    {
        List<Point> corners = new List<Point>();

        // fetching the four corner locations
        Point[] potentialCorners = new Point[]
        {
            new Point(location.z + 1, location.x + 1),
            new Point(location.z + 1, location.x - 1),
            new Point(location.z - 1, location.x + 1),
            new Point(location.z - 1, location.x - 1)
        };
        // only save surrounding locations that are in the maze bounds
        foreach (Point potentialNeighbor in potentialCorners)
        {
            if (PointInMaze(potentialNeighbor))
            {
                corners.Add(potentialNeighbor);
            }
        }

        return corners;
    }

    // Check if a given point falls in the boundaries of the maze
    // Note: the edges (outermost tiles) count as in
    private bool PointInMaze(Point location)
    {
        return 
            location.z >= 0 && 
            location.x >= 0 && 
            location.z < Maze.GetLength(0) && 
            location.x < Maze.GetLength(1);
    }

    // Randomize a given list of points
    private List<Point> RandomizeList(List<Point> list)
    {
        List<Point> randomList = new List<Point>();

        while (list.Count > 0)
        {
            int removeIndex = Random.Range(0, list.Count);
            randomList.Add(list[removeIndex]);
            list.RemoveAt(removeIndex);
        }

        return randomList;
    }

    // Writes the current maze to a string to be checked
    public string WriteMaze()
    {
        string maze = "";

        for (int z = Maze.GetLength(0) - 1; z >= 0; z--)
        {
            for (int x = 0; x < Maze.GetLength(1); x++)
            {
                maze += (char) Maze[z, x];
            }
            maze += "\n";
        }

        return maze;
    }

    // Retrieve a single tile from the maze
    public MazeTile GetTile(Point location)
    {
        Point scaledLocation = new Point(location.z / PixelsPerSquare, location.x / PixelsPerSquare);
        // out of bounds locations will give an invalid point
        if (!PointInMaze(scaledLocation))
        {
            return MazeTile.Invalid;
        }

        return Maze[scaledLocation.z, scaledLocation.x];
    }

    // Mostly used for testing, can be used to verify that all the maps are aligned
    private void PrintAllTileData() 
    {
        for (int z = 0; z < MapDepth * TexturePixelDepth; z++) 
        {
            for (int x = 0; x < MapWidth * TexturePixelWidth; x++) 
            {
                int pathZ = z / PixelsPerSquare;
                int pathX = x / PixelsPerSquare;

                int localZ = z % TexturePixelDepth;
                int localX = x % TexturePixelWidth;

                PathedTileGeneration tile = Tiles[z / TexturePixelDepth + x / TexturePixelWidth * MapDepth];
                float[,] tileHeightMap = tile.GetHeightMap();
                Texture2D tileTexture = tile.GetTexture();
                Color[] tileColorMap = tileTexture.GetPixels();

                Debug.Log(
                    $"Point: ({z}, {x})\n" +
                    $"Local Point: ({localZ}, {localX})\n" +
                    $"Path Point: ({pathZ}, {pathX})\n" +
                    $"Tile: {tile.gameObject.name}\n" +
                    $"Height: {tileHeightMap[tileHeightMap.GetLength(0) - localZ - 1, tileHeightMap.GetLength(1) - localX - 1]}\n" +
                    $"Color: {tileColorMap[tileColorMap.Length - (localZ + localX * TexturePixelDepth) - 1]}"
                );
            }
        }
    }
}
