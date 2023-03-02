using BomberKnight.Enums;
using KorzUtils.Helper;
using System.Collections.Generic;
using UnityEngine;

namespace BomberKnight.UnityComponents;

internal class BombPickup : MonoBehaviour
{
    private int _passedSteps = 0;

    private float _yPosition = 0f;
    
    public List<BombType> Bombs { get; set; } = new();

    public GameObject Second { get; set; }

    public GameObject Third { get; set; }

    void Start()
    {
        _yPosition = transform.position.y;
        if (Bombs.Count > 1)
        {
            Second = new("Second bomb");
            Second.AddComponent<SpriteRenderer>().sprite = SpriteHelper.CreateSprite<BomberKnight>("Sprites.BombSprite");
            Second.GetComponent<SpriteRenderer>().color = Bombs[1] switch
            {
                BombType.GrassBomb => Color.green,
                BombType.SporeBomb => new(1f, 0.4f, 0f),
                BombType.GoldBomb => Color.yellow,
                BombType.EchoBomb => new(1f, 0f, 1f),
                BombType.BounceBomb => Color.white,
                _ => Color.cyan
            };
            Second.GetComponent<SpriteRenderer>().sortingLayerID = 1;
            Second.layer = 1;
            Second.transform.SetParent(transform);
            Second.transform.localScale = new(1f, 1f);
            Second.transform.localPosition = new(0.24f, 0f, -1f);

            if (Bombs.Count > 2)
            {
                Third = new("Third bomb");
                Third.AddComponent<SpriteRenderer>().sprite = SpriteHelper.CreateSprite<BomberKnight>("Sprites.BombSprite");
                Third.GetComponent<SpriteRenderer>().color = Bombs[2] switch
                {
                    BombType.GrassBomb => Color.green,
                    BombType.SporeBomb => new(1f, 0.4f, 0f),
                    BombType.GoldBomb => Color.yellow,
                    BombType.EchoBomb => new(1f, 0f, 1f),
                    BombType.BounceBomb => Color.white,
                    _ => Color.cyan
                };
                Third.transform.SetParent(transform);
                Third.transform.localScale = new(1f, 1f);
                Third.transform.localPosition = new(.12f, .24f, -1.2f);
            }
        }
        GetComponent<SpriteRenderer>().color = Bombs[0] switch
         {
             BombType.GrassBomb => Color.green,
             BombType.SporeBomb => new(1f, 0.4f, 0f),
             BombType.GoldBomb => Color.yellow,
             BombType.EchoBomb => new(1f, 0f, 1f),
             BombType.BounceBomb => Color.white,
             BombType.MiningBomb => Color.red,
             _ => Color.cyan
         };
    }

    void FixedUpdate()
    {
        if (transform.position.y >= _yPosition - 0.02f)
            _passedSteps++;
        if (_passedSteps == 60)
            Destroy(GetComponent<Rigidbody2D>());
        _yPosition = transform.position.y;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.name == "Knight" || collision.gameObject.name == "HeroBox")
        {
            BombManager.GiveBombs(Bombs);
            GameObject.Destroy(gameObject);
        }
        else if (collision.collider.gameObject.layer == 22 || collision.collider.gameObject.layer == 11)
        {
            Physics2D.IgnoreCollision(collision.collider, GetComponent<CircleCollider2D>());
            Physics2D.IgnoreCollision(collision.collider, GetComponent<BoxCollider2D>());
        }
    }
}
