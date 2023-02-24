using UnityEngine;

namespace BomberKnight.UnityComponents;

internal class BombWall : MonoBehaviour
{
    internal delegate bool? ExplosionTrigger(string explosionName);

    internal event ExplosionTrigger Bombed;

    void Start()
    {
        if (GetComponent<Collider2D>() is null)
            gameObject.AddComponent<BoxCollider2D>();
    }

    void OnTriggerEnter2D(Collider2D coll)
    {
        if (coll.gameObject.name.Contains("Explosion"))
        {
            bool? shouldDestroy = Bombed?.Invoke(coll.gameObject.name);
            if (shouldDestroy == true)
                Destroy(gameObject);
        }
    }
}
