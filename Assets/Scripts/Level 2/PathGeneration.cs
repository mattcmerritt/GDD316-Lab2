using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Enum type to represent each tile in the maze in a more meaningful way
[System.Serializable]
public enum MazeTile
{
    Path = 'O',
    Wall = 'X'
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

    private void Start()
    {
        GenerateMaze(1, 1, 11, 11); // single tile, 11px x 11px texture
        Debug.Log(WriteMaze());
    }

    // Creates a single maze that can be applied as a path to many different tiles
    // mapDepth and mapWidth are the dimensions of the level in tiles
    // textureDepth and textureWidth are the dimensions of the texture map for a single tile
    public void GenerateMaze(int mapDepth, int mapWidth, int textureDepth, int textureWidth)
    {
        // Creating a map of all walls
        Maze = new MazeTile[mapDepth * textureDepth, mapWidth * textureWidth];
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

        // loop to generate maze using DFS approach
        while (tilesToVisit.Count > 0)
        {
            // grab point off of candidate stack
            Point current = tilesToVisit.Pop();

            // verify that the point can be on the path, change it if so
            if (TileCanBePath(current))
            {
                Maze[current.z, current.x] = MazeTile.Path;
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

        List<Point> corners = GetNeighboringCornerTiles(location);

        // verify that all the corner tiles are walls (no diagonal paths)
        bool hasClosedCorners = true;
        foreach (Point corner in corners)
        {
            if (Maze[corner.z, corner.x] == MazeTile.Path)
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
    private List<Point> GetNeighboringCornerTiles(Point location)
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
    private string WriteMaze()
    {
        string maze = "";

        for (int z = 0; z < Maze.GetLength(0); z++)
        {
            for (int x = 0; x < Maze.GetLength(1); x++)
            {
                maze += (char) Maze[z, x];
            }
            maze += "\n";
        }

        return maze;
    }
}
