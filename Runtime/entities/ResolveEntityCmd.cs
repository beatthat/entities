using BeatThat.Commands;
using BeatThat.Notifications;
using BeatThat.Service;

namespace BeatThat.Entities
{
    /// <summary>
    /// Convenience implementation requires just the DataType as generic parameter.
    ///
    /// Assumes that the services for HasEntities&lt;YourDataType&gt; and EntityAPI&lt;YourDataType&gt;
    /// can be located/injected using just those interfaces. 
    /// 
    /// This means you must register those services with the interfaces specified, e.g.
    /// 
    /// <code>
    /// [RegisterService(proxyInterfaces: new Type[] { typeof(HasEntities&lt;YourDataType&gt;) } ]
    /// public class YourStoreClass : EntityDataStore&lt;YourDataType&gt; { }
    ///
    /// </code>
    /// </summary>
    public class ResolveEntityCmd<DataType> : ResolveEntityCmd<DataType, HasEntities<DataType>, EntityResolver<DataType>> {}

    /// <summary>
    /// Generic command to resolve a single entity by a key (id, alias, or uri)
    /// </summary>
    public class ResolveEntityCmd<DataType, StoreType, ResolverType> : NotificationCommandBase<string>
        where StoreType : HasEntities<DataType>
        where ResolverType : EntityResolver<DataType>
	{
        public bool m_debug;

        [Inject] private StoreType hasData { get; set; }
        [Inject] private ResolverType resolver { get; set; }

        public override string notificationType { get { return Entity<DataType>.RESOLVE_REQUESTED; } }

        public override void Execute (string key)
		{
            switch(ResolveAdviceHelper.AdviseOnAndSendErrorIfCoolingDown (key, hasData, Entity<DataType>.RESOLVE_FAILED, debug: m_debug)) {
                case ResolveAdvice.PROCEED:
                    break;
                default:
                    return;
			}

            Entity<DataType>.ResolveStarted (key);

            this.resolver.GetOne(key, (r =>
            {
                if (ResolveErrorHelper.HandledError(key, r, Entity<DataType>.RESOLVE_FAILED, debug: m_debug))
                {
                    return;
                }

                var resultItem = r.item;

                if(!IsOk(resultItem.status)) {
                    NotificationBus.Send(Entity<DataType>.RESOLVE_FAILED, new ResolveFailedDTO
                    {
                        key = key,
                        error = resultItem.status
                    });
                    return;
                }

                Entity<DataType>.ResolveSucceeded(
                    new ResolveSucceededDTO<DataType> {
                    key = key,
                    id = resultItem.id,
                    data = resultItem.data
                });
			}));

		}

        virtual protected bool IsOk(string status) 
        {
            return status == Constants.STATUS_OK;
        }
	}
}


