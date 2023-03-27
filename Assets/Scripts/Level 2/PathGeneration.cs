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
    public const int PixelsPerTile = 2;

    /*
    private void Start()
    {
        GenerateMaze(1, 1, 11, 11); // single tile, 11px x 11px texture
        Debug.Log(WriteMaze());
    }
    */

    // Creates a single maze that can be applied as a path to many different tiles
    // mapDepth and mapWidth are the dimensions of the level in tiles
    // textureDepth and textureWidth are the dimensions of the texture map for a single tile
    public void GenerateMaze(int mapDepth, int mapWidth, int textureDepth, int textureWidth)
    {
        // Creating a map of all walls
        Maze = new MazeTile[mapDepth * textureDepth / PixelsPerTile, mapWidth * textureWidth / PixelsPerTile];
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

        Debug.Log($"Ended with {totalPathTiles} path tiles in maze ({(float)totalPathTiles / Maze.Length}% path)");
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

        return surroundingWalls >= 3 && hasClosedCorners;
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
        Point scaledLocation = new Point(location.z / PixelsPerTile, location.x / PixelsPerTile);
        // out of bounds locations will give an invalid point
        if (!PointInMaze(scaledLocation))
        {
            return MazeTile.Invalid;
        }

        return Maze[scaledLocation.z, scaledLocation.x];
    }
}
