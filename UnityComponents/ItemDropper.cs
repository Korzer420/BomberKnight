using ItemChanger;
using KorzUtils.Helper;
using System.Collections;
using UnityEngine;

namespace BomberKnight.UnityComponents;

internal class ItemDropper : MonoBehaviour
{
    private bool _alreadyThrown = false;

    public AbstractPlacement Placement { get; set; }

    public Vector3 DropPosition { get; set; } = Vector3.zero;

    public bool Firework { get; set; } = true;

    public Color FireworkColor { get; set; } = Color.white;

    public void PrepareDrop()
    {
        if (!_alreadyThrown)
        {
            _alreadyThrown = true;
            if (Firework)
                GameManager.instance.StartCoroutine(DoFirework());
            else if (DropPosition != Vector3.zero)
                ItemHelper.SpawnShiny(DropPosition, Placement);
            else
                ItemHelper.SpawnShiny(transform.position, Placement);
        }
    }

    private IEnumerator DoFirework()
    {
        GameObject explosion = GameObject.Instantiate(Bomb.Explosion);
        explosion.transform.localScale = new(1.8f, 1.8f, 1.8f);
        explosion.SetActive(false);

        // Color explosion
        ParticleSystem.MainModule settings = explosion.GetComponentInChildren<ParticleSystem>().main;
        settings.startColor = new ParticleSystem.MinMaxGradient(FireworkColor);
        explosion.GetComponentInChildren<SpriteRenderer>().color = FireworkColor;
        explosion.GetComponentInChildren<SimpleSpriteFade>().fadeInColor = FireworkColor;

        typeof(SimpleSpriteFade).GetField("normalColor", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
            .SetValue(explosion.GetComponentInChildren<SimpleSpriteFade>(), FireworkColor);
        Destroy(explosion.LocateMyFSM("damages_enemy"));
        Destroy(explosion.GetComponent<DamageHero>());

        float passedTime = 0f;
        int passedMilestone = 0;
        while(passedTime < 3f)
        {
            passedTime += Time.deltaTime;
            if (passedTime >= .5f && passedMilestone == 0)
            {
                passedMilestone++;
                GameObject.Instantiate(explosion, transform.position, Quaternion.identity).SetActive(true);
            }
            else if (passedTime >= 0.75f && passedMilestone == 1)
            {
                passedMilestone++;
                GameObject.Instantiate(explosion, transform.position + RollDistance(), Quaternion.identity).SetActive(true);
                yield return new WaitForSeconds(.2f);
                GameObject.Instantiate(explosion, transform.position + RollDistance(), Quaternion.identity).SetActive(true);
            }
            else if (passedTime >= 1f && passedMilestone == 2)
            {
                passedMilestone++;
                for (int i = 0; i < 4; i++)
                {
                    GameObject.Instantiate(explosion, transform.position + RollDistance(), Quaternion.identity).SetActive(true);
                    yield return new WaitForSeconds(.2f);
                }
            }
            else if (passedTime >= 1.5f && passedMilestone == 3)
            {
                passedMilestone++;
                for (int i = 0; i < 6; i++)
                {
                    GameObject.Instantiate(explosion, transform.position + RollDistance(), Quaternion.identity).SetActive(true);
                    yield return new WaitForSeconds(.2f);
                }
            }
            else if (passedTime >= 2f && passedMilestone == 4)
            {
                passedMilestone++;
                for (int i = 0; i < 6; i++)
                {
                    GameObject.Instantiate(explosion, transform.position + RollDistance(), Quaternion.identity).SetActive(true);
                    yield return new WaitForSeconds(.2f);
                }
            }
            else if (passedTime >= 2.5f && passedMilestone == 5)
            {
                passedMilestone++;
                for (int i = 0; i < 12; i++)
                {
                    GameObject.Instantiate(explosion, transform.position + RollDistance(), Quaternion.identity).SetActive(true);
                    yield return new WaitForSeconds(.2f);
                }
            }
            yield return null;
        }

        if (DropPosition != Vector3.zero)
        {
            GameObject.Instantiate(explosion, DropPosition, Quaternion.identity).SetActive(true);
            ItemHelper.SpawnShiny(DropPosition, Placement);
        }
        else
        {
            GameObject.Instantiate(explosion, transform.position, Quaternion.identity).SetActive(true);
            ItemHelper.SpawnShiny(transform.position, Placement);
        }
    }

    private static Vector3 RollDistance() => new(Random.Range(0f, 3f) - 1.5f, Random.Range(0f, 3f) - 1.5f);
}
