using ItemChanger;
using KorzUtils.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace BomberKnight.ItemData;

[Serializable]
public class WrappedSprite : ISprite
{
    #region Constructors

    public WrappedSprite() { }

    public WrappedSprite(string key)
    {
        if (!string.IsNullOrEmpty(key))
            Key = key;
    }

    #endregion

    #region Properties

    public string Key { get; set; }

    [Newtonsoft.Json.JsonIgnore]
    public Sprite Value => SpriteHelper.CreateSprite<BomberKnight>(Key); 

    #endregion

    public ISprite Clone() => new WrappedSprite(Key);
}
