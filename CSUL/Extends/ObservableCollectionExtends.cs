using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace CSUL.Extends
{
    public static class ObservableCollectionExtends
    {
        public static void AddRange<T>(this ObservableCollection<T> parent, IEnumerable<T> collection)
        {
            foreach (var item in collection)
            {
                parent.Add(item);
            }
        }

        public static void AddRange<T>(this ObservableCollection<T> parent, IEnumerable<T> collection, Func<T, bool>? condition = null, Action<IEnumerable<T>, T>? exec = null)
        {
            foreach (var item in collection)
            {
                parent.Add(item);
                if(condition != null)
                {
                    condition(item);
                    exec?.Invoke(parent, item);
                }
            }
        }
    }
}
