namespace MarTools
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    
    public static class RandomUtilities
    {
        [System.Serializable]
        public class WeightedList<T>
        {
            public List<WeightedOption<T>> Options = new List<WeightedOption<T>>();
        }
    
        [System.Serializable]
        public class WeightedOption<T>
        {
            public T element;
            public float weight = 1;
    
            public WeightedOption(T element, float weight)
            {
                this.element = element;
                this.weight = weight;
            }
        }
    
        /// <summary>
        /// Picks a random element from a weighted option list. Can be used to calculate drop rates and other
        /// chance based things where percentages are different for each item
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="WeightedOptions"></param>
        /// <returns></returns>
        public static T PickRandomWeighted<T>(this IEnumerable<WeightedOption<T>> WeightedOptions)
        {
            float weightSum = WeightedOptions.Sum(item => item.weight);
            float randomValue = UnityEngine.Random.value * weightSum;
    
            float weightAccumulated = 0;
            foreach (var option in WeightedOptions)
            {
                weightAccumulated += option.weight;
                if(randomValue < weightAccumulated)
                {
                    return option.element;
                }
            }
    
            throw new Exception("No element exists");
        }
        /// <summary>
        /// Converts a collection of items into a weighted collection that can be used for the GetRandomWeightedOptions method
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection"></param>
        /// <param name="weights"></param>
        /// <returns></returns>
        public static IEnumerable<WeightedOption<T>> ConvertToWeighted<T>(this IEnumerable<T> collection, float[] weights)
        {
            List<WeightedOption<T>> Options = new List<WeightedOption<T>>();
    
            int index = 0;
            foreach (var element in collection)
            {
                Options.Add(new WeightedOption<T>(element, weights[index++]));
            }
    
            if (index != weights.Length) Debug.Log("Weighs array is not the same size as the collection"); 
            return Options;
        }
    
        public static List<WeightedOption<Value>> ConvertToWeighted<T, Value>(this IEnumerable<T> collection, Func<T, Value> GetValue, Func<T, float> Weight)
        {
            List<WeightedOption<Value>> Options = new List<WeightedOption<Value>>();
            foreach (var element in collection)
            {
                Options.Add(new WeightedOption<Value>(GetValue.Invoke(element), Weight.Invoke(element)));
            }
            return Options;
        }
        public static WeightedList<int> ConvertToWeighted(this List<float> collection)
        {
            WeightedList<int> List = new WeightedList<int>() { Options = new List<WeightedOption<int>>()};
    
            for (int i = 0; i < collection.Count; i++)
            {
                List.Options.Add(new WeightedOption<int>(i, collection[i]));
            }
            return List;
        }
    
        /// <summary>
        /// Converts a collection of items into a weighted collection using a custom fucntion
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection"></param>
        /// <param name="weightFunction"></param>
        /// <returns></returns>
        public static IEnumerable<WeightedOption<T>> ConvertToWeighted<T>(this IEnumerable<T> collection, Func<T, float> weightFunction)
        {
            List<WeightedOption<T>> Options = new List<WeightedOption<T>>();
            foreach (T item in collection)
            {
                Options.Add(new WeightedOption<T>(item, weightFunction(item)));
            }
    
            return Options;
        }
    
        /// <summary>
        /// Picks a random element from a container
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection"></param>
        /// <returns></returns>
        public static T PickRandom<T>(this IEnumerable<T> collection)
        {
            var array = collection.ToArray();
            if (array != null && array.Length != 0)
            {
                return array[UnityEngine.Random.Range(0, array.Length)];
            }
            else
            {
                throw new System.Exception("The supplied container is empty or null");
            }
        }

        public static T PickRandomAndRemove<T>(this List<T> collection)
        {
            var t = collection.PickRandom();
            collection.Remove(t);

            return t;
        }

        public static List<T> PickRandomAndRemove<T>(this List<T> collection, int amount)
        {
            List<T> Picked = new List<T>();
            for (int i = 0; i < amount; i++)
            {
                var p = collection.PickRandomAndRemove();
                Picked.Add(p);
            }

            return Picked;
        }

        /// <summary>
        /// Picks a random float from the Vector2 array range (inclusive)
        /// </summary>
        /// <param name="range"></param>
        /// <returns></returns>
        public static float PickRandom(this Vector2 range)
        {
            return Mathf.Lerp(range.x, range.y, UnityEngine.Random.value);
        }
    
        public static T PickRandom<T>(this WeightedList<T> List)
        {
            return List.Options.PickRandomWeighted();
        }
    
        public static void Shuffle<T>(this IList<T> list)
        {
            System.Random rng = new System.Random();
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
    }
    
}