using System.Collections.Generic;
using System.Linq;
using QPath;
using UnityEngine;

/// <summary>
/// The Hex class defines the grid position, world space position, size, 
/// neighbours, etc... of a Hex Tile. However, it does NOT interact with
/// Unity directly in any way.
/// </summary>
public class Hex : IQPathTile {

    public Hex(HexMap hexMap, int q, int r)
    {
        this.HexMap = hexMap;

        this.Q = q;
        this.R = r;
        this.S = -(q + r);

        units = new HashSet<Unit>();

        floatParams[HEX_FLOAT_PARAMS.Elevation] = 0;
        floatParams[HEX_FLOAT_PARAMS.Moisture] = 0;
        floatParams[HEX_FLOAT_PARAMS.IronOre] = 0;
        floatParams[HEX_FLOAT_PARAMS.XOffset] = UnityEngine.Random.Range(0, 0.5f) - 0.25f;
        floatParams[HEX_FLOAT_PARAMS.ZOffset] = UnityEngine.Random.Range(0, 0.5f) - 0.25f;
    }

    // Q + R + S = 0
    // S = -(Q + R)

    public readonly int Q;  // Column
    public readonly int R;  // Row
    public readonly int S;

    // Data for map generation and maybe in-game effects
    public enum HEX_FLOAT_PARAMS { Elevation, Moisture, IronOre, XOffset, ZOffset };
    public Dictionary<HEX_FLOAT_PARAMS, float> floatParams = new Dictionary<HEX_FLOAT_PARAMS, float>();
    
    public float Elevation {
        get {
            return floatParams[HEX_FLOAT_PARAMS.Elevation];
        }

        set {
            floatParams[HEX_FLOAT_PARAMS.Elevation] = value;
        }
    }
    public float Moisture {
        get {
            return floatParams[HEX_FLOAT_PARAMS.Moisture];
        }

        set {
            floatParams[HEX_FLOAT_PARAMS.Moisture] = value;
        }
    }

    public enum TERRAIN_TYPE { PLAINS, GRASSLANDS, MARSH, FLOODPLAINS, DESERT, LAKE, OCEAN }
    public enum ELEVATION_TYPE { FLAT, HILL, MOUNTAIN, WATER }

    public TERRAIN_TYPE TerrainType { get; set; }
    public ELEVATION_TYPE ElevationType { get; set; }

    public enum FEATURE_TYPE { NONE, FOREST, RAINFOREST, MARSH }
    public FEATURE_TYPE FeatureType { get; set; }

    public readonly HexMap HexMap;

    static readonly float WIDTH_MULTIPLIER = Mathf.Sqrt(3) / 2;

    float radius = 1f;
    Hex[] neighbours;

    HashSet<Unit> units;
    public Unit[] Units { 
        get{
            return units.ToArray();
        }
    }
    public SurfaceStructure SurfaceStructure { get; protected set; }
    public City City { get; protected set; }

    public override string ToString()
    {
        return Q + ", " + R;
    }

    /// <summary>
    /// Returns the world-space position of this hex
    /// </summary>
    public Vector3 Position()
    {
        return new Vector3(
            HexHorizontalSpacing() * (this.Q + this.R/2f),
            0,
            HexVerticalSpacing() * this.R
        );
    }

    public float HexHeight()
    {
        return radius * 2;
    }

    public float HexWidth()
    {
        return WIDTH_MULTIPLIER * HexHeight();
    }

    public float HexVerticalSpacing()
    {
        return HexHeight() * 0.75f;
    }

    public float HexHorizontalSpacing()
    {
        return HexWidth();
    }

    public Vector3 PositionFromCamera()
    {
        return HexMap.GetHexPosition(this);
    }

    public Vector3 PositionFromCamera( Vector3 cameraPosition)
    {
        float numRows = HexMap.NumRows;
        float numColumns = HexMap.NumColumns;
        float mapHeight = numRows * HexVerticalSpacing();
        float mapWidth  = numColumns * HexHorizontalSpacing();

        Vector3 position = Position();

        if(HexMap.AllowWrapEastWest)
        {
            float howManyWidthsFromCamera = Mathf.Round((position.x - cameraPosition.x) / mapWidth);
            int howManyWidthToFix = (int)howManyWidthsFromCamera;

            position.x -= howManyWidthToFix * mapWidth;
        }

        if(HexMap.AllowWrapNorthSouth)
        {
            float howManyHeightsFromCamera = Mathf.Round((position.z - cameraPosition.z) / mapHeight);
            int howManyHeightsToFix = (int)howManyHeightsFromCamera;

            position.z -= howManyHeightsToFix * mapHeight;
        }


        return position;
    }

    public static float CostEstimate(IQPathTile aa, IQPathTile bb, IQPathUnit unit)
    {
        float normalHexesPerTurn = ((Unit)unit).NormalHexesPerTurn;
        float distance = Distance((Hex)aa, (Hex) bb);
        return distance / normalHexesPerTurn;
    }

