
using System.Diagnostics;

namespace BBBirder
{
    public interface IIndexElement
    {
        int Index { get; set; }
        object Collection { get; set; }
    }

    public static class IIndexElementExtensions
    {
        [Conditional("DEBUG")]
        public static void SetCollection(this IIndexElement element, object collection)
        {
            if (collection is null)
            {
                element.Collection = collection;
                return;
            }

            if (element.Collection is null)
            {
                element.Collection = collection;
                return;
            }

            if (!ReferenceEquals(element.Collection, collection))
            {
                throw new("IIndexElement already belongs to another collection");
            }
        }
    }
}
