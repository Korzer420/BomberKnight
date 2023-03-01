using BomberKnight.Enums;
using ItemChanger.Extensions;
using ItemChanger.FsmStateActions;
using KorzUtils.Enums;
using KorzUtils.Helper;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using Color = UnityEngine.Color;

namespace BomberKnight.UnityComponents;

/// <summary>
/// A bomb gameobject that... well explodes.
/// </summary>
public class Bomb : MonoBehaviour
{
    #region Event Data

    public delegate void BombTrigger(BombEventArgs bombEventArgs);

    /// <summary>
    /// Fired when the bomb is placed before it's type is processed.
    /// </summary>
    public static event BombTrigger BombSpawned;

    /// <summary>
    /// Fired right after the explosion is activated and before the bomb object is removed.
    /// </summary>
    public static event BombTrigger BombExploded;

    #endregion

    #region Members

    private static GameObject _cloud;
    private bool _isHoming;
    private Rigidbody2D _rigidBody;
    private int _echoStack = 0;
    private bool _canDealContactDamage = false;

    #endregion

    #region Properties

    /// <summary>
    /// Gets or sets the explosion object which will be created upon destruction.
    /// </summary>
    internal static GameObject Explosion { get; set; }

    /// <summary>
    /// Gets or sets the time the bomb should take before exploding.
    /// </summary>
    internal static float FuseTime { get; set; } = 3f;

    /// <summary>
    /// Gets or sets the type of the bomb to apply special effects.
    /// </summary>
    public BombType Type { get; set; }

    /// <summary>
    /// Gets or sets if the bomb can cancel the fuse time time.
    /// </summary>
    public bool CanExplode { get; set; }

    public bool EnemyBomb { get; set; }

    /// <summary>
    /// Gets the cloud object
    /// </summary>
    public static GameObject Cloud => _cloud == null ? _cloud = GameObject.Find("_GameManager").transform.Find("GlobalPool/Knight Spore Cloud(Clone)").gameObject : _cloud;

    #endregion

    #region Unity Methods

    void Start() => StartCoroutine(Ticking());

    void OnCollisionEnter2D(Collision2D collision)
    {
        // Ignore hero.
        if (collision.gameObject.name == "Knight" || (collision.gameObject.layer == 8 && _isHoming))
            Physics2D.IgnoreCollision(collision.collider, GetComponent<CircleCollider2D>());
        else if ((_canDealContactDamage || CharmHelper.EquippedCharm(CharmRef.Grubsong)) && collision.gameObject.GetComponent<HealthManager>() is HealthManager enemy)
        {
            if (CharmHelper.EquippedCharm(CharmRef.Grubsong))
                HeroController.instance.AddMPCharge(2);
            if (_canDealContactDamage)
            {
                int damage = PlayerData.instance.GetInt(nameof(PlayerData.instance.nailDamage));
                if (Type == BombType.PowerBomb)
                    damage *= 2;
                enemy.Hit(new()
                {
                    AttackType = AttackTypes.Generic,
                    DamageDealt = damage,
                    IgnoreInvulnerable = true,
                    Source = gameObject,
                    MagnitudeMultiplier = 0f
                });

                // 5% chance to explode immediatly.
                if (UnityEngine.Random.Range(0, 20) == 0)
                    CanExplode = true;
                else
                {
                    _canDealContactDamage = false;
                    StartCoroutine(DamageCooldown());
                }
            }
        }
    }

    #endregion

    #region Methods

