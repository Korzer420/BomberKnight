using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace BomberKnight.UnityComponents;

internal class SporeBombSequence : MonoBehaviour
{
    private float _passedTime = 0f;
    private bool _isYellow = true;
    private SpriteRenderer _renderer;

    public int Stage { get; set; }

    void Start() 
    { 
        _renderer = GetComponent<SpriteRenderer>();
        _renderer.color = Color.gray;
    }
    
    void OnTriggerEnter2D(Collider2D coll)
    {
        if (Stage == 0 && coll.gameObject.name.StartsWith("Knight Spore Cloud"))
        {
            GetComponent<SpriteRenderer>().color = Color.yellow;
            Stage++;
        }
        else if (Stage == 1 && coll.gameObject.name.Contains("Explosion"))
        {
            Bomb.FakeExplosion(transform.position, Color.yellow, new(1f, 1f));
            Stage++;
            gameObject.layer = 20; // Set to hero box
        }
    }

    void FixedUpdate()
    {
        if (Stage == 2)
        {
            _passedTime += Time.deltaTime;
            if (_passedTime >= 0.5f)
            {
                _renderer.color = _isYellow ? new(1f, 0.4f, 0f) : Color.yellow;
                _isYellow = !_isYellow;
                _passedTime = 0f;
            }
            transform.position = HeroController.instance.transform.position + new Vector3(1.5f, Mathf.Sin(500));
        }
    }

    void OnCollisionEnter2D(Collision2D coll)
    {
        if (Stage == 2 && (coll.gameObject.name.StartsWith("Vomit Glob") || coll.gameObject.name.StartsWith("Puddle Box")))
        {
            Stage++;
            GetComponent<ItemDropper>().PrepareDrop(true);
        }
    }
}
