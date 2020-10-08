using System;
using System.Reflection;
using QLBH.Core.Data;
using QLBH.Core.Providers;

namespace QLBH.Core.Infors
{
    [Serializable]
    [ObfuscationAttribute(Feature = "properties renaming")]
    public class NotifyInfo
    {
        public NotifyInfo()
        {
            SetOrigin();
        }
        /// <summary>
        /// Read only to determine object is original.
        /// </summary>
        public bool IsOrigin { get; private set; }

        protected void NotifyChange()
        {
            IsOrigin = false;
        }

        public void SetOrigin()
        {
            IsOrigin = true;
        }

        internal void NotOrigin()
        {
            IsOrigin = false;
        }

        protected virtual NotifyInfo CloneThis(NotifyInfo objClone)
        {
            objClone.IsOrigin = false;

            return objClone;
        }

        public NotifyInfo Clone()
        {
            NotifyInfo objCloned = (NotifyInfo)Activator.CreateInstance(GetType(), null);
            PropertyInfo[] propertyInfos = CBO.Instance.GetPropertyInfo(GetType());

            for (int i = 0; i < propertyInfos.Length; i++)
            {
                try
                {
                    if(propertyInfos[i].Name!= "IsOrigin")
                        GetType().InvokeMember(propertyInfos[i].Name, BindingFlags.SetProperty, null, objCloned,
                                               new[] {propertyInfos[i].GetValue(this, null)});
                }
                catch (Exception exception)
                {
                    EventLogProvider.Instance.WriteOfflineLog(exception.ToString(), "Clone object.");
                }
            }
            
            objCloned.IsOrigin = IsOrigin;

            return objCloned;
        }

    }
}