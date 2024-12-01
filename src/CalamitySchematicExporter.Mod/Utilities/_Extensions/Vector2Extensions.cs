using System.Runtime.CompilerServices;

namespace CalamitySchematicExporter.Utilities;

/// <summary>
///     Provides <see cref="Vector2"/> extension methods.
/// </summary>
public static class Vector2Extensions
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="vector"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 SnapToTileCoordinates(this Vector2 vector) {
        return (vector / 16f).Floor();
    }
}