/// <summary>
/// One landscape feature of the terrain (plains, plateaus, dunes…): changes the height of a point and says what its
/// ground is made of. Must be thread-safe (the chunks are generated in the background) and deterministic.
/// </summary>
public interface ITerrainFeature
{
    void Apply(ref TerrainPoint p);
}
