using System;
using System.Collections;
using System.Collections.Generic;
using QPath;
using UnityEngine;

public class HexMap : MonoBehaviour, IQPathWorld {

	// Use this for initialization
	public virtual void StartGame () {

        GeneratePlayers( 6 );

        GenerateMap();
	}

    public delegate void CityCreatedDelegate ( City city, GameObject cityGO );
    public event CityCreatedDelegate OnCityCreated;

    public delegate void UnitCreatedDelegate ( Unit unit, GameObject unitGO );
    public event UnitCreatedDelegate OnUnitCreated;

    public delegate void StructureCreatedDelegate ( SurfaceStructure structure, GameObject structureGO );
    public event StructureCreatedDelegate OnStructureCreated;


    void GeneratePlayers( int numPlayers )
    {
        Players = new Player[numPlayers];
        for (int i = 0; i < numPlayers; i++)
        {
            Players[i] = new Player(i+1, "Player " + (i+1).ToString() );
            Players[i].Type = Player.PlayerType.AI;
        }
        //CurrentPlayer = Players[0];
        Players[0].Type = Player.PlayerType.LOCAL;
        currentPlayerIndex = 0;
    }

    public GameObject HexPrefab;

    public Mesh MeshWater;
    public Mesh MeshFlat;
    public Mesh MeshHill;
    public Mesh MeshMountain;

    public GameObject ForestPrefab;
    public GameObject JunglePrefab;
    public GameObject OrePrefab;

    public Material MatOcean;
    public Material MatPlains;
    public Material MatGrasslands;
    public Material MatJungle;
    public Material MatMountains;
    public Material MatDesert;

    public GameObject UnitDwarfPrefab;
    public GameObject UnitHumanPrefab;
    public GameObject UnitToucanPrefab;
    public GameObject UnitTruckPrefab;

    public GameObject CityPrefab;
    public GameObject WarehousePrefab;
    public GameObject OreMinePrefab;

    public int TurnNumber = 0;

    // Tiles with height above whatever, is a whatever
    [System.NonSerialized] public float HeightMountain = 1f;
    [System.NonSerialized] public float HeightHill = 0.6f;
    [System.NonSerialized] public float HeightFlat = 0.0f;

    [System.NonSerialized] public float MoistureJungle = 0.875f;
    [System.NonSerialized] public float MoistureForest = 0.75f;
    [System.NonSerialized] public float MoistureGrasslands = 0.5f;
    [System.NonSerialized] public float MoisturePlains = 0.125f;

    [System.NonSerialized] public float IronOreMinLimit = 0.7f;

    [System.NonSerialized] public int NumRows = 30;
    [System.NonSerialized] public int NumColumns = 60;

    // TODO: Link up with the Hex class's version of this
    [System.NonSerialized] public bool AllowWrapEastWest = true;
    [System.NonSerialized] public bool AllowWrapNorthSouth = false;

    private Hex[,] hexes;
    private Dictionary<Hex, GameObject> hexToGameObjectMap;
    private Dictionary<GameObject, Hex> gameObjectToHexMap;

    public Player[] Players;
    int currentPlayerIndex = 0;
    public Player CurrentPlayer 
    {
        get { return Players[currentPlayerIndex]; }
    }

    public void AdvanceToNextPlayer()
    {
        currentPlayerIndex = (currentPlayerIndex + 1) % Players.Length;

        if(currentPlayerIndex == 0)
        {
            // New turn has begun!
            TurnNumber++;
            Debug.Log("STARTING TURN: " + TurnNumber);
        }

        Debug.Log("Starting turn for player index: " + currentPlayerIndex + " -- " + CurrentPlayer.PlayerName + " -- " + CurrentPlayer.Type.ToString());
    }

    private Dictionary<Unit, GameObject> unitToGameObjectMap;
    private Dictionary<City, GameObject> cityToGameObjectMap;
    private Dictionary<SurfaceStructure, GameObject> structureToGameObjectMap;

