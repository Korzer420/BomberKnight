using BomberKnight.UnityComponents;
using ItemChanger;
using ItemChanger.Extensions;
using ItemChanger.Locations;
using KorzUtils.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace BomberKnight.ItemData.Locations;

// Deepnest_32
internal class DeepnestBombBagLocation : AutoLocation
{
    protected override void OnLoad()
    {
        Events.AddFsmEdit(new("Spider Flyer (9)", "Control"), ModifySpider);
    }

    protected override void OnUnload()
    {
        throw new NotImplementedException();
    }

    private void ModifySpider(PlayMakerFSM fsm)
    {
        if (Placement.Items.Any(x => !x.IsObtained()))
        {
            if (Placement.Items.All(x => x.WasEverObtained()))
                ItemHelper.SpawnShiny(new(124.78f, 5.42f), Placement);
            else
            {
                // Force the enemy to being stuck in the idle animation
                fsm.GetState("Idle").RemoveTransitionsTo("Chase Start");
                ItemDropper itemDropper = fsm.gameObject.AddComponent<ItemDropper>();
                itemDropper.Placement = Placement;
                itemDropper.Firework = false;
                itemDropper.DropPosition = new(124.78f, 5.42f);
                fsm.GetComponent<HealthManager>().hp = 200;
            }
        }
        fsm.GetComponent<tk2dSprite>().color = Color.blue;
    }
}
