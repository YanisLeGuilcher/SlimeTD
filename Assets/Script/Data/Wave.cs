using System;
using System.Collections.Generic;
using UnityEngine;


namespace Script.Data
{
    [CreateAssetMenu(fileName = "Wave", menuName = "Data/Wave")]
    public class Wave : ScriptableObject
    {
        [Serializable]
        public struct WavePart
        {
            public int numberOfMonster;
            public GameObject monster;
            public float timeBetweenMonster;
        }

        public List<WavePart> waveParts;

        public bool isInfiniteWave;
    }
}