    public Hex GetHexAt(int x, int y)
    {
        if(hexes == null)
        {
            Debug.LogError("Hexes array not yet instantiated.");
            return null;
        }

        if(AllowWrapEastWest)
        {
            x = x % NumColumns;
            if(x < 0)
            {
                x += NumColumns;
            }
        }
        if(AllowWrapNorthSouth)
        {
            y = y % NumRows;
            if(y < 0)
            {
                y += NumRows;
            }
        }

        try {
            return hexes[x, y];
        }
        catch
        {
            Debug.LogWarning("GetHexAt: " + x + "," + y);
            return null;
        }
    }

    public Hex GetHexFromGameObject(GameObject hexGO)
    {
        if( gameObjectToHexMap.ContainsKey(hexGO) )
        {
            return gameObjectToHexMap[hexGO];
        }

        return null;
    }

    public GameObject GetHexGO(Hex h)
    {
        if( hexToGameObjectMap.ContainsKey(h) )
        {
            return hexToGameObjectMap[h];
        }

        return null;
    }

    public GameObject GetUnitGO(Unit c)
    {
        if( unitToGameObjectMap.ContainsKey(c) )
        {
            return unitToGameObjectMap[c];
        }

        return null;
    }

    public Vector3 GetHexPosition(int q, int r)
    {
        Hex hex = GetHexAt(q, r);

        return GetHexPosition(hex);
    }

    public Vector3 GetHexPosition(Hex hex)
    {
        return hex.PositionFromCamera( Camera.main.transform.position, NumRows, NumColumns );
    }


    virtual public void GenerateMap()
    {
        // Generate a map filled with ocean

        hexes = new Hex[NumColumns, NumRows];
        hexToGameObjectMap = new Dictionary<Hex, GameObject>();
        gameObjectToHexMap = new Dictionary<GameObject, Hex>();

        for (int column = 0; column < NumColumns; column++)
        {
            for (int row = 0; row < NumRows; row++)
            {
                // Instantiate a Hex
                Hex h = new Hex( this, column, row );

                hexes[ column, row ] = h;

                Vector3 pos = h.PositionFromCamera( 
                    Camera.main.transform.position, 
                    NumRows, 
                    NumColumns 
                );


                GameObject hexGO = Instantiate(
                    HexPrefab, 
                    pos,
                    Quaternion.identity,
                    this.transform
                );

                hexToGameObjectMap[h] = hexGO;
                gameObjectToHexMap[hexGO] = h;

                h.TerrainType = Hex.TERRAIN_TYPE.OCEAN;
                h.ElevationType = Hex.ELEVATION_TYPE.WATER;

                hexGO.name = string.Format("HEX: {0},{1}", column, row);
                hexGO.GetComponent<HexComponent>().Hex = h;
                hexGO.GetComponent<HexComponent>().HexMap = this;


            }
        }

        UpdateHexVisuals();

        //StaticBatchingUtility.Combine( this.gameObject );
    }

    public void UpdateHexVisuals()
    {
        for (int column = 0; column < NumColumns; column++)
        {
            for (int row = 0; row < NumRows; row++)
            {
                Hex hex = hexes[column,row];

                UpdateHexElevationVisuals(hex);
                UpdateHexMoistureVisuals(hex);
                UpdateHexOreVisuals(hex);
                UpdateHexDebugText(hex);
            }
        }
    }

    public void UpdateHexElevationVisuals(Hex hex) {
        GameObject hexGO = hexToGameObjectMap[hex];

        HexComponent hexComp = hexGO.GetComponentInChildren<HexComponent>();
        MeshRenderer mr = hexGO.GetComponentInChildren<MeshRenderer>();
        MeshCollider mc = hexGO.GetComponentInChildren<MeshCollider>();
        MeshFilter mf = hexGO.GetComponentInChildren<MeshFilter>();

        if(hex.Elevation >= HeightMountain)
        {
            mr.material = MatMountains;
            mf.mesh = MeshMountain;
            hex.ElevationType = Hex.ELEVATION_TYPE.MOUNTAIN;
            hexComp.VerticalOffset = 0.83f;
        }
        else if(hex.Elevation >= HeightHill)
        {
            hex.ElevationType = Hex.ELEVATION_TYPE.HILL;
            mf.mesh = MeshHill;
            hexComp.VerticalOffset = 0.25f;
        }
        else if(hex.Elevation >= HeightFlat)
        {
            hex.ElevationType = Hex.ELEVATION_TYPE.FLAT;
            mf.mesh = MeshFlat;
        }
        else
        {
            hex.ElevationType = Hex.ELEVATION_TYPE.WATER;
            mr.material = MatOcean;
            mf.mesh = MeshWater;
        }

        mc.sharedMesh = mf.mesh;
    }

