using ItemChanger.Internal;
using System.Reflection;

namespace BomberKnight;

internal static class SoundEffectManager
{
    public static SoundManager Manager { get; set; }

    static SoundEffectManager()
    {
        Manager = new(Assembly.GetExecutingAssembly(), "BomberKnight.Resources.Sounds.");
    }
}
