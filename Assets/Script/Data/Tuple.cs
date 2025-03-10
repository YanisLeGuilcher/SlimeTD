namespace Script.Data
{
    [System.Serializable]
    public class Tuple<T1,T2>
    {
        public T1 first;
        public T2 second;
    }
    
    [System.Serializable]
    public class Tuple<T1,T2,T3>
    {
        public T1 first;
        public T2 second;
        public T3 third;
    }
}

