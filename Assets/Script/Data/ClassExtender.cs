using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using Script.Entities.Monster;
using Script.Entities.Tower;
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
        
        public static Monster Spawner(this List<Monster> entities)
        {
            var splitList = entities.Split(monster => monster is MonsterSpawner);

            return splitList.Item1.Any() ? splitList.Item1.FirstInPosition() : splitList.Item2.FirstInPosition();
        }
        
        public static (List<T>,List<T>) Split<T>(this List<T> list, Func<T,bool> predicate)
        {
            List<T> part1 = new();
            List<T> part2 = new();
            foreach (var element in list)
            {
                if(predicate(element))
                    part1.Add(element);
                else
                    part2.Add(element);
            }

            return (part1, part2);
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
        
        private const string Charset = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789,.\n\r\0 ";
        private static readonly int BaseSize = Charset.Length;

        public static byte[] ToBase38(this string str)
        {
            BigInteger value = 0;

            foreach (char c in str)
            {
                int index = Charset.IndexOf(c);
                if (index == -1) 
                    throw new FormatException($"Caractère {c} invalide dans l'encodage.");
                value = value * BaseSize + index;
            }
            return value.ToByteArray().Reverse().ToArray();
        }
        
        
        public static string FromBase38(this byte[] bytes)
        {
            BigInteger value = new BigInteger(bytes.Reverse().ToArray());
            StringBuilder result = new StringBuilder();
            
            while (value > 0)
            {
                int remainder = (int)(value % BaseSize);
                result.Insert(0, Charset[remainder]);
                value /= BaseSize;
            }

            return result.Length > 0 ? result.ToString() : Charset[0].ToString();
        }

        public static List<TowerData> ToData(this List<Tower> towers)
        {
            List<TowerData> data= new();

            foreach (var tower in towers)
            {
                if (tower is Defender defender)
                {
                    data.Add(new TowerData
                    {
                        attackStyle = defender.AttackStyle,
                        type = defender.Type,
                        position = defender.transform.position
                    });
                }
                else
                {
                    data.Add(new TowerData
                    {
                        type = tower.Type,
                        position = tower.transform.position
                    });
                }
            }
            return data;
        }
    }
}


