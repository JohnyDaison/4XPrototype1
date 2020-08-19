using System.Collections.Generic;
using UnityEngine;

public class HexMap_Continent : HexMap {

    public GameObject mainCamera;

    public List<HexPerlinNoiseType> hexNoiseTypes = new List<HexPerlinNoiseType>();

    override public void StartGame() {
        base.StartGame();
    }

    override public void GenerateMap()
    {
        // First, call the base version to make all the hexes we need
        base.GenerateMap();

        /*  Interesting seeds
            1307101566 - start on the coast
            390098444 - start on peninsula
            1570814268 - a lot of mountains on perimeter
            1175914183 - nice terrain
            1791841673 - interesting terrain
            1879764718 - dynamic meshes testing
        */

        int seed = 1879764718;
        Random.InitState(seed);
        Debug.LogFormat("seed: {0}", seed);

        CreateContinents(3);

        DefineHexNoiseTypes();

        ApplyHexNoiseTypes();

        // Now make sure all the hex visuals are updated to match the data.

        UpdateHexVisuals();

        // foreach (Player p in Players) {
        //     CreateStartingUnit(p);
        // }

        CreateStartingUnit(CurrentPlayer);
    }

    void CreateContinents(int numContinents) {
        int continentSpacing = NumColumns / numContinents;

        // Set base ocean elevation
        for (int column = 0; column < NumColumns; column++)
        {
            for (int row = 0; row < NumRows; row++)
            {
                Hex hex = GetHexAt(column, row);
                hex.Elevation = -0.5f;
            }
        }

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
    }

    void DefineHexNoiseTypes() {
        // TODO: These noises have been tuned to 60x30 world map. They should be scaled with number of hexes or with hex diameter.
        if(hexNoiseTypes.Count > 0) {
            return;
        }
        
        hexNoiseTypes.Add(new HexPerlinNoiseType(
            Hex.HEX_FLOAT_PARAMS.Elevation, 
            0.01f, 
            new Vector2( Random.Range(0f, 1f), Random.Range(0f, 1f) ), 
            2f,
            -1f));

        hexNoiseTypes.Add(new HexPerlinNoiseType(
            Hex.HEX_FLOAT_PARAMS.Moisture, 
            0.05f, 
            new Vector2( Random.Range(0f, 1f), Random.Range(0f, 1f) ), 
            1f,
            0f));

        hexNoiseTypes.Add(new HexPerlinNoiseType(
            Hex.HEX_FLOAT_PARAMS.IronOre, 
            0.01f, 
            new Vector2( Random.Range(0f, 1f), Random.Range(0f, 1f) ), 
            1f,
            0));
    }

    void ApplyHexNoiseTypes() {
        float maxCoordinate = Mathf.Max(NumColumns,NumRows);

        for (int column = 0; column < NumColumns; column++)
        {
            for (int row = 0; row < NumRows; row++)
            {
                Hex hex = GetHexAt(column, row);

                hexNoiseTypes.ForEach((HexPerlinNoiseType noiseType) => {
                    float noiseValue = 
                        Mathf.PerlinNoise( ((column/maxCoordinate) / noiseType.NoiseResolution) + noiseType.NoiseOffset.x,
                            ((row/maxCoordinate) / noiseType.NoiseResolution) + noiseType.NoiseOffset.y );

                     // According to documentation, values slightly out range can happen, so prevent it
                    noiseValue = Mathf.Clamp(noiseValue, 0f, 1f);

                    hex.floatParams[noiseType.HexParam] += (noiseValue * noiseType.NoiseScale) + noiseType.NoiseValueOffset;
                });
            }
        }
    }

    void CreateStartingUnit(Player player) {
        UnitType unitType = GameController.instance.UnitTypeDB.GetTypeById("human1");
        Unit unit = new Unit(unitType, UnitHumanPrefab);

        int startQ, startR;
        int tries = 0, maxTries = 100;
        bool positionFound = false;

        do {
            startQ = Random.Range(0, NumColumns);
            startR = Random.Range(0, NumRows);
            tries++;
            positionFound = IsValidStartingPosition(startQ, startR);
        }
        while (!positionFound && tries <= maxTries);

        if (positionFound)
        {
            SpawnUnitAt(unit, startQ, startR, player);

            if(player == CurrentPlayer) {
                PanCameraToPosition(startQ, startR);
            }
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
        float minHeightCoef = 0.25f;
        float maxHeightCoef = 1f;
        
        Hex centerHex = GetHexAt(q, r);

        Hex[] areaHexes = GetHexesWithinRangeOf(centerHex, range);

        foreach(Hex hex in areaHexes)
        {
            //if(h.Elevation < 0)
                //h.Elevation = 0;
            
            float distanceRatio = Hex.Distance(centerHex, hex) / range;
            float distanceCoef = Mathf.Pow(distanceRatio, 2f);

            hex.Elevation = centerHeight * Mathf.Lerp( maxHeightCoef, minHeightCoef, distanceCoef);
        }
    }

}
