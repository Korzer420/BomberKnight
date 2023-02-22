using System.Collections.Generic;
using UnityEngine;

namespace BomberKnight.UnityComponents;

internal class DeepnestSpiderControl : MonoBehaviour
{
    private GameObject _spinner;
    private float _passedDirectionalTime = 0.1f;
    private List<GameObject> _bombs = new();

    void Start()
    {
        On.HealthManager.TakeDamage += HealthManager_TakeDamage;
        _spinner = new("Spinner");
        _spinner.transform.SetParent(transform);
        _spinner.transform.localPosition = new(0f, 0f);
    }

    void FixedUpdate()
    {
        if (_passedDirectionalTime > 0f)
        {
            _passedDirectionalTime += Time.deltaTime;
            _spinner.transform.localEulerAngles += new Vector3(0f, 0f, Time.deltaTime);
            if (_passedDirectionalTime > 4f && UnityEngine.Random.Range(0, 5) == 0)
                _passedDirectionalTime = -0.1f;
        }
        else
        {
            _passedDirectionalTime -= Time.deltaTime;
            _spinner.transform.localEulerAngles -= new Vector3(0f, 0f, Time.deltaTime);
            if (_passedDirectionalTime < -4f && UnityEngine.Random.Range(0, 5) == 0)
                _passedDirectionalTime = 0.1f;
        }

        if (_bombs.Count < 8 && UnityEngine.Random.Range(_bombs.Count, 20) < 4)
        {

        }

    }

    private void HealthManager_TakeDamage(On.HealthManager.orig_TakeDamage orig, HealthManager self, HitInstance hitInstance)
    {
        if (self.gameObject == gameObject && hitInstance.AttackType == AttackTypes.Spell)
            hitInstance.Multiplier = 0.8f;
        orig(self, hitInstance);
    }
}
