using KorzUtils.Helper;
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

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.name.Contains("Explosion"))
        {
            bool? shouldDestroy = Bombed?.Invoke(collision.gameObject.name);
            if (shouldDestroy == true)
                Destroy(gameObject);
        }
    }
}
