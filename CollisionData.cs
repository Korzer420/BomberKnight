using BomberKnight.UnityComponents;

namespace BomberKnight;

/// <summary>
/// Contains information how a <see cref="EnemyBombDetector"/> should process collisions.
/// </summary>
internal class CollisionData
{
    #region Properties

    /// <summary>
    /// Gets or sets whether the bomb should explode immediatly upon contact with the player.
    /// </summary>
    public bool ExplodeOnHero { get; set; }

    /// <summary>
    /// Gets or sets whether the bomb should explode immediatly upon contact with the nail (or a spell?)
    /// </summary>
    public bool ExplodeOnAttack { get; set; }

    /// <summary>
    /// Gets or sets whether the bomb should explode immediatly upon contact with terrain (layer 7).
    /// </summary>
    public bool ExplodeOnTerrain { get; set; }

    #endregion
}