using System;
using System.Collections.Generic;
using System.Linq;

namespace MissPaulingBot.Extensions;

public static class EnumerableExtensions
{
    public static List<List<T>> SplitBy<T>(this IEnumerable<T> enumerable, int count)
    {
        var newList = new List<List<T>>();
        var list = enumerable as List<T> ?? enumerable.ToList();

        if (list.Count <= count)
        {
            newList.Add(list);
        }
        else
        {
            for (var i = 0; i < list.Count; i += count)
            {
                newList.Add(list.GetRange(i, Math.Min(count, list.Count - i)));
            }
        }

        return newList;
    }
        
    public static bool EqualsAny(this string str, params string[] others) 
        => others.Any(str.Equals);
}