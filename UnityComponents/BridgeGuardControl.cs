using KorzUtils.Helper;
using System.Collections;
using UnityEngine;

namespace BomberKnight.UnityComponents;

internal class BridgeGuardControl : MonoBehaviour
{
    private GameObject[] _doors = new GameObject[2];

    internal static bool ReadyToJump { get; set; }

    internal static bool IsLeft { get; set; }

    void Start()
    {
        On.HealthManager.TakeDamage += HealthManager_TakeDamage;
        On.HealthManager.Die += HealthManager_Die;
        GameObject door = GameObject.Find("left1");
        _doors[0] = new("Left Door");
        _doors[0].layer = 7;
        _doors[0].AddComponent<SpriteRenderer>().sprite = SpriteHelper.CreateSprite<BomberKnight>("Sprites.Door");
        _doors[0].AddComponent<BoxCollider2D>().size = door.GetComponent<BoxCollider2D>().size;
        _doors[0].transform.position = new(1.4364f, 18.7f, 0f);
        _doors[0].transform.localScale = new(1f, 1f);

        _doors[1] = new("Right Door");
        _doors[1].layer = 7;
        _doors[1].AddComponent<SpriteRenderer>().sprite = SpriteHelper.CreateSprite<BomberKnight>("Sprites.Door");
        _doors[1].AddComponent<BoxCollider2D>().size = door.GetComponent<BoxCollider2D>().size;
        _doors[1].transform.position = new(98.3f, 18.7f, 0f);
        _doors[1].transform.localScale = new(-1f, 1f);

        _doors[0].SetActive(true);
        _doors[1].SetActive(true);
    }

    private void HealthManager_Die(On.HealthManager.orig_Die orig, HealthManager self, float? attackDirection, AttackTypes attackType, bool ignoreEvasion)
    {
        orig(self, attackDirection, attackType, ignoreEvasion);
        if (self.gameObject.name == "Fire Sentry")
        { self.GetComponent<ItemDropper>().PrepareDrop(); }
    }

    private IEnumerator DestroyDoors()
    {
        yield return new WaitForSeconds(5f);
        Bomb.FakeExplosion(_doors[0].transform.localPosition, Color.white, new(1.2f, 1.2f));
        Bomb.FakeExplosion(_doors[1].transform.localPosition, Color.white, new(1.2f, 1.2f));
        Destroy(_doors[0]);
        Destroy(_doors[1]);
    }

    private void HealthManager_TakeDamage(On.HealthManager.orig_TakeDamage orig, HealthManager self, HitInstance hitInstance)
    {
        // Since the shade can appear in the room, we have to check for name.
        if (self.gameObject.name == "Fire Sentry")
        {
            if (hitInstance.AttackType != AttackTypes.Nail || ReadyToJump)
                hitInstance.DamageDealt = 0;
            else
                hitInstance.DamageDealt = 1;
            orig(self, hitInstance);
            if (hitInstance.DamageDealt == 1)
                ReadyToJump = true;
        }
        else
            orig(self, hitInstance);
    }
}
