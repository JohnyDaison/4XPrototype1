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
            807093867 - dynamic meshes testing
            951680847 - shore testing
        */

        int seed = Random.Range(0, int.MaxValue);
        Random.InitState(seed);
        Debug.LogFormat("seed: {0}", seed);

        CreateContinents(3);

        DefineHexNoiseTypes();

        ApplyHexNoiseTypes();

        CalculateHexProperties();

        CalculateShallowWater();

        CalculateRoughness();

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
                hex.Elevation = -0.5f * HexMap.ElevationScale;
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
                x = (x+NumColumns) % NumColumns;

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
            2f * HexMap.ElevationScale,
            -1f * HexMap.ElevationScale,
            0f,
            0f,
            HexPerlinNoiseType.NoiseModifierFunction.None,
            HexPerlinNoiseType.NoiseApplicationType.Additive));

        hexNoiseTypes.Add(new HexPerlinNoiseType(
            Hex.HEX_FLOAT_PARAMS.Elevation, 
            0.1f, 
            new Vector2( Random.Range(0f, 10f), Random.Range(0f, 10f) ), 
            8f * HexMap.ElevationScale,
            0f,
            6f * HexMap.ElevationScale,
            0f,
            HexPerlinNoiseType.NoiseModifierFunction.SquareRoot,
            HexPerlinNoiseType.NoiseApplicationType.Additive));

        hexNoiseTypes.Add(new HexPerlinNoiseType(
            Hex.HEX_FLOAT_PARAMS.Moisture, 
            0.05f, 
            new Vector2( Random.Range(0f, 1f), Random.Range(0f, 1f) ), 
            1f,
            0f,
            0f,
            0f,
            HexPerlinNoiseType.NoiseModifierFunction.None,
            HexPerlinNoiseType.NoiseApplicationType.Additive));

        hexNoiseTypes.Add(new HexPerlinNoiseType(
            Hex.HEX_FLOAT_PARAMS.IronOre, 
            0.01f, 
            new Vector2( Random.Range(0f, 1f), Random.Range(0f, 1f) ), 
            1f,
            0f,
            0f,
            0f,
            HexPerlinNoiseType.NoiseModifierFunction.None,
            HexPerlinNoiseType.NoiseApplicationType.Additive));

        hexNoiseTypes.Add(new HexPerlinNoiseType(
            Hex.HEX_FLOAT_PARAMS.XOffset, 
            1f, 
            new Vector2( Random.Range(0f, 1f), Random.Range(0f, 1f) ), 
            0.4f,
            -0.2f,
            0f,
            0f,
            HexPerlinNoiseType.NoiseModifierFunction.None,
            HexPerlinNoiseType.NoiseApplicationType.Additive));

        hexNoiseTypes.Add(new HexPerlinNoiseType(
            Hex.HEX_FLOAT_PARAMS.ZOffset, 
            1f, 
            new Vector2( Random.Range(0f, 1f), Random.Range(0f, 1f) ), 
            0.4f,
            -0.2f,
            0f,
            0f,
            HexPerlinNoiseType.NoiseModifierFunction.None,
            HexPerlinNoiseType.NoiseApplicationType.Additive));

        hexNoiseTypes.Add(new HexPerlinNoiseType(
            Hex.HEX_FLOAT_PARAMS.Roughness, 
            0.001f, 
            new Vector2( Random.Range(0f, 1f), Random.Range(0f, 1f) ), 
            1f,
            -0.2f,
            0f,
            0f,
            HexPerlinNoiseType.NoiseModifierFunction.None,
            HexPerlinNoiseType.NoiseApplicationType.None));
    }

    void ApplyHexNoiseTypes() {
        float maxCoordinate = Mathf.Max(NumColumns,NumRows);

        for (int column = 0; column < NumColumns; column++)
        {
            for (int row = 0; row < NumRows; row++)
            {
                Hex hex = GetHexAt(column, row);

                hexNoiseTypes.ForEach((HexPerlinNoiseType noiseType) => {
                    if (noiseType.ApplicationType == HexPerlinNoiseType.NoiseApplicationType.Additive) {
                        hex.floatParams[noiseType.HexParam] += SampleNoiseType(noiseType, column/maxCoordinate, row/maxCoordinate);
                    } else if (noiseType.ApplicationType == HexPerlinNoiseType.NoiseApplicationType.Multiplicative) {
                        hex.floatParams[noiseType.HexParam] *= SampleNoiseType(noiseType, column/maxCoordinate, row/maxCoordinate);
                    }
                });
            }
        }
    }

    void CalculateRoughness() {
        for (int column = 0; column < NumColumns; column++)
        {
            for (int row = 0; row < NumRows; row++)
            {
                Hex hex = GetHexAt(column, row);

                if (hex.ElevationType == Hex.ELEVATION_TYPE.MOUNTAIN) {
                    hex.floatParams[Hex.HEX_FLOAT_PARAMS.Roughness] = HexMesh.hexTriangleSide * (0.5f /* + 0.1f * (hex.floatParams[Hex.HEX_FLOAT_PARAMS.Elevation] - HeightMountain) / HexMap.ElevationScale */);
                }
            }
        }
    }

    public List<HexPerlinNoiseType> GetNoiseTypesByParam(Hex.HEX_FLOAT_PARAMS hexParam) {
        return hexNoiseTypes.FindAll((HexPerlinNoiseType noiseType) => {
            return noiseType.HexParam == hexParam;
        });
    }

    public float SampleNoiseType(HexPerlinNoiseType noiseType, float x, float y) {
        float noiseValue = Mathf.PerlinNoise(   (x / noiseType.NoiseResolution) + noiseType.NoiseOffset.x ,
                                                (y / noiseType.NoiseResolution) + noiseType.NoiseOffset.y );

        // According to documentation, values slightly out range can happen, so prevent it
        noiseValue = Mathf.Clamp(noiseValue, 0f, 1f);

        float mappedValue = (noiseValue * noiseType.NoiseScale) + noiseType.NoiseValueOffset;

        float zeroRangedValue = mappedValue;

        if (noiseType.ZeroRange > 0) {
            if (Mathf.Abs(mappedValue) < noiseType.ZeroRange) {
                zeroRangedValue = Mathf.Sign(mappedValue) * noiseType.ZeroValue;
            } else {
                float cutValue = mappedValue - Mathf.Sign(mappedValue) * noiseType.ZeroRange;
                zeroRangedValue = Mathf.Sign(mappedValue) * noiseType.ZeroValue + cutValue;
            }
        }
        
        float finalValue = zeroRangedValue;

        if (noiseType.ModifierFunction == HexPerlinNoiseType.NoiseModifierFunction.Squared) {
            finalValue = Mathf.Pow(finalValue, 2f);
        } else if (noiseType.ModifierFunction == HexPerlinNoiseType.NoiseModifierFunction.SquareRoot) {
            finalValue = Mathf.Sqrt(finalValue);
        }

        return finalValue;
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
        Debug.Log($"Elevate area at {q},{r}; range {range}");
        float minHeightCoef = 0.25f * HexMap.ElevationScale;
        float maxHeightCoef = 1f * HexMap.ElevationScale;
        
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