    private IEnumerator Ticking()
    {
        BombSpawned?.Invoke(new(Type, transform.localPosition));
        float passedTime = 0f;
        float passedMilestone = 0f; // Used to blink faster over time.
        Color bombColor = BombManager.GetBombColor(Type);
        Color currentColor = bombColor;
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.color = currentColor;

        (GameObject, float) homingData = CheckForHoming();
        _canDealContactDamage = CharmHelper.EquippedCharm(CharmRef.ThornsOfAgony) && !EnemyBomb;
        FuseTime = CharmHelper.EquippedCharm(CharmRef.DeepFocus) && EnemyBomb ? 6f : 3f;

        while (passedTime < FuseTime && !CanExplode)
        {
            if ((passedMilestone >= 0.5f && passedTime < 1f)
                || (passedMilestone >= .25f && passedTime >= 1f && passedTime <= 2f)
                || (passedMilestone >= .125f && passedTime > 2f))
            {
                passedMilestone = 0f;
                currentColor = currentColor == bombColor ? Color.red : bombColor;
                spriteRenderer.color = currentColor;
            }
            passedMilestone += Time.deltaTime;
            passedTime += Time.deltaTime;

            Homing(homingData);
            yield return null;
        }

        // Boom
        Explode(bombColor);

        GameManager.instance.StartCoroutine(FixGround());
        if (Type != BombType.EchoBomb)
            GameObject.Destroy(gameObject);
        else
        {
            spriteRenderer.color = new(0f, 0f, 0f, 0f);
            StartCoroutine(Repeat());
        }
    }

    private static IEnumerator FixGround()
    {
        yield return new WaitForSeconds(0.2f);
        PlayMakerFSM.BroadcastEvent("VANISHED");
    }

    private IEnumerator Repeat()
    {
        for (_echoStack = 1; _echoStack < 5; _echoStack++)
        {
            GetComponent<SpriteRenderer>().color = new(1f, 0f, 1f, 1f - 0.2f * _echoStack);
            float passedTime = 0f;
            while(passedTime < 3f)
            {
                passedTime += Time.deltaTime;
                yield return new WaitUntil(() => !GameManager.instance.IsGamePaused());
            }
            Explode(new(1f, 0f, 1f));
        }
        GameObject.Destroy(gameObject);
    }

    /// <summary>
    /// Check if the bomb should move to an enemy.
    /// </summary>
    private (GameObject, float) CheckForHoming()
    {
        GameObject enemyToChase = null;
        float homingSpeed = 10f;
        if (CharmHelper.EquippedCharm(CharmRef.Dashmaster))
            homingSpeed += 5f;
        if (CharmHelper.EquippedCharm(CharmRef.Sprintmaster))
            homingSpeed += 5f;
        // If both are equipped it is increased even further.
        if (homingSpeed == 20f)
            homingSpeed += 22.5f;
        if (!EnemyBomb && CharmHelper.EquippedCharm(CharmRef.GatheringSwarm))
        {
            _rigidBody = GetComponent<Rigidbody2D>();

            HealthManager[] enemies = GameObject.FindObjectsOfType<HealthManager>();
            if (enemies != null && enemies.Any())
            {
                // Only enemies less than 100 units away can be targeted. Pick the nearest.
                float nearestDistance = 100f;
                foreach (HealthManager enemy in enemies)
                {
                    float distance = (enemy.transform.position - transform.localPosition).sqrMagnitude;
                    if (distance <= nearestDistance)
                    {
                        nearestDistance = distance;
                        enemyToChase = enemy.gameObject;
                        _rigidBody.mass = 1f;
                        _rigidBody.gravityScale = 0f;
                    }
                }
            }
        }
        _isHoming = enemyToChase != null;
        return new(enemyToChase, homingSpeed);
    }

    /// <summary>
    /// Move the bomb slowly to the passed target.
    /// </summary>
    private void Homing((GameObject, float) homingData)
    {
        if (homingData.Item1 == null)
            return;
        transform.position = Vector3.MoveTowards(transform.position, homingData.Item1.transform.position, homingData.Item2 * Time.deltaTime);

        if (transform.position.x < homingData.Item1.transform.position.x)
            transform.SetRotationZ(transform.localEulerAngles.z + 240f * Time.deltaTime);
        else
            transform.SetRotationZ(transform.localEulerAngles.z - 240f * Time.deltaTime);
    }

