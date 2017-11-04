using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MRL.SSL.GameDefinitions
{
    public static class Extentions
    {
        public static List<T> Clone<T>(this List<T> listToClone) where T : ICloneable
        {
            return listToClone.Select(item => (T)item.Clone()).ToList();
        }

        public static Dictionary<T, V> Clone<T, V>(this Dictionary<T, V> dicToClone) where V : ICloneable
        {
            return dicToClone.Select(k => new KeyValuePair<T, V>(k.Key, (V)k.Value.Clone())).ToDictionary(k => k.Key, v => v.Value);
        }
    }
}
