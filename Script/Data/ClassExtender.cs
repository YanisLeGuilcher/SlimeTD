using System.Collections.Generic;
using System.Linq;
using Script.Entities.Monster;
using UnityEngine;

namespace Script.Data
{
    public static class ClassExtender
    {
        public static bool ContainTuple<T1,T2>(this IEnumerable<Tuple<T1,T2>> listOfTuples, T1 type) =>
            listOfTuples.Any(tuple => tuple.first.Equals(type));

        public static T2 GetTuple<T1,T2>(this IEnumerable<Tuple<T1,T2>> listOfTuples, T1 type) =>
            listOfTuples.First(tuple => tuple.first.Equals(type)).second;
        
        public static T2 GetTupleOrDefault<T1,T2>(this IEnumerable<Tuple<T1,T2>> listOfTuples, T1 type, T2 defaultValue)
        {
            var tmp = listOfTuples.FirstOrDefault(tuple => tuple.first.Equals(type));
            return tmp != null ? tmp.second : defaultValue;
        }


        public static bool TryGetTuple<T1,T2>(this IEnumerable<Tuple<T1,T2>> listOfTuples, T1 type, out T2 value)
        {
            var tmp = listOfTuples.FirstOrDefault(tuple => tuple.first.Equals(type));
            if (tmp != null)
            {
                value = tmp.second;
                return true;
            }
            value = default;
            return false;
        }

        public static bool IsRoot(this Transform transform) => transform.root == transform;

        public static Monster Strongest(this IEnumerable<Monster> entities) =>
            entities.Aggregate((max, p) => p.Rank > max.Rank ? p : max);
        
        public static Monster Weakest(this IEnumerable<Monster> entities) =>
            entities.Aggregate((min, p) => p.Rank < min.Rank ? p : min);
        
        public static Monster LastInPosition(this IEnumerable<Monster> entities) =>
            entities.Aggregate((min, p) => p.Progress < min.Progress ? p : min);
        
        public static Monster FirstInPosition(this IEnumerable<Monster> entities) =>
            entities.Aggregate((max, p) => p.Progress > max.Progress ? p : max);

        public static bool Contains(this LayerMask layerMask, int layer) => (layerMask.value >> layer) % 2 == 1;

    }
}