    private void Explode(Color bombColor)
    {
        GameObject explosion = GameObject.Instantiate(Explosion);
        explosion.name = Type + " Explosion";
        explosion.SetActive(false);

        // Color explosion
        ParticleSystem.MainModule settings = explosion.GetComponentInChildren<ParticleSystem>().main;
        settings.startColor = new ParticleSystem.MinMaxGradient(bombColor);
        explosion.GetComponentInChildren<SpriteRenderer>().color = bombColor != Color.white
            ? bombColor
            : new(0f, 0f, 0f);
        explosion.GetComponentInChildren<SimpleSpriteFade>().fadeInColor = bombColor != Color.white
            ? new(bombColor.r, bombColor.g, bombColor.b, 0f)
            : new(0f, 0f, 0f, 0f);

        typeof(SimpleSpriteFade).GetField("normalColor", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).SetValue(explosion.GetComponentInChildren<SimpleSpriteFade>(), bombColor != Color.white
            ? bombColor
            : new(0f, 0f, 0f));

        CalculateDamage(explosion);

        explosion.transform.localPosition = transform.localPosition;
        explosion.transform.localScale = Type switch
        {
            BombType.PowerBomb => new(2f, 2f, explosion.transform.localScale.z),
            BombType.BounceBomb => new(.75f, .75f, explosion.transform.localScale.z),
            _ => new(1.2f, 1.2f, explosion.transform.localScale.z)
        };

        if (!EnemyBomb && CharmHelper.EquippedCharm(CharmRef.DeepFocus))
            explosion.transform.localScale *= 1.5f;

        explosion.GetComponent<CircleCollider2D>().isTrigger = true;
        explosion.AddComponent<Rigidbody2D>().gravityScale = 0f;

        if (Type == BombType.GoldBomb)
        {
            bool hasGreed = CharmHelper.EquippedCharm(CharmRef.FragileGreed);
            FlingGeoAction.SpawnGeo(UnityEngine.Random.Range(1, hasGreed ? 100 : 25), hasGreed ? 10 : 5, 0, ItemChanger.FlingType.Everywhere, explosion.transform);
        }
        else if (Type == BombType.SporeBomb)
            GameManager.instance.StartCoroutine(SporeCloud());

        explosion.SetActive(true);
        BombExploded?.Invoke(new(Type, transform.localPosition));
        if (Type == BombType.PowerBomb)
            PowerExplosion();
        else
            PlayMakerFSM.BroadcastEvent("BOMBED");
    }

    private void CalculateDamage(GameObject explosion)
    {
        float damage = Type switch
        {
            BombType.GrassBomb => 20,
            BombType.GoldBomb => Mathf.Min(CharmHelper.EquippedCharm(CharmRef.UnbreakableGreed)
            ? 150 : (CharmHelper.EquippedCharm(CharmRef.FragileGreed) ? 100 : 50),
            PlayerData.instance.GetInt("geo") / 100 + 5),
            BombType.BounceBomb => 1,
            BombType.PowerBomb => 40,
            BombType.EchoBomb => 10 * (1 - _echoStack * .2f),
            BombType.MiningBomb => 30,
            _ => 10
        };

        // Bonus damage with shaman stone
        if (CharmHelper.EquippedCharm(CharmRef.ShamanStone))
        {
            damage *= 1.3f;
            // Bombs also count as spell damage if shaman stone is equipped.
            explosion.LocateMyFSM("damages_enemy").FsmVariables.FindFsmInt("attackType").Value = (int)AttackTypes.Spell;
        }

        explosion.LocateMyFSM("damages_enemy").FsmVariables.FindFsmInt("damageDealt").Value = Convert.ToInt32(damage);
    }

    private IEnumerator SporeCloud()
    {
        GameObject newCloud = GameObject.Instantiate(Cloud, transform.position,
                Quaternion.identity);
        newCloud.SetActive(true);
        yield return new WaitForSeconds(4.1f);
        GameObject.Destroy(newCloud);
    }

    private IEnumerator DamageCooldown()
    {
        float passedTime = 0f;
        while (passedTime < 0.5f)
        {
            passedTime += Time.deltaTime;
            yield return new WaitUntil(() => !GameManager.instance.IsGamePaused());
        }
        _canDealContactDamage = true;
    }

