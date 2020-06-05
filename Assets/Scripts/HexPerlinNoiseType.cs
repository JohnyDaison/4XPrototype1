using UnityEngine;

public readonly struct HexPerlinNoiseType {

    public HexPerlinNoiseType(Hex.HEX_FLOAT_PARAMS hexParam, float noiseResolution, Vector2 noiseOffset, float noiseScale) {
        HexParam = hexParam;
        NoiseResolution = noiseResolution;
        NoiseOffset = noiseOffset;
        NoiseScale = noiseScale;
    }

    public Hex.HEX_FLOAT_PARAMS HexParam {get;}
    public float NoiseResolution {get;}
    public Vector2 NoiseOffset {get;}
    public float NoiseScale {get;}
}