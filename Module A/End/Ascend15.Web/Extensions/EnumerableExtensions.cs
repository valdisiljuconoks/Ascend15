using System.Collections;

namespace Ascend15.Extensions
{
    public static class EnumerableExtensions
    {
        public static bool IsNullOrEmpty(this IEnumerable target)
        {
            if (target == null)
            {
                return true;
            }

            return !target.GetEnumerator().MoveNext();
        }
    }
}
