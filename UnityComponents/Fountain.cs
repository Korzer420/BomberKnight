using UnityEngine;

namespace BomberKnight.UnityComponents;

internal class Fountain : MonoBehaviour
{
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.name == "Knight")
        {
            HeroController.instance.TakeDamage(gameObject, GlobalEnums.CollisionSide.bottom, 1, 2);
            StartCoroutine(HeroController.instance.HazardRespawn());
        }
        else if (collision.gameObject.name.Contains("Bomb") && GetComponent<ItemDropper>() is ItemDropper itemDropper)
            itemDropper.PrepareDrop();
        else if (collision.gameObject.layer == 11 || collision.gameObject.layer == 26)
            Physics2D.IgnoreCollision(collision.collider, GetComponent<BoxCollider2D>());
    }
}
