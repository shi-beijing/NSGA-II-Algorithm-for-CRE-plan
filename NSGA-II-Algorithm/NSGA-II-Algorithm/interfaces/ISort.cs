using System.Collections.Generic;

namespace NSGA_II_Algorithm.interfaces
{
    interface ISort<T, G>
    {
        List<G> Sort(List<T> list);

        // Sort the list of type T into different F1 F2 F3 (class is G), F1 2 3 to form a list of type G
    }
}
