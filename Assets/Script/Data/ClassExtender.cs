using System;
using System.Collections.Generic;
using System.Linq;
using Script.Entities.Monster;
using UnityEngine;
using UnityEngine.EventSystems;

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

        public static Monster Strongest(this IEnumerable<Monster> entities)
        {
            return entities.Aggregate(new List<Monster>(), (strongest, current) =>
            {
                if (strongest.Count == 0 || current.LifePoint > strongest[0].LifePoint)
                {
                    strongest.Clear();
                    strongest.Add(current);
                }
                else if (current.LifePoint == strongest[0].LifePoint)
                    strongest.Add(current);
                return strongest;
            }).FirstInPosition();
        }

        public static Monster Weakest(this IEnumerable<Monster> entities)
        {
            return entities.Aggregate(new List<Monster>(), (weakest, current) =>
            {
                if (weakest.Count == 0 || current.LifePoint < weakest[0].LifePoint)
                {
                    weakest.Clear();
                    weakest.Add(current);
                }
                else if (current.LifePoint == weakest[0].LifePoint)
                    weakest.Add(current);
                return weakest;
            }).FirstInPosition();
        }
        
        public static Monster Spawner(this IEnumerable<Monster> entities)
        {
            Monster target = null;
            bool withSpawner = false;
            foreach (var entity in entities)
            {
                if (entity is MonsterSpawner)
                {
                    withSpawner = true;
                    if (target && target.Progress < entity.Progress)
                        target = entity;
                    else
                        target = entity;
                }
                else if (!withSpawner)
                {
                    if (target && target.Progress < entity.Progress)
                        target = entity;
                    else
                        target = entity;
                }
            }
            return target;
        }
        
        public static Monster LastInPosition(this IEnumerable<Monster> entities) =>
            entities.Aggregate((min, p) => p.Progress < min.Progress ? p : min);
        
        public static Monster FirstInPosition(this IEnumerable<Monster> entities) =>
            entities.Aggregate((max, p) => p.Progress > max.Progress ? p : max);

        public static bool Contains(this LayerMask layerMask, int layer) => (layerMask.value >> layer) % 2 == 1;

        public static bool Equals(this float a, float b, float epsilon) => Mathf.Abs(a - b) < epsilon;
        
        public static T Next<T>(this T src) where T : System.Enum
        {
            T[] values = (T[])System.Enum.GetValues(typeof(T));
            int index = Array.IndexOf(values, src) + 1;
            return values[index % values.Length];
        }
        
        public static Dictionary<T1, T2> ToDictionary<T1,T2>(this IEnumerable<Tuple<T1,T2>> tuples)
        {
            Dictionary<T1, T2> dico = new();

            foreach (var tuple in tuples)
                dico.Add(tuple.first,tuple.second);

            return dico;
        }
        
        
        public static bool IsPointerOverUIExcludingSortingLayer(this EventSystem eventSys, int sortingLayerToIgnore)
        {
            var eventData = new PointerEventData(eventSys)
            {
                position = Input.mousePosition
            };
            var results = new List<RaycastResult>();
            eventSys.RaycastAll(eventData, results);

            foreach (var result in results)
                if (result.gameObject.layer != sortingLayerToIgnore)
                    return true;
            
            return false;
        }


    }
}


