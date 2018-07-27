using System;
using System.Collections.Generic;
using BeatThat.Service;
using UnityEngine;

namespace BeatThat.Entities
{
    /// <summary>
    /// Convenience attrbibute for registering an EntityStore<DataType> class.
    /// 
    /// Allows you to do this:
    /// 
    /// <code>
    /// [RegisterEntityStore] // auto registers service as HasEntities<MyDataType> and HasEntityData<DataType>
    /// public class MyEntityStore : EntityStore<MyDataType> {}
    /// </code>
    /// 
    /// ...instead of the longer, boiler-plate version
    /// <code>
    /// [ResisterService(
    ///     proxyInterfaces: new Type[] { 
    ///         typeof(HasEntities<TopicMasteryData>),
    ///         typeof(HasEntityData<TopicMasteryData>)
    ///     }
    /// )]
    /// public class MyEntityStore : EntityStore<MyDataType> {}
    /// </code>
    /// </summary>
    public class RegisterEntityStoreAttribute : RegisterServiceAttribute
	{
        public RegisterEntityStoreAttribute(
            InterfaceRegistrationPolicy interfaceRegistrationPolicy
                = InterfaceRegistrationPolicy.RegisterInterfacesDeclaredOnTypeIfNoProxyInterfacesArgument,
            Type[] proxyInterfaces = null,
            int priority = 0) 
            : base(null, interfaceRegistrationPolicy, proxyInterfaces, priority)
        {
            
        }

        override public void GetProxyInterfaces(Type implType, ICollection<Type> toInterfaces)
        {
            base.GetProxyInterfaces(implType, toInterfaces);

            var fromInterfaces = implType.GetInterfaces();

            FindAndAddProxyType(implType, typeof(HasEntities<>), fromInterfaces, toInterfaces);
            FindAndAddProxyType(implType, typeof(HasEntityData<>), fromInterfaces, toInterfaces);

            Type entityStoreType = FindEntityStoreType(implType);
            if(entityStoreType != null && !toInterfaces.Contains(entityStoreType)) {
                toInterfaces.Add(entityStoreType);
            }
        }

        private Type FindEntityStoreType(Type type)
        {
            if(type == null || type == typeof(object)) {
                return null;
            }

            if (!type.IsGenericType) {
                return FindEntityStoreType(type.BaseType);
            }                

            if(!typeof(EntityStore<>).IsAssignableFrom(type.GetGenericTypeDefinition())) {
                return FindEntityStoreType(type.BaseType);
            }

            if(type.ContainsGenericParameters) {
                return FindEntityStoreType(type.BaseType);
            }

            return type;
        }

        private void FindAndAddProxyType(Type implType, Type genericType, Type[] searchTypes, ICollection<Type> toInterfaces)
        {
            var iFound = Array.Find(searchTypes, intf =>
            {
                return intf.IsGenericType && !intf.ContainsGenericParameters
                           && genericType.IsAssignableFrom(intf.GetGenericTypeDefinition());
            });

            if (iFound == null)
            {
#if UNITY_EDITOR || DEBUG_UNSTRIP
                Debug.LogWarning("RegisterEntityStore is applied to type "
                                 + implType.FullName + " which doesn't implement "
                                 + genericType.FullName);
#endif
                return;
            }

            var genArgs = iFound.GetGenericArguments();
            if (genArgs == null || genArgs.Length != 1)
            {

#if UNITY_EDITOR || DEBUG_UNSTRIP
                Debug.LogWarning("RegisterEntityStore is applied to type "
                                 + implType.FullName + " but "
                                 + genericType.FullName
                                 + " should be a single param generic type");
#endif
                return;
            }

            if (!toInterfaces.Contains(iFound))
            {
                toInterfaces.Add(iFound);
            }
        }

	}
}


