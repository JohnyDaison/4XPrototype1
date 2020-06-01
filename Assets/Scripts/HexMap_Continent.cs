using UnityEngine;

public class HexMap_Continent : HexMap {

    public GameObject mainCamera;

    override public void GenerateMap()
    {
        // First, call the base version to make all the hexes we need
        base.GenerateMap();

        int numContinents = 3;
        int continentSpacing = NumColumns / numContinents;

        /*  Interesting seeds
            1307101566 - start on the coast
            390098444 - start on peninsula
            1570814268 - a lot of mountains on perimeter
        */

        int seed = Random.Range(0, int.MaxValue);
        Random.InitState(seed);
        Debug.LogFormat("seed: {0}", seed);

        for (int c = 0; c < numContinents; c++)
        {
            // Make some kind of raised area
            int numSplats = Random.Range(4, 8);
            for (int i = 0; i < numSplats; i++)
            {
                int range = Random.Range(5, 8);
                int y = Random.Range(range, NumRows - range);
                int x = Random.Range(0, 10) - y/2 + (c * continentSpacing);

                ElevateArea(x, y, range);
            }

        }

        // Add lumpiness Perlin Noise?
        float noiseResolution = 0.01f;
        float maxCoordinate = Mathf.Max(NumColumns,NumRows);
        Vector2 noiseOffset = new Vector2( Random.Range(0f, 1f), Random.Range(0f, 1f) ); 

        float noiseScale = 2f;  // Larger values makes more islands (and lakes, I guess)


        for (int column = 0; column < NumColumns; column++)
        {
            for (int row = 0; row < NumRows; row++)
            {
                Hex h = GetHexAt(column, row);
                float n = 
                    Mathf.PerlinNoise( ((column/maxCoordinate) / noiseResolution) + noiseOffset.x,
                        ((row/maxCoordinate) / noiseResolution) + noiseOffset.y )
                    - 0.5f;
                h.Elevation += n * noiseScale;
            }
        }

        // Simulate rainfall/moisture (probably just Perlin it for now) and set plains/grasslands + forest 
        noiseResolution = 0.05f;
        noiseOffset = new Vector2( Random.Range(0f, 1f), Random.Range(0f, 1f) ); 

        noiseScale = 2f;  // Larger values makes more islands (and lakes, I guess)


        for (int column = 0; column < NumColumns; column++)
        {
            for (int row = 0; row < NumRows; row++)
            {
                Hex h = GetHexAt(column, row);
                float n = 
                    Mathf.PerlinNoise( ((float)column/Mathf.Max(NumColumns,NumRows) / noiseResolution) + noiseOffset.x, 
                        ((float)row/Mathf.Max(NumColumns,NumRows) / noiseResolution) + noiseOffset.y )
                    - 0.5f;
                h.Moisture = n * noiseScale;
            }
        }


        // Now make sure all the hex visuals are updated to match the data.

        UpdateHexVisuals();

        // foreach (Player p in Players) {
        //     CreateStartingUnit(p);
        // }

        CreateStartingUnit(CurrentPlayer);
    }

    void CreateStartingUnit(Player player) {
        Unit unit = new Unit(Unit.UNIT_TYPE.HUMAN, UnitHumanPrefab);

        // For development, turn on CanBuildCities on this unit
        unit.CanBuildCities = true;

        int startQ, startR;

        do {
            startQ = Random.Range(0, NumColumns);
            startR = Random.Range(0, NumRows);
        }
        while (!IsValidStartingPosition(startQ, startR));

        SpawnUnitAt(unit, startQ, startR, player);

        if(player == CurrentPlayer) {
            PanCameraToPosition(startQ, startR);
        }
    }

    bool IsValidStartingPosition(int startQ, int startR) {
        int minWalkable = 3;

        Hex startHex = GetHexAt(startQ, startR);
        if (startHex.BaseMovementCost(false, false, false) != 1) {
            return false;
        }

        Hex[] neighbours = (Hex[])startHex.GetNeighbours();
        int walkableNeighbourCount = 0;

        foreach (Hex hex in neighbours) {
            if (hex.BaseMovementCost(false, false, false) > 0) {
                walkableNeighbourCount++;
            }
        }

        return walkableNeighbourCount >= minWalkable;
    }

    void PanCameraToPosition(int Q, int R) {
        Hex hex = GetHexAt(Q, R);
        mainCamera.GetComponent<CameraMotion>().PanToHex(hex);
    }

    void ElevateArea(int q, int r, int range, float centerHeight = .8f)
    {
        Hex centerHex = GetHexAt(q, r);

        Hex[] areaHexes = GetHexesWithinRangeOf(centerHex, range);

        foreach(Hex h in areaHexes)
        {
            //if(h.Elevation < 0)
                //h.Elevation = 0;
            
            h.Elevation = centerHeight * Mathf.Lerp( 1f, 0.25f, Mathf.Pow(Hex.Distance(centerHex, h) / range,2f) );
        }
    }

}