    /// <summary>
    /// "Tries" to break all breakable walls and floors.
    /// </summary>
    private void PowerExplosion()
    {
        string scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        PlayMakerFSM.BroadcastEvent("POWERBOMBED");
        foreach (Breakable breakable in GameObject.FindObjectsOfType<Breakable>())
            breakable.Break(70f, 110f, 3f);
        if (scene == "Tutorial_01")
        {
            // Big door.
            GameObject current = GameObject.Find("_Props/Hallownest_Main_Gate/Door");
            if (current != null)
            {
                current.LocateMyFSM("Great Door").GetState("Idle").AdjustTransition("HIT", "Break");
                current.LocateMyFSM("Great Door").SendEvent("HIT");
            }
            // Big breakable floor
            current = GameObject.Find("_Props/Collapser Tute 01");
            if (current != null)
                current.LocateMyFSM("collapse tute").SendEvent("BREAK");
        }
        // Room next to crossroads stag.
        else if (scene == "Crossroads_03")
        {
            // Grub wall.
            GameObject current = GameObject.Find("_Scenery/Break Wall 2");
            current?.LocateMyFSM("FSM").SendEvent("SPELL");
            
        }
        // Aspid arena (Glowing Womb)
        else if (scene == "Crossroads_22")
        {
            GameObject current = GameObject.Find("infected_door");
            if (current != null)
                GameObject.Destroy(current);
        }
        // Aspid arena
        else if (scene == "Crossroads_08")
        {
            GameObject current = GameObject.Find("Break Wall 2");
            current?.LocateMyFSM("FSM").SendEvent("SPELL");
        }
        // Hive room with breakable wall.
        else if (scene == "Hive_03_c")
        {
            GameObject current = GameObject.Find("Break Floor 1");
            if (current != null)
            {
                current.LocateMyFSM("break_floor").GetState("Idle").AdjustTransition("NAIL HIT", "Break");
                current.LocateMyFSM("break_floor").SendEvent("NAIL HIT");
            }
        }
        // Mask Shard wall room.
        else if (scene == "Hive_04")
        {
            GameObject current = GameObject.Find("Hive Break Wall");
            current?.LocateMyFSM("Smash").SendEvent("HIT");
        }
        // Room left to archive.
        else if (scene == "Fungus3_02")
        {
            GameObject current = GameObject.Find("One Way Wall Exit");
            if (current != null)
                GameObject.Destroy(current);
        }
        // Right elevator
        else if (scene == "Ruins2_10b")
        {
            GameObject current = GameObject.Find("elev_break wall");
            if (current != null)
                GameObject.Destroy(current);
        }
        // Dung Defender exit.
        else if (scene == "Abyss_01")
        {
            GameObject current = GameObject.Find("dung_defender_wall");
            if (current != null)
                GameObject.Destroy(current);
        }
        else if (scene == "Crossroads_33")
        {
            GameObject current = GameObject.Find("_Props/full_wall_left");
            if (current != null)
                GameObject.Destroy(current);
        }
    }

    /// <summary>
    /// Creates a harmless explosion.
    /// </summary>
    public static void FakeExplosion(Vector3 position, Color color, Vector3 scale)
    {
        if (Vector3.Distance(HeroController.instance.transform.position, position) > 50)
            return;
        GameObject explosion = GameObject.Instantiate(Bomb.Explosion);
        explosion.name = "Fake Explosion";
        // Color explosion
        ParticleSystem.MainModule settings = explosion.GetComponentInChildren<ParticleSystem>().main;
        settings.startColor = new ParticleSystem.MinMaxGradient(color);
        explosion.GetComponentInChildren<SpriteRenderer>().color = color;
        explosion.GetComponentInChildren<SimpleSpriteFade>().fadeInColor = color;
        typeof(SimpleSpriteFade).GetField("normalColor", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
            .SetValue(explosion.GetComponentInChildren<SimpleSpriteFade>(), color);

        Destroy(explosion.GetComponent<DamageHero>());
        Destroy(explosion.LocateMyFSM("damages_enemy"));
        explosion.transform.position = position;
        explosion.transform.localScale = scale;
    }

    #endregion
}
