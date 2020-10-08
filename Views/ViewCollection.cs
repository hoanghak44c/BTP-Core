using System;
using System.Collections;
using System.Collections.Generic;
using QLBH.Core.Exceptions;
using QLBH.Core.Interfaces;

namespace QLBH.Core.Views
{
    public class ViewCollection<TView> : List<TView>, IBaseViewIndex<TView>, ICollection<TView>, IList
    {
        public ViewCollection()
        {
            if (typeof(TView) != typeof(IBaseViewA) &&
                typeof(TView).GetInterface(typeof(IBaseViewA).Name) == null)
            {
                throw new ManagedException("Không đúng kiểu View");
            }
        }

        public TView this[string viewName]
        {
            get
            {
                return AppViewManager.Instance.Find(
                    delegate(TView match)
                        {
                            return ((IBaseViewA)match).ViewName == viewName;
                        });
            }
        }

        bool ICollection<TView>.IsReadOnly
        {
            get { return true; }
        }

        bool IList.IsReadOnly
        {
            get { return true; }
        }

        /// <summary>
        /// Views là thuộc tính chỉ đọc, không thể thực hiện thao tác này!
        /// </summary>
        public new void Add(TView item)
        {
            throw new ManagedException("Views là thuộc tính chỉ đọc, không thể thực hiện thao tác này!");
        }

        internal void InternalAdd(TView item)
        {
            base.Add(item);
        }

        /// <summary>
        /// Views là thuộc tính chỉ đọc, không thể thực hiện thao tác này!
        /// </summary>
        public new void Clear()
        {
            throw new ManagedException("Views là thuộc tính chỉ đọc, không thể thực hiện thao tác này!");
        }

        /// <summary>
        /// Views là thuộc tính chỉ đọc, không thể thực hiện thao tác này!
        /// </summary>
        public new void Insert(int index, TView item)
        {
            throw new ManagedException("Views là thuộc tính chỉ đọc, không thể thực hiện thao tác này!");
        }

        /// <summary>
        /// Views là thuộc tính chỉ đọc, không thể thực hiện thao tác này!
        /// </summary>
        public new bool Remove(TView item)
        {
            throw new ManagedException("Views là thuộc tính chỉ đọc, không thể thực hiện thao tác này!");
        }

        /// <summary>
        /// Views là thuộc tính chỉ đọc, không thể thực hiện thao tác này!
        /// </summary>
        public new void RemoveAt(int index)
        {
            throw new ManagedException("Views là thuộc tính chỉ đọc, không thể thực hiện thao tác này!");
        }

        /// <summary>
        /// Views là thuộc tính chỉ đọc, không thể thực hiện thao tác này!
        /// </summary>
        public new void AddRange(IEnumerable<TView> collection)
        {
            throw new ManagedException("Views là thuộc tính chỉ đọc, không thể thực hiện thao tác này!");
        }

        /// <summary>
        /// Views là thuộc tính chỉ đọc, không thể thực hiện thao tác này!
        /// </summary>
        public new void InsertRange(int index, IEnumerable<TView> collection)
        {
            throw new ManagedException("Views là thuộc tính chỉ đọc, không thể thực hiện thao tác này!");
        }

        /// <summary>
        /// Views là thuộc tính chỉ đọc, không thể thực hiện thao tác này!
        /// </summary>
        public new int RemoveAll(Predicate<TView> match)
        {
            throw new ManagedException("Views là thuộc tính chỉ đọc, không thể thực hiện thao tác này!");
        }

        /// <summary>
        /// Views là thuộc tính chỉ đọc, không thể thực hiện thao tác này!
        /// </summary>
        public new void RemoveRange(int index, int count)
        {
            throw new ManagedException("Views là thuộc tính chỉ đọc, không thể thực hiện thao tác này!");
        }
    }
}