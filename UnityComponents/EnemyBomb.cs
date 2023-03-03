using BomberKnight.Data;
using KorzUtils.Helper;
using UnityEngine;

namespace BomberKnight.UnityComponents;

internal class EnemyBomb : MonoBehaviour
{
    #region Members

    private bool _initialized;
    private float _passedTime = 0f;
    private float _passedMilestone = 0f;
    private SpriteRenderer _spriteRenderer;

    #endregion

    #region Properties

    /// <summary>
    /// Gets or sets on which collisions the bomb should explode immediatly.
    /// </summary>
    public CollisionData CollisionBehaviour { get; set; }

    /// <summary>
    /// Gets or sets the color of the explosion.
    /// </summary>
    public Color ExplosionColor { get; set; }

    /// <summary>
    /// Gets or sets wheter the bomb should start its detonation timer (3 seconds).
    /// </summary>
    public bool Tick { get; set; }

    public bool WithGravity { get; set; }

    public BombFlingData FlingData { get; set; }

    #endregion

    #region Unity Methods

    void Start()
    {
        if (_initialized)
            return;
        _initialized = true;
        _spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        _spriteRenderer.sprite = SpriteHelper.CreateSprite<BomberKnight>("Sprites.BombSprite");
        _spriteRenderer.color = ExplosionColor;
        // Let the bombs render before anything (besides the menu) to increase invisibity (This is the sorting layer of the full soul sprite)
        _spriteRenderer.sortingLayerID = -349214895;
        gameObject.AddComponent<EnemyBombDetector>().Data = CollisionBehaviour;

        GameObject child = new("Hero Detector");
        child.transform.SetParent(transform);
        child.transform.localPosition = Vector3.zero;
        child.transform.localScale = new(1f, 1f, 1f);
        child.AddComponent<EnemyBombDetector>().Data = CollisionBehaviour;
        child.SetActive(true);

        if (WithGravity)
        {
            gameObject.AddComponent<Rigidbody2D>().gravityScale = 0.2f;
            gameObject.GetComponent<Rigidbody2D>().mass = 1f;
            if (FlingData != null)
                FlingUtils.FlingObject(FlingData.FlingConfig, FlingData.SourcePosition, FlingData.Offset);
        }
    }

    void FixedUpdate()
    {
        if (Tick)
        {
            if ((_passedMilestone >= 0.5f && _passedTime < 1f)
                || (_passedMilestone >= .25f && _passedTime >= 1f && _passedTime <= 2f)
                || (_passedMilestone >= .125f && _passedTime > 2f))
            {
                _passedMilestone = 0f;
                _spriteRenderer.color = _spriteRenderer.color == ExplosionColor ? Color.red : ExplosionColor;
            }
            _passedMilestone += Time.deltaTime;
            _passedTime += Time.deltaTime;
            if (_passedTime >= 3f)
                Explode();
        }
    }

    #endregion

    #region Methods

    internal void Explode()
    {
        GameObject explosion = GameObject.Instantiate(Bomb.Explosion);
        Destroy(explosion.LocateMyFSM("damages_enemy"));
        ParticleSystem.MainModule settings = explosion.GetComponentInChildren<ParticleSystem>().main;
        settings.startColor = new ParticleSystem.MinMaxGradient(ExplosionColor);
        explosion.GetComponentInChildren<SpriteRenderer>().color = ExplosionColor != Color.white
            ? ExplosionColor
            : new(0f, 0f, 0f);
        explosion.GetComponentInChildren<SimpleSpriteFade>().fadeInColor = ExplosionColor != Color.white
            ? new(ExplosionColor.r, ExplosionColor.g, ExplosionColor.b, 0f)
            : new(0f, 0f, 0f, 0f);

        typeof(SimpleSpriteFade).GetField("normalColor", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).SetValue(explosion.GetComponentInChildren<SimpleSpriteFade>(), ExplosionColor != Color.white
            ? ExplosionColor
            : new(0f, 0f, 0f));
        explosion.transform.position = transform.position;
        explosion.transform.localScale = new(1.2f, 1.2f);
        explosion.SetActive(true);
        if (gameObject.name.StartsWith("Spider"))
            gameObject.SetActive(false);
        else
            Destroy(gameObject);
    } 

    #endregion
}
