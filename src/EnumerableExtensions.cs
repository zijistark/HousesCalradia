using System.Collections.Generic;

using TaleWorlds.Core;

namespace HousesCalradia
{
    internal static class EnumerableExtensions
    {
        public static T RandomPick<T>(this IEnumerable<T> e) => e.GetRandomElementInefficiently();
    }
}
