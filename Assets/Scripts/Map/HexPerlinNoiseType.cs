using UnityEngine;

public readonly struct HexPerlinNoiseType {

    public enum NoiseModifierFunction {
        None,
        Squared,
        SquareRoot
    }
    
    public enum NoiseApplicationType {
        None,
        Additive,
        Multiplicative
    }
    
    public HexPerlinNoiseType(  Hex.HEX_FLOAT_PARAMS hexParam, 
                                float noiseResolution, Vector2 noiseOffset, float noiseScale, float noiseValueOffset, 
                                float zeroRange, float zeroValue,
                                NoiseModifierFunction modifierFunction, NoiseApplicationType applicationType) {
        HexParam = hexParam;
        NoiseResolution = noiseResolution;
        NoiseOffset = noiseOffset;
        NoiseScale = noiseScale;
        NoiseValueOffset = noiseValueOffset;
        ZeroRange = zeroRange;
        ZeroValue = zeroValue;
        ModifierFunction = modifierFunction;
        ApplicationType = applicationType;
    }

    public Hex.HEX_FLOAT_PARAMS HexParam {get;}
    public float NoiseResolution {get;}
    public Vector2 NoiseOffset {get;}
    public float NoiseScale {get;}
    public float NoiseValueOffset {get;}

    public float ZeroRange {get;}
    public float ZeroValue {get;}

    public NoiseModifierFunction ModifierFunction {get;}
    public NoiseApplicationType ApplicationType {get;}
}