using HutongGames.PlayMaker.Actions;
using UnityEngine;

namespace BomberKnight.UnityComponents;

internal class BombWall : MonoBehaviour
{
    internal delegate void ExplosionTrigger();

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
            Bombed?.Invoke();
            Destroy(gameObject);
        }
    }
}
