using System.Collections.Generic;

namespace AugatonLib.Random
{
    public static class Chance
    {
        public static bool Roll(float percent)
        {
            if (percent <= 0f)
                return false;

            if (percent >= 100f)
                return true;

            return UnityEngine.Random.Range(0f, 100f) < percent;
        }

        public static T Pick<T>(IReadOnlyList<T> source)
        {
            return source is null || source.Count == 0
                ? default
                : source[UnityEngine.Random.Range(0, source.Count)];
        }

        public static T PickWeighted<T>(IReadOnlyList<T> source, System.Func<T, int> weightSelector)
        {
            if (source is null || source.Count == 0 || weightSelector is null)
                return default;

            int total = 0;

            foreach (T candidate in source)
            {
                int weight = weightSelector(candidate);

                if (weight > 0)
                    total += weight;
            }

            if (total <= 0)
                return default;

            int roll = UnityEngine.Random.Range(0, total);

            foreach (T candidate in source)
            {
                int weight = weightSelector(candidate);

                if (weight <= 0)
                    continue;

                roll -= weight;

                if (roll < 0)
                    return candidate;
            }

            return source[source.Count - 1];
        }

        public static void Shuffle<T>(IList<T> list)
        {
            if (list is null)
                return;

            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = UnityEngine.Random.Range(0, i + 1);
                T temp = list[i];
                list[i] = list[j];
                list[j] = temp;
            }
        }
    }
}
