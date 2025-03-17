using System;
namespace Script.Data
{
    public class ValueCalculator<T>
    {
        private Func<T> predicate;

        private T value;

        private bool valueSet;
        
        public ValueCalculator(Func<T> newPredicate) => predicate = newPredicate;

        public void ChangePredicate(Func<T> newPredicate) => predicate = newPredicate;

        public void UnsetValue() => valueSet = false;

        public T ForceCalculation() => value = predicate();


        public T Value
        {
            get
            {
                if (valueSet)
                    return value;
                return value = predicate();
            }
        }
    }
    
    public class ValueCalculator<T,TE>
    {
        private Func<TE,T> predicate;

        private TE param;
        
        private T value;

        private bool valueSet;
        
        public ValueCalculator(Func<TE,T> newPredicate, TE newParam = default)
        {
            predicate = newPredicate;
            param = newParam;
        }

        public void ChangePredicate(Func<TE,T> newPredicate) => predicate = newPredicate;
        public void ChangePredicate(Func<TE,T> newPredicate, TE newParam)
        {
            predicate = newPredicate;
            param = newParam;
        }

        public void UnsetValue() => valueSet = false;

        public T ForceCalculation(TE newParam) => value = predicate(newParam);
        public T ForceCalculation() => value = predicate(param);


        public T Value
        {
            get
            {
                if (valueSet)
                    return value;
                return value = predicate(param);
            }
        }
    }
}