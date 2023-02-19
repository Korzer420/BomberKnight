using ItemChanger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BomberKnight.ItemData;

internal static class ItemManager
{
    #region Members

    private static Dictionary<string, AbstractItem> _items = new();

    #endregion

    #region Constructors

    static ItemManager()
    {
        // To do: Load json file with items and location
    }

    #endregion

    #region Methods

    internal static void Initialize()
    {
        
    }

    #endregion
}
