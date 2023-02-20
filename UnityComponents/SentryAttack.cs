using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BomberKnight.UnityComponents;

internal class SentryAttack : MonoBehaviour
{
    #region Members

    private List<Vector3> _wayPoints = new();

    #endregion

    public int StartPosition { get; set; }

    public bool FromLeft { get; set; }

    public MoveType Move { get; set; }

    void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.gameObject.name == "Knight")
        {
            GameManager.instance.StartCoroutine(Respawn(transform.position, !FromLeft));
            Destroy(gameObject);
        }
    }

    void Start()
    {
        transform.position = StartPosition switch
        {
            1 when FromLeft => new Vector3(5f, 19f),
            2 when FromLeft => new Vector3(5f, 21f),
            3 when FromLeft => new Vector3(5f, 23f),
            1 when !FromLeft => new(95f, 19f),
            2 when !FromLeft => new(95f, 21f),
            _ => new(95f, 23f)
        };
        if (Move == MoveType.Random)
            Move = (MoveType)UnityEngine.Random.Range(0, 4);

        switch (Move)
        {
            default:
            case MoveType.Straight:
                _wayPoints.Add(FromLeft ? new Vector3(95f, transform.localPosition.y) : new Vector3(5f, transform.localPosition.y));
                break;
            case MoveType.Diagonal:
                float height = transform.localPosition.y;
                float startWidth = transform.localPosition.x;
                bool up = height != 23;
                for (int i = 1; i <= 20; i++)
                {
                    height += up ? 2.5f : -2.5f;
                    if (height >= 23f)
                        up = false;
                    else if (height <= 18f)
                        up = true;
                    _wayPoints.Add(new(startWidth + ((FromLeft ? 1 : -1) * (i * 2.25f * StartPosition)), height));
                }
                break;
            case MoveType.Circles:
                transform.localPosition = new(transform.localPosition.x, 19f);
                Vector3 wayPoint = transform.localPosition;
                for (int i = 0; i < 8; i++)
                {
                    Vector3 circleStart = wayPoint;
                    // Create circle points
                    for (int j = 1; j < 12; j++)
                    {
                        if ((j > 3 && j < 7) || j > 9)
                        {
                            float maxOffset = (FromLeft ? 1 : -1) * 6f * StartPosition;
                            if (j <= 6)
                                _wayPoints.Add(new(circleStart.x + maxOffset - ((FromLeft ? 1 : -1) * (j - 3) * 2f * StartPosition), circleStart.y + j * 1.5f));
                            else
                                _wayPoints.Add(new(circleStart.x - maxOffset + ((FromLeft ? 1 : -1) * (j - 9) * 2f * StartPosition), circleStart.y + 9 - 1.5f * (j - 6)));
                        }
                        else
                        {
                            if (j <= 3)
                                _wayPoints.Add(new(circleStart.x + ((FromLeft ? 1 : -1) * j * 2f * StartPosition), circleStart.y + j * 1.5f));
                            else
                                _wayPoints.Add(new(circleStart.x - ((FromLeft ? 1 : -1) * (j - 6) * 2f * StartPosition), circleStart.y + 9 - 1.5f * (j - 6)));
                        }
                    }
                    _wayPoints.Add(circleStart);
                    if (FromLeft)
                    {
                        _wayPoints.Add(new(circleStart.x + StartPosition * 6, circleStart.y - 2f));
                        _wayPoints.Add(new(circleStart.x + StartPosition * 14, circleStart.y));
                    }
                    else
                    {
                        _wayPoints.Add(new(circleStart.x - StartPosition * 6, circleStart.y - 2f));
                        _wayPoints.Add(new(circleStart.x - StartPosition * 14, circleStart.y));
                    }
                    wayPoint = _wayPoints.Last();
                }
                break;
        }
    }

    void FixedUpdate()
    {
        transform.position = Vector3.MoveTowards(transform.position, _wayPoints[0], Time.deltaTime * 10f);
        transform.eulerAngles += new Vector3(0f, 0f, FromLeft ? -2 : 2f);
        if (Vector3.Distance(transform.position, _wayPoints[0]) < 1f)
        {
            _wayPoints.RemoveAt(0);
            if (!_wayPoints.Any())
            {
                Bomb.FakeExplosion(transform.position, Color.white, new(1f, 1f));
                Destroy(gameObject);
            }
        }
        if ((transform.position.x > 95f && FromLeft) || (transform.position.x < 7 && !FromLeft) || BridgeGuardControl.ReadyToJump)
        {
            Bomb.FakeExplosion(transform.position, Color.white, new(1f, 1f));
            Destroy(gameObject);
        }
    }

    internal static IEnumerator Respawn(Vector3 position, bool left = false, bool damage = true)
    {
        float passedTime = 0f;
        Bomb.FakeExplosion(position, Color.white, new(1f, 1f, 1f));
        HeroController.instance.RelinquishControl();
        Rigidbody2D player = HeroController.instance.gameObject.GetComponent<Rigidbody2D>();
        while (passedTime < 0.4f)
        {
            player.velocity = new(left ? -250f : 250f, 0f);
            passedTime += Time.deltaTime;
            yield return null;
        }
        if (damage)
        { 
            HeroController.instance.TakeDamage(null, GlobalEnums.CollisionSide.bottom, 1, 2);
            HeroController.instance.HazardRespawn();
        }
        HeroController.instance.RegainControl();
    }
}

internal enum MoveType
{
    Random,

    Circles = 2,

    Diagonal = 4,

    Straight = 5,
}