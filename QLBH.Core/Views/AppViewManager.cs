using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Windows.Forms;
using QLBH.Core.Exceptions;
using QLBH.Core.Interfaces;

namespace QLBH.Core.Views
{
    public class AppViewManager : IEnumerable<IBaseViewA>
    {
        private static AppViewManager instance;

        public static AppViewManager Instance
        {
            get { return instance ?? (instance = new AppViewManager()); }
        }

        private readonly List<IBaseViewA> managedViews = new List<IBaseViewA>();

        public ApplicationContext Context { private get; set; }

        internal IBaseViewA CreateView(string viewType, string viewName)
        {
            try
            {
                if (this[viewName] != null && !this[viewName].IsDisposed)
                {
                    throw new ManagedException("View name is already exists.");
                }

                if (this[viewName] != null && this[viewName].IsDisposed)
                {
                    managedViews.Remove(this[viewName]);
                }

                var type = Type.GetType(viewType);

                if (type == null)
                {
                    Assembly asm = Assembly.Load(viewType.Substring(0, viewType.LastIndexOf(".")));

                    type = asm.GetType(viewType);
                }

                var view = (IBaseViewA)Activator.CreateInstance(type);

                if (view == null)
                {
                    throw new ManagedException("Lỗi khai báo view.");
                }

                view.ViewName = viewName;

                managedViews.Add(view);

                return view;
            }
            catch (Exception ex)
            {

                throw new ManagedException(ex.Message);
            }
        }

        internal IBaseViewA CreateView<T>(string viewName)
        {
            try
            {
                return CreateView<T>(viewName, null);
            }
            catch (Exception ex)
            {
                throw new ManagedException(ex.Message);
            }
        }

        internal IBaseViewA CreateView<T>(string viewName, params object[] parameters)
        {
            try
            {
                return CreateView(viewName, typeof(T), parameters);
            }
            catch (Exception ex)
            {
                throw new ManagedException(ex.Message, viewName, parameters);
            }
        }

        internal IBaseViewA CreateView(string viewName, Type typeOfView, params object[] parameters)
        {
            try
            {
                if (this[viewName] != null && !this[viewName].IsDisposed)
                {
                    throw new ArgumentException("View name is already exists.");
                }

                if (this[viewName] != null && this[viewName].IsDisposed)
                {
                    managedViews.Remove(this[viewName]);
                }

                var view = (IBaseViewA)Activator.CreateInstance(typeOfView, parameters);

                if (view == null)
                {
                    throw new ManagedException("Lỗi khai báo view.");
                }

                view.ViewName = viewName;

                if (!Contains(view)) Add(view);

                return view;

            }
            catch (Exception ex)
            {
                throw new ManagedException(ex.Message, viewName, parameters);
            }
        }

        public IBaseViewA this[int index]
        {
            get
            {
                try
                {
                    return managedViews[index];
                }
                catch (Exception ex)
                {
                    throw new ManagedException(ex.Message, index);
                }
            }
        }

        public IBaseViewA this[string viewName]
        {
            get
            {
                try
                {
                    return managedViews.Find(delegate(IBaseViewA match) { return match.ViewName == viewName; });
                }
                catch (Exception ex)
                {
                    throw new ManagedException(ex.Message);
                }
            }
        }

        public int IndexOf(IBaseViewA view)
        {
            return managedViews.IndexOf(view);
        }

        public int Count
        {
            get { return managedViews.Count; }
        }

        internal bool Contains(IBaseViewA item)
        {
            return managedViews.Contains(item);
        }

        internal void Add(IBaseViewA view)
        {
            managedViews.Add(view);
        }

        internal void Remove(IBaseViewA view)
        {
            managedViews.Remove(view);

            if (managedViews.Count == 0 && Context != null) Context.ExitThread();
        }

        #region Implementation of IEnumerable

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public IEnumerator<IBaseViewA> GetEnumerator()
        {
            return managedViews.GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        public delegate bool MatchDelegate<T>(T match);

        public T Find<T>(MatchDelegate<T> matchDelegate)
        {
            foreach (IBaseViewA managedView in managedViews)
            {
                if (managedView is T && matchDelegate((T)managedView)) return (T)managedView;
            }

            return default(T);
        }

        internal IBaseViewA FindByTypeOfController(Type typeOfController)
        {
            foreach (IBaseViewA managedView in managedViews)
            {
                if ((managedView as AppBaseView).Controller.GetType() == typeOfController) return managedView;
            }

            return null;
        }

        internal IBaseViewA FindByTypeOfView(Type typeOfView)
        {
            foreach (IBaseViewA managedView in managedViews)
            {
                if (managedView.GetType() == typeOfView) return managedView;
            }

            return null;
        }

        internal ViewCollection<T> FindAll<T>(MatchDelegate<T> matchDelegate)
        {
            var result = new ViewCollection<T>();

            foreach (IBaseViewA managedView in managedViews)
            {
                if (managedView is T && matchDelegate((T)managedView)) result.InternalAdd((T)managedView);
            }

            return result;
        }

        internal ViewCollection<IBaseViewA> FindAll(Type viewType)
        {
            var result = new ViewCollection<IBaseViewA>();

            foreach (IBaseViewA managedView in managedViews)
            {
                if (managedView.GetType() == viewType) result.InternalAdd(managedView);
            }

            return result;
        }
    }
}