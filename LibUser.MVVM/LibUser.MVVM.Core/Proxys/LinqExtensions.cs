using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace LibUser.MVVM.Core.Proxys
{
    public static class LinqExtensions
    {
        public static ObservableCollection<T> ToObservableCollection<T>(this IEnumerable<T> _LinqResult)
        {
            return new ObservableCollection<T>(_LinqResult);
        }

        public static ObservableCollection<T> AddRange<T>(this ObservableCollection<T> _Base, ICollection<T> _WaitForAdd)
        {
            foreach (var item in _WaitForAdd)
            {
                _Base.Add(item);
            }
            return _Base;
        }

    }
}
