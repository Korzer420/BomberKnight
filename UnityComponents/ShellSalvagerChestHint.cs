using KorzUtils.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace BomberKnight.UnityComponents;

internal class ShellSalvagerChestHint : MonoBehaviour
{
    #region Members

    private float _hintCooldown = 0f;

    #endregion

    #region Unity Methods

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.name == "Knight" || other.name == "Herobox")
            if (_hintCooldown <= 0f)
            { 
                GameHelper.DisplayMessage("An inscription of five knights is visible.");
                _hintCooldown = 3f;
            }
        
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.name == "Knight" || collision.collider.name == "Herobox")
            if (_hintCooldown <= 0f)
            {
                GameHelper.DisplayMessage("An inscription of five knights is visible.");
                _hintCooldown = 3f;
            }
    }

    void FixedUpdate()
    {
        if (_hintCooldown > 0f)
            _hintCooldown -= Time.deltaTime;
    }

    #endregion
}
