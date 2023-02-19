using UnityEngine;

namespace BomberKnight.UnityComponents;

internal class BombWall : MonoBehaviour
{
    public GameObject NewGate { get; set; }

    void OnTriggerEnter2D(Collider2D coll)
    {
        if (coll.gameObject.name.Contains("Explosion"))
        {
            NewGate.SetActive(true);
            Destroy(gameObject);
        }
    }
}
