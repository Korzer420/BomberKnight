using HutongGames.PlayMaker.Actions;
using KorzUtils.Helper;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BomberKnight.UnityComponents;

internal class DeepnestSpiderControl : MonoBehaviour
{
    #region Members

    private GameObject _spinner;
    private float _passedDirectionalTime = 0.1f;
    private bool _moveOut = true;
    private float _attackCooldown = 8f;
    private SpiderBossState _current;
    private Vector3[] _bombPositions = new Vector3[]
    {
        new(0f, 2f),
        new(1.41f, 1.41f),
        new(2f, 0f),
        new(1.41f, -1.41f),
        new(0f, -2f),
        new(-1.41f,-1.41f),
        new(-2f, 0f),
        new(-1.41f, 1.41f)
    };
    private List<GameObject> _bombs = new();

    #endregion

    #region Event handler

    private void HealthManager_Die(On.HealthManager.orig_Die orig, HealthManager self, float? attackDirection, AttackTypes attackType, bool ignoreEvasion)
    {
        orig(self, attackDirection, attackType, ignoreEvasion);
        if (self.gameObject == gameObject)
            GetComponent<ItemDropper>().PrepareDrop();
    }

    private void HealthManager_TakeDamage(On.HealthManager.orig_TakeDamage orig, HealthManager self, HitInstance hitInstance)
    {
        if (self.gameObject == gameObject)
        {
            // Remove knockback to prevent being pushed into the spikes.
            hitInstance.MagnitudeMultiplier = 0f;
            if (hitInstance.AttackType == AttackTypes.Spell)
                hitInstance.Multiplier = 0.8f;
        }
        orig(self, hitInstance);
    }

    #endregion

    #region Unity Methods

    void Start()
    {
        On.HealthManager.TakeDamage += HealthManager_TakeDamage;
        On.HealthManager.Die += HealthManager_Die;
        _spinner = new("Spinner");
        _spinner.transform.localPosition = transform.position;
        _spinner.transform.localScale = new(1f, 1f);
        for (int i = 0; i < 8; i++)
            _bombs.Add(CreateBomb());
    }

    void OnDestroy()
    {
        On.HealthManager.TakeDamage -= HealthManager_TakeDamage;
        On.HealthManager.Die -= HealthManager_Die;
    }

    void FixedUpdate()
    {
        // Prevent the spider from drowning
        if (transform.localPosition.y < 6f)
            transform.localPosition = new(transform.localPosition.x, 6f);
        if (_current != SpiderBossState.Attacking)
        {
            // Rotate
            if (_passedDirectionalTime > 0f)
            {
                _passedDirectionalTime += Time.deltaTime;
                _spinner.transform.eulerAngles += new Vector3(0f, 0f, Time.deltaTime * 35f);
                if (_passedDirectionalTime > 4f && UnityEngine.Random.Range(0, 50) == 0)
                    _passedDirectionalTime = -0.1f;
            }
            else
            {
                _passedDirectionalTime -= Time.deltaTime;
                _spinner.transform.eulerAngles -= new Vector3(0f, 0f, Time.deltaTime * 35f);
                if (_passedDirectionalTime < -4f && UnityEngine.Random.Range(0, 50) == 0)
                    _passedDirectionalTime = 0.1f;
            }
            _spinner.transform.position = transform.position;

            if (_current == SpiderBossState.PrepareAttack)
            {
                _moveOut = false;
                if (_bombs[0].transform.localPosition.y <= 0.2f)
                {
                    _current = SpiderBossState.Attacking;
                    GameManager.instance.StartCoroutine(Attack());
                    return;
                }
            }
            else
                _attackCooldown -= Time.deltaTime;
            for (int i = 0; i < 8; i++)
            {
                Vector3 move = _bombPositions[i];
                move *= Time.deltaTime * 3;
                if (_moveOut)
                    _bombs[i].transform.localPosition += move;
                else
                    _bombs[i].transform.localPosition -= move;
            }

            if (_bombs[0].transform.localPosition.y > 20f && _moveOut)
                _moveOut = false;
            else if (_bombs[0].transform.localPosition.y < 2f && !_moveOut)
                _moveOut = true;

            if (_attackCooldown <= 0f && Random.Range(0, 10) == 0)
            {
                _attackCooldown = 8f;
                _current = SpiderBossState.PrepareAttack;
            }
        }
    }

    #endregion

    private IEnumerator Attack()
    {
        int rolledAttack = Random.Range(0, 10);
        int bombCount = _bombs.Count(x => x.activeSelf);
        foreach (GameObject bomb in _bombs.ToArray())
            GameObject.Destroy(bomb);

        if (rolledAttack <= 3)
        {
            // Take player as target and throw the bombs in 1 seconds interval in their direction.
            int counter = 0;
            while (counter < bombCount)
            {
                counter++;
                Vector3 direction = HeroController.instance.transform.position - transform.position;
                direction.Normalize();
                direction *= 25f;
                GameObject enemyBomb = CreateBomb(true);
                enemyBomb.SetActive(true);
                yield return null;
                enemyBomb.GetComponent<Rigidbody2D>().AddForce(direction, ForceMode2D.Impulse);
                yield return new WaitForSeconds(1f);
            }
        }
        else if (rolledAttack <= 6)
        {
            for (int i = 0; i < bombCount; i++)
            {
                GameObject bomb = CreateBomb(true);
                bomb.GetComponent<EnemyBomb>().FlingData = new()
                {
                    SourcePosition = transform,
                    Offset = new(0f, 2f),
                    FlingConfig = new()
                    {
                        AngleMin = 0f,
                        AngleMax = 360f,
                        Object = bomb,
                        SpeedMin = 20f,
                        SpeedMax = 40f
                    }
                };
                bomb.SetActive(true);
                yield return new WaitForSeconds(0.75f);
            }
        }
        else
        {
            int start = Random.Range(0, 8);
            for (int i = 0; i < bombCount; i++)
            {
                int current = start + i;
                if (current > 7)
                    current -= 8;
                GameObject enemyBomb = CreateBomb(true);
                enemyBomb.SetActive(true);
                yield return null;
                enemyBomb.GetComponent<Rigidbody2D>().AddForce(_bombPositions[current] * 10f, ForceMode2D.Impulse);
                if (rolledAttack == 9)
                    yield return new WaitForSeconds(1f);
            }
        }

        _bombs.Clear();
        for (int i = 0; i < 8; i++)
            _bombs.Add(CreateBomb());
        _moveOut = true;
        _current = SpiderBossState.Spin;
    }

    private GameObject CreateBomb(bool isAttack = false)
    {
        GameObject enemyBomb = new("Spiderbomb");
        enemyBomb.SetActive(false);
        if (isAttack)
            enemyBomb.transform.localPosition = transform.position;
        else
        {
            enemyBomb.transform.SetParent(_spinner.transform);
            enemyBomb.transform.localPosition = Vector3.zero;
        }
        enemyBomb.transform.localScale = new(2f, 2f);
        EnemyBomb projectile = enemyBomb.AddComponent<EnemyBomb>();
        projectile.CollisionBehaviour = new()
        {
            ExplodeOnAttack = true,
            ExplodeOnHero = true,
            ExplodeOnTerrain = isAttack,
        };
        projectile.WithGravity = isAttack;
        projectile.Tick = isAttack;
        projectile.ExplosionColor = new(0.8f, 0f, 0.2f);
        enemyBomb.SetActive(!isAttack);
        return enemyBomb;
    }
}

internal enum SpiderBossState
{
    Spin,

    PrepareAttack,

    Attacking
}
