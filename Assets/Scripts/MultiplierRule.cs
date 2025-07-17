using UnityEngine.Serialization;

[System.Serializable]
public class MultiplierRule
{
    [FormerlySerializedAs("minLevel")]
    public int MinLevel;
    [FormerlySerializedAs("multiplier")]
    public double Multiplier;
}
