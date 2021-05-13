using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Pathfinder : MonoBehaviour
{
    [SerializeField] private Material startMaterial;
    [SerializeField] private Material testNodeMaterial;
    [SerializeField] private Material targetMaterial;
    [SerializeField] private Material defaultMaterial;
    [SerializeField] private Material barrierMaterial;
    
    private GameObject[] tiles;

    public Tile[,] grid = new Tile[6,6];
    
    // Start is called before the first frame update
    private void Start()
    {
        Debug.Log("To play click twice for the start and target points respectively.");
        Debug.Log("Click further to add barriers between the two points.");
        Debug.Log("Press space to start.");
        Debug.Log("Press return to restart.");
        // collate all Cube objects into an array of GameObjects
        tiles = GameObject.FindGameObjectsWithTag("Cube");
    }

    public void Astar()
    {
        // instantiate the grid with Tile objects
        for (int i = 0; i < 6; i++)
        {
            for (int j = 0; j < 6; j++)
            {
                grid[i, j] = new Tile();
            }
        }

        int count = 0;
        Tile current = null;
        
        // set the start and target points
        var start = new Tile {x = 0, y = 0};
        var target = new Tile {x = 0, y = 0};
        
        var openList = new List<Tile>();
        var closedList = new List<Tile>();
        int g = 0;
        
        
        // assign the cube objects to their position in the Tile grid
        // count is incremented to get next cube
        for (int i = 0; i < 6; i++)
        {
            for (int j = 0; j < 6; j++)
            {
                grid[j,i].cubeTile = tiles[count];
                grid[j,i].x = j;
                grid[j,i].y = i;
                if (grid[j,i].cubeTile.GetComponentInParent<Cube>().type == TileType.normal )
                {
                    grid[j, i].tileType = TileType.normal;
                }
                else if (grid[j,i].cubeTile.GetComponentInParent<Cube>().type == TileType.barrier)
                {
                    grid[j, i].tileType = TileType.barrier;
                }
                else if (grid[j,i].cubeTile.GetComponentInParent<Cube>().type == TileType.target)
                {
                    grid[j, i].tileType = TileType.target;
                    target.x = j;
                    target.y = i;
                }
                else if (grid[j, i].cubeTile.GetComponentInParent<Cube>().type == TileType.start)
                {
                    grid[j, i].tileType = TileType.start;
                    start.x = j;
                    start.y = i;
                }
                count++;
            }
        }

        // start tile is added to the openList
        openList.Add(start);
        
        Boolean found = false;

        // A* algorithm
        while (openList.Count > 0)
        {
            // tile with lowest F score is set as current
            // F = G+H
            var lowest = openList.Min(l => l.F);
            current = openList.First(l => l.F == lowest);
            
            // adds current to the list of visited nodes
            closedList.Add(current);
            // removes current from open list
            openList.Remove(current);
            
            // if the target tile has been added to the closed list then a path has been found and the while loop can be left
            if (closedList.FirstOrDefault(l => l.x == target.x && l.y == target.y) != null)
            {
                found = true;
                break; 
            }
    
            // adjacentTiles to the current tile are determined
            var adjacentTiles = getWalkableAdjacentTiles(current.x, current.y);
            
            // g is incremented as we will use the G value of it's parent on the next loop
            g = current.G + 1;
            
            
            
            foreach (var adjacentTile in adjacentTiles)
            {
                
                // if the adjacentTile is already in the closedList, ignore it
                if (closedList.FirstOrDefault(l=>l.x == adjacentTile.x && l.y == adjacentTile.y) != null) continue;
        
                // if the adjacentTile isn't already in the openList...
                if (openList.FirstOrDefault(l=>l.x == adjacentTile.x && l.y == adjacentTile.y) == null)
                {
                    // set G to the current g score which is the total cost of the path to the current tile
                    adjacentTile.G = g;
                    // calculate the H score which is the estimated movement cost from the current square to target
                    adjacentTile.H = CalculateH(adjacentTile.x, adjacentTile.y, target.x, target.y);
                    // calculate the F score by adding G and H
                    adjacentTile.F = adjacentTile.G + adjacentTile.H;
                    // set the parent to the current tile
                    adjacentTile.parent = current;
                    
                    // add the tile to the openList
                    openList.Insert(0, adjacentTile);
                }
                else
                {
                    // if using the current G score makes the parent's F score lower then replace the parent with current
                    // this signifies a better path
                    if (g+adjacentTile.H < adjacentTile.F)
                    {
                        adjacentTile.G = g;
                        adjacentTile.F = adjacentTile.G + adjacentTile.H;
                        adjacentTile.parent = current;
                    }
                }
            }
        }

        if (!found)
        {
            Debug.Log("A path could not be found to the target.");
        }
        else
        {
            StartCoroutine(testPath(closedList, current));
        }
    
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Astar();
        }
        if (Input.GetKeyDown(KeyCode.Return))
        {
            resetGrid();
            GlobalVariables.tiles = 0;
        }
    }

    void resetGrid()
    {
        foreach (var tile in tiles)
        {
            tile.GetComponentInParent<Cube>().type = TileType.normal;
            tile.GetComponent<Renderer>().material = defaultMaterial;
        }
    }

    IEnumerator drawPath(Tile current)
    {
        while (current !=null)
        {
            addTile(current.x, current.y, TileType.start);
            current = current.parent;
            yield return new WaitForSeconds(0.7f);
        }
    }
    
    IEnumerator testPath(List<Tile> tiles, Tile current)
    {
        foreach (var tile in tiles)
        {
            addTile(tile.x,tile.y, TileType.tested);
            yield return new WaitForSeconds(0.7f);
        }
        StartCoroutine(drawPath(current));
    }
    
    int CalculateH(int x, int y, int targetX, int targetY)
    {
        return Math.Abs(targetX - x) + Math.Abs(targetY - y);
    }

    List<Tile> getWalkableAdjacentTiles(int x, int y)
    {
        var proposedTiles = new List<Tile>();
        try
        {
            if (grid[x, y+1].tileType == TileType.normal ||
                grid[x, y+1].tileType == TileType.target)
            {
                proposedTiles.Add(grid[x,y+1]);
            }
        }
        catch (Exception e)
        {
            
        }

        try
        {
            if (grid[x, y-1].tileType == TileType.normal ||
                grid[x, y-1].tileType == TileType.target)
            {
                proposedTiles.Add(grid[x,y-1]);
            }
        }
        catch (Exception e)
        {
            
        }

        try
        {
            if (grid[x+1, y].tileType == TileType.normal ||
                grid[x+1, y].tileType == TileType.target)
            {
                proposedTiles.Add(grid[x+1, y]);
            }
        }
        catch (Exception e)
        {
            
        }

        try
        {
            if (grid[x-1, y].tileType == TileType.normal ||
                grid[x-1, y].tileType == TileType.target)
            {
                proposedTiles.Add(grid[x-1, y]);
            }
        }
        catch (Exception e)
        {
            
        }

        return proposedTiles;
    }

    public void addTile(int x, int y, TileType type)
    {
        Material material = defaultMaterial;
        switch (type)
        {
            case TileType.normal: material = defaultMaterial; break;
            case TileType.start: material = startMaterial; break;
            case TileType.target: material = targetMaterial; break;
            case TileType.tested: material = testNodeMaterial; break;
            case TileType.barrier: material = barrierMaterial; break;
        }
        grid[x, y].tileType = type; 
        grid[x, y].cubeTile.GetComponent<Renderer>().material = material;
    }
}

public class Tile
{
    public GameObject cubeTile;
    public int x;
    public int y;
    public int F;
    public int G;
    public int H;
    public Tile parent;
    public TileType tileType;

    
}
public enum TileType
{
    start,
    target,
    normal,
    tested,
    barrier
}

public static class GlobalVariables{
    public static int tiles;
}