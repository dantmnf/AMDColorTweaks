using System.Collections;
using System.Collections.Generic;

namespace AMDColorTweaks.ViewModel
{
    internal class EnumWithDescription<T> where T : System.Enum
    {
        public T Value { get; init; }
        public string Description { get; init; }

        public EnumWithDescription(T value, string description)
        {
            Value = value;
            Description = description;
        }

        public override string ToString()
        {
            return Description;
        }

        public static EnumWithDescription<T> FindDescription(T value, ICollection<EnumWithDescription<T>> collection)
        {
            foreach (var item in collection)
            {
                if (item.Value.Equals(value))
                {
                    return item;
                }
            }
            var str = value.ToString();
            if (string.IsNullOrEmpty(str)) str = "<Unknown>";
            return new(value, str);
        }

        public class Collection : IEnumerable
        {
            private readonly Dictionary<T, EnumWithDescription<T>> dict = new();

            public void Add(T value, string description)
            {
                dict.Add(value, new(value, description));
            }

            public EnumWithDescription<T> this[T key] {
                get
                {
                    if (dict.TryGetValue(key, out var value))
                    {
                        return value;
                    }
                    var str = key.ToString();
                    if (string.IsNullOrEmpty(str)) str = "<Unknown>";
                    return new EnumWithDescription<T>(key, str);
                } 
            }

            public IEnumerable<T> Keys => dict.Keys;

            public IEnumerable<EnumWithDescription<T>> Values => dict.Values;

            public int Count => dict.Count;

            public bool ContainsKey(T key)
            {
                return dict.ContainsKey(key);
            }

            public IEnumerator<EnumWithDescription<T>> GetEnumerator()
            {
                return Values.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return dict.Values.GetEnumerator();
            }
        }
    }
}