    public void UpdateHexMoistureVisuals(Hex hex) {
        GameObject hexGO = hexToGameObjectMap[hex];

        HexComponent hexComp = hexGO.GetComponentInChildren<HexComponent>();
        MeshRenderer mr = hexGO.GetComponentInChildren<MeshRenderer>();

        if(hex.Elevation >= HeightFlat && hex.Elevation < HeightMountain)
        {
            if(hex.Moisture >= MoistureJungle)
            {
                mr.material = MatJungle;
                hex.TerrainType = Hex.TERRAIN_TYPE.GRASSLANDS;
                hex.FeatureType = Hex.FEATURE_TYPE.RAINFOREST;

                // Spawn trees
                Vector3 p = hexGO.transform.position;
                p.y += hexComp.GetVerticalOffset(0,0);

                GameObject.Instantiate(JunglePrefab, p, Quaternion.identity, hexGO.transform);
            }
            else if(hex.Moisture >= MoistureForest)
            {
                mr.material = MatGrasslands;
                hex.TerrainType = Hex.TERRAIN_TYPE.GRASSLANDS;
                hex.FeatureType = Hex.FEATURE_TYPE.FOREST;

                // Spawn trees
                Vector3 p = hexGO.transform.position;
                p.y += hexComp.GetVerticalOffset(0,0);

                GameObject.Instantiate(ForestPrefab, p, Quaternion.identity, hexGO.transform);
            }
            else if(hex.Moisture >= MoistureGrasslands)
            {
                mr.material = MatGrasslands;
                hex.TerrainType = Hex.TERRAIN_TYPE.GRASSLANDS;
            }
            else if(hex.Moisture >= MoisturePlains)
            {
                mr.material = MatPlains;
                hex.TerrainType = Hex.TERRAIN_TYPE.PLAINS;
            }
            else 
            {
                mr.material = MatDesert;
                hex.TerrainType = Hex.TERRAIN_TYPE.DESERT;
            }
        }
    }

    public void UpdateHexOreVisuals(Hex hex) {
        GameObject hexGO = hexToGameObjectMap[hex];

        if(hex.Elevation >= HeightFlat && hex.Elevation < HeightMountain)
        {
            if(hex.floatParams[Hex.HEX_FLOAT_PARAMS.IronOre] >= IronOreMinLimit)
            {
                // Spawn ores
                Vector3 p = hexGO.transform.position;

                GameObject.Instantiate(OrePrefab, p, Quaternion.identity, hexGO.transform);
            }
        }
    }

    public void UpdateHexDebugText(Hex hex) {
        GameObject hexGO = hexToGameObjectMap[hex];

        hexGO.GetComponentInChildren<TextMesh>().text = 
                    string.Format("{0},{1}\n{2}", hex.Q, hex.R, hex.BaseMovementCost( false, false, false ));
    }

    public Hex[] GetHexesWithinRangeOf(Hex centerHex, int range)
    {
        List<Hex> results = new List<Hex>();

        for (int dx = -range; dx < range-1; dx++)
        {
            for (int dy = Mathf.Max(-range+1, -dx-range); dy < Mathf.Min(range, -dx+range-1); dy++)
            {
                results.Add( GetHexAt(centerHex.Q + dx, centerHex.R + dy) );
            }
        }

        return results.ToArray();
    }

    public void SpawnUnitAt( Unit unit, int q, int r, Player player)
    {
        if(unitToGameObjectMap == null)
        {
            unitToGameObjectMap = new Dictionary<Unit, GameObject>();
        }

        Hex myHex = GetHexAt(q, r);
        GameObject myHexGO = hexToGameObjectMap[myHex];
        HexComponent hexComp = myHexGO.GetComponentInChildren<HexComponent>();
        unit.SetHex(myHex);

        Vector3 position = myHexGO.transform.position;
        position.y += hexComp.GetVerticalOffset(0,0);
        GameObject unitGO = Instantiate(unit.prefab, position, Quaternion.identity, myHexGO.transform);
        UnitView unitView = unitGO.GetComponent<UnitView>();
        unit.OnObjectMoved += unitView.OnUnitMoved;
        unitView.Unit = unit;
        unitView.HexMap = this;

        unit.player = player;
        player.AddUnit(unit);
        unit.OnObjectDestroyed += OnUnitDestroyed;
        unitToGameObjectMap.Add(unit, unitGO);

        if(OnUnitCreated != null)
        {
            OnUnitCreated(unit, unitGO);
        }
    }

    public void OnUnitDestroyed( MapObject mo )
    {
        GameObject go = unitToGameObjectMap[(Unit)mo];
        unitToGameObjectMap.Remove((Unit)mo);
        Destroy(go);
    }

    public void SpawnCityAt( City city, GameObject prefab, int q, int r, Player player)
    {
        Debug.Log("SpawnCityAt");
        if(cityToGameObjectMap == null)
        {
            cityToGameObjectMap = new Dictionary<City, GameObject>();
        }

        Hex myHex = GetHexAt(q, r);
        GameObject myHexGO = hexToGameObjectMap[myHex];

        try
        {
            city.SetHex(myHex);
        }
        catch(UnityException e)
        {
            Debug.LogError(e.Message);
            return;
        }

        Vector3 cityPosition = myHexGO.transform.position;
        cityPosition.y += myHexGO.GetComponent<HexComponent>().GetVerticalOffset(0,0);

        GameObject cityGO = Instantiate(prefab, cityPosition, Quaternion.identity, myHexGO.transform);

        city.player = player;
        player.AddCity(city);
        city.OnObjectDestroyed += OnCityDestroyed;
        cityToGameObjectMap.Add(city, cityGO);

        if(OnCityCreated != null)
        {
            OnCityCreated(city, cityGO);
        }
    }

    public void OnCityDestroyed( MapObject mo )
    {
        GameObject go = cityToGameObjectMap[(City)mo];
        cityToGameObjectMap.Remove((City)mo);
        Destroy(go);
    }

    public void SpawnStructureAt( SurfaceStructure structure, GameObject prefab, int q, int r, Player player)
    {
        Debug.Log("SpawnStructureAt");
        if(structureToGameObjectMap == null)
        {
            structureToGameObjectMap = new Dictionary<SurfaceStructure, GameObject>();
        }

        Hex myHex = GetHexAt(q, r);
        GameObject myHexGO = hexToGameObjectMap[myHex];

        try
        {
            structure.SetHex(myHex);
        }
        catch(UnityException e)
        {
            Debug.LogError(e.Message);
            return;
        }

        Vector3 structurePosition = myHexGO.transform.position;
        structurePosition.y += myHexGO.GetComponent<HexComponent>().GetVerticalOffset(0,0);

        GameObject structureGO = Instantiate(prefab, structurePosition, Quaternion.identity, myHexGO.transform);

        structure.player = player;
        player.AddStructure(structure);
        structure.OnObjectDestroyed += OnStructureDestroyed;
        structureToGameObjectMap.Add(structure, structureGO);

        if(OnStructureCreated != null)
        {
            OnStructureCreated(structure, structureGO);
        }
    }

    public void OnStructureDestroyed( MapObject mo )
    {
        GameObject go = structureToGameObjectMap[(SurfaceStructure)mo];
        structureToGameObjectMap.Remove((SurfaceStructure)mo);
        Destroy(go);
    }

}
