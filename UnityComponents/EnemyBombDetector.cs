using BomberKnight.Data;
using UnityEngine;

namespace BomberKnight.UnityComponents;

/// <summary>
/// Component to detect interaction from the player with the 
/// </summary>
internal class EnemyBombDetector : MonoBehaviour
{
    #region Members

    private bool _initialized;

    #endregion

    #region Properties

    /// <summary>
    /// Gets or sets on which collisions the bomb should explode.
    /// </summary>
    public CollisionData Data { get; set; }

    #endregion

    #region Unity Methods

    void Awake()
    {
        if (_initialized)
            return;
        _initialized = true;
        CircleCollider2D circleCollider = gameObject.AddComponent<CircleCollider2D>();
        circleCollider.isTrigger = true;
        circleCollider.radius /= 0.8f;
        gameObject.AddComponent<BoxCollider2D>().size = new Vector2(circleCollider.radius * 1.5f, circleCollider.radius * 1.5f);
        // Each bomb has a child object to detect the player, which is what we check here.
        if (transform.parent != null && transform.parent.GetComponent<EnemyBombDetector>())
            gameObject.layer = 13;
        else
            gameObject.layer = 19;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.name == "Knight" || other.name == "Herobox")
        {
            if (Data.ExplodeOnHero)
                TriggerExplosion();
            else
            {
                Physics2D.IgnoreCollision(other, GetComponent<CircleCollider2D>());
                Physics2D.IgnoreCollision(other, GetComponent<BoxCollider2D>());
            }
        }
        else if ((Data.ExplodeOnTerrain && other.gameObject.layer == 8) || (Data.ExplodeOnAttack && other.gameObject.layer == 17))
            TriggerExplosion();
        else if (other.gameObject.layer == 22 || other.gameObject.layer == 11)
        {
            Physics2D.IgnoreCollision(other, GetComponent<CircleCollider2D>());
            Physics2D.IgnoreCollision(other, GetComponent<BoxCollider2D>());
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.name == "Knight" || collision.collider.name == "Herobox")
        {
            if (Data.ExplodeOnHero)
                TriggerExplosion();
            else
            {
                Physics2D.IgnoreCollision(collision.collider, GetComponent<BoxCollider2D>());
                Physics2D.IgnoreCollision(collision.collider, GetComponent<CircleCollider2D>());
            }
        }
        else if ((Data.ExplodeOnTerrain && collision.collider.gameObject.layer == 8) || (Data.ExplodeOnAttack && collision.collider.gameObject.layer == 17))
            TriggerExplosion();
        else if (collision.collider.gameObject.layer == 22 || collision.collider.gameObject.layer == 11)
        {
            Physics2D.IgnoreCollision(collision.collider, GetComponent<CircleCollider2D>());
            Physics2D.IgnoreCollision(collision.collider, GetComponent<BoxCollider2D>());
        }
    }

    #endregion

    #region Methods

    private void TriggerExplosion()
    {
        if (GetComponent<EnemyBomb>() is EnemyBomb enemyBomb)
            enemyBomb.Explode();
        else
            transform.parent.GetComponent<EnemyBomb>().Explode();
    }

    #endregion
}
