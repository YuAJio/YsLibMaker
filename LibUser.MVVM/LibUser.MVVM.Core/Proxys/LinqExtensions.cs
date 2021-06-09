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
            if (_Base == null) throw new ArgumentNullException("The list what is watting for add is Null");
            foreach (var item in _WaitForAdd)
            {
                _Base.Add(item);
            }
            return _Base;
        }

        public static ObservableCollection<T> AddOb<T>(this ObservableCollection<T> _Base, T _WaitForAdd)
        {
            if (_Base == null) throw new ArgumentNullException("The list what is watting for add is Null");
            _Base.Add(_WaitForAdd);
            return _Base;
        }

    }
}
