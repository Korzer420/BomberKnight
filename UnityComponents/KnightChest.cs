using BomberKnight.ItemData.Locations;
using KorzUtils.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace BomberKnight.UnityComponents;

internal class KnightChest : MonoBehaviour
{
    #region Properties

    public static ShellSalvagerLocation Location { get; set; }

    #endregion

    #region Unity Methods

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other?.tag == "Nail Attack")
        {
            if (transform.parent != null && transform.parent.GetComponent<KnightChest>() is not null)
                Location.HitChest(transform.parent.name);
            else
                Location.HitChest(gameObject.name);
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision?.gameObject?.tag == "Nail Attack")
        {
            if (transform.parent != null && transform.parent.GetComponent<KnightChest>() is not null)
                Location.HitChest(transform.parent.name);
            else
                Location.HitChest(gameObject.name);
        }
    }

    #endregion
}
