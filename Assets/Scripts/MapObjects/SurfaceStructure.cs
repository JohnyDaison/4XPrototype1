public abstract class SurfaceStructure : MapObject
{
    public Player player;

    override public void SetHex(Hex newHex)
    {
        if (Hex != null)
        {
            // Will a surface structure ever LEAVE a hex and enter a new one?
            Hex.RemoveSurfaceStructure(this);
        }

        base.SetHex(newHex);

        Hex.AddSurfaceStructure(this);
    }

    public abstract string GetNamePlateText();
    public abstract void DoTurn();
}