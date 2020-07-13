using UnityEngine;

public readonly struct HexPerlinNoiseType {

    public HexPerlinNoiseType(Hex.HEX_FLOAT_PARAMS hexParam, float noiseResolution, Vector2 noiseOffset, float noiseScale, float noiseValueOffset) {
        HexParam = hexParam;
        NoiseResolution = noiseResolution;
        NoiseOffset = noiseOffset;
        NoiseScale = noiseScale;
        NoiseValueOffset = noiseValueOffset;
    }

    public Hex.HEX_FLOAT_PARAMS HexParam {get;}
    public float NoiseResolution {get;}
    public Vector2 NoiseOffset {get;}
    public float NoiseScale {get;}
    public float NoiseValueOffset {get;}
}