    public static float Distance(Hex a, Hex b)
    {
        int width = a.HexMap.NumColumns;
        int halfWidth = width / 2;
        int height = a.HexMap.NumRows;
        int halfHeight = height / 2;

        int dQ = Mathf.Abs(a.Q - b.Q);
        int dR = Mathf.Abs(a.R - b.R);

        int aQAdjusted = a.Q;
        int aRAdjusted = a.R;
                
        if (a.HexMap.AllowWrapEastWest && dQ > halfWidth)
        {
            dQ = width - dQ;
            if(aQAdjusted > halfWidth)
            {
                aQAdjusted -= width;
            }
            else
            {
                aQAdjusted += width;
            }
        }

        
        if(a.HexMap.AllowWrapNorthSouth && dR > halfHeight)
        {
            dR = height - dR;
            if (aRAdjusted > halfHeight)
            {
                aRAdjusted -= height;
            }
            else
            {
                aQAdjusted += height;
            }
        }

        int aSAdjusted = -(aQAdjusted + aRAdjusted);
        int dS = Mathf.Abs(aSAdjusted - b.S);

        return 
            Mathf.Max( 
                dQ,
                dR,
                dS
            );
    }

    public void AddUnit( Unit unit )
    {
        if(units == null)
        {
            units = new HashSet<Unit>();
        }

        units.Add(unit);
    }

    public void RemoveUnit( Unit unit )
    {
        if(units != null)
        {
            units.Remove(unit);
        }
    }

    public void AddCity( City city )
    {
        if(this.City != null)
        {
            throw new UnityException("Trying to add a city to a hex that already has one!");
        }

        this.City = city;
    }

    public void RemoveCity( City city )
    {
        if(this.City == null)
        {
            Debug.LogError("Trying to remove a city where there isn't one!");
            return;
        }

        if(this.City != city)
        {
            Debug.LogError("Trying to remove a different city!");
            return;
        }

        this.City = null;
    }

    public void AddSurfaceStructure( SurfaceStructure surfaceStructure )
    {
        if(this.SurfaceStructure != null)
        {
            throw new UnityException("Trying to add a surface structure to a hex that already has one!");
        }

        this.SurfaceStructure = surfaceStructure;
    }

    public void RemoveSurfaceStructure( SurfaceStructure surfaceStructure )
    {
        if(this.SurfaceStructure == null)
        {
            Debug.LogError("Trying to remove a surface structure where there isn't one!");
            return;
        }

        if(this.SurfaceStructure != surfaceStructure)
        {
            Debug.LogError("Trying to remove a different surface structure!");
            return;
        }

        this.SurfaceStructure = null;
    }

    /// <summary>
    /// Returns the most common movement cost for this tile, for a typical melee unit
    /// </summary>
    public int BaseMovementCost( bool isHillWalker, bool isForestWalker, bool isFlier)
    {
        if( (ElevationType == ELEVATION_TYPE.MOUNTAIN || ElevationType == ELEVATION_TYPE.WATER) && isFlier == false )
            return -99;

        int moveCost = 1;

        if (!isFlier) {
            if( ElevationType == ELEVATION_TYPE.HILL && isHillWalker == false )
                moveCost += 1;

            if( (FeatureType == FEATURE_TYPE.FOREST || FeatureType == FEATURE_TYPE.RAINFOREST) && isForestWalker == false )
                moveCost += 1;
        }

        return moveCost;
    }

    #region IQPathTile implementation
    public IQPathTile[] GetNeighbours()
    {
        if(this.neighbours != null)
            return this.neighbours;
        
        List<Hex> neighbours = new List<Hex>();

        neighbours.Add( HexMap.GetHexAt( Q +  1,  R +  0 ) );
        neighbours.Add( HexMap.GetHexAt( Q + -1,  R +  0 ) );
        neighbours.Add( HexMap.GetHexAt( Q +  0,  R + +1 ) );
        neighbours.Add( HexMap.GetHexAt( Q +  0,  R + -1 ) );
        neighbours.Add( HexMap.GetHexAt( Q + +1,  R + -1 ) );
        neighbours.Add( HexMap.GetHexAt( Q + -1,  R + +1 ) );

        List<Hex> neighbours2 = new List<Hex>();

        foreach(Hex h in neighbours)
        {
            if(h != null)
            {
                neighbours2.Add(h);
            }
        }

        this.neighbours = neighbours2.ToArray();

        return this.neighbours;
    }

    public float AggregateCostToEnter(float costSoFar, IQPathTile sourceTile, IQPathUnit theUnit)
    {
        // TODO: We are ignoring source tile right now, this will have to change when
        // we have rivers.
        return ((Unit)theUnit).AggregateTurnsToEnterHex(this, costSoFar);
    }
    #endregion
}
