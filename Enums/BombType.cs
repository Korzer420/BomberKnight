namespace BomberKnight.Enums;

public enum BombType
{
    /// <summary>
    /// Default bomb, no special effect. Homing with Shape of Unn.
    /// </summary>
    GrassBomb,

    /// <summary>
    /// Deals less damage but leaves a spore cloud upon destruction. 
    /// </summary>
    SporeBomb,

    /// <summary>
    /// Spawns geo upon destruction. Deals damage based on your current geo. Enhanced by Fragile Greed.
    /// </summary>
    GoldBomb,

    /// <summary>
    /// Deals less damage but explodes three times.
    /// </summary>
    EchoBomb,

    /// <summary>
    /// Does 1 damage to enemies, but can boost the knight. Doesn't cause damage to the player.
    /// </summary>
    BounceBomb,

    /// <summary>
    /// Larger hitbox with increased damage. Strong enough to break one way walls. Takes 1 lifeblood and 5 bombs.
    /// </summary>
    PowerBomb,

    /// <summary>
    /// Quest bomb with a larger radius that can destroy the crystal.
    /// </summary>
    MiningBomb
}
