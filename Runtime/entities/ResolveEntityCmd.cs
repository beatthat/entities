using BeatThat.Commands;
using BeatThat.Notifications;
using BeatThat.DependencyInjection;
using System;
using UnityEngine;

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
    public class ResolveEntityCmd<DataType> : ResolveEntityCmd<DataType, HasEntities<DataType>, EntityResolver<DataType>> { }

    /// <summary>
    /// Generic command to resolve a single entity by a key (id, alias, or uri)
    /// </summary>
    public class ResolveEntityCmd<DataType, StoreType, ResolverType> : NotificationCommandBase<ResolveRequestDTO>
        where StoreType : HasEntities<DataType>
        where ResolverType : EntityResolver<DataType>
    {
        public bool m_debug;
        [Inject]public StoreType hasData { get; set; }
        [Inject]public ResolverType resolver { get; set; }

        public override string notificationType { get { return Helper.notificationType; } }

        public override void Execute(ResolveRequestDTO req)
        {
            this.helper.Execute(req);
        }

        virtual protected bool IsOk(string status)
        {
            return Helper.IsOkDefault(status);
        }

        public class Helper
        {
            public delegate bool IsStatusOKDelegate(string status);

            public bool debug { get; set; }
            private StoreType hasData { get; set; }
            private IsStatusOKDelegate isOkStatus { get; set; }
            private ResolverType resolver { get; set; }

            public Helper(
                StoreType hasData, 
                ResolverType resolver, 
                IsStatusOKDelegate isOkStatus = null, 
                bool debug = false)
            {
                this.hasData = hasData;
                this.resolver = resolver;
                this.isOkStatus = isOkStatus ?? Helper.IsOkDefault;
                this.debug = debug;
            }

            public static string notificationType
            {
                get
                {
                    return Entity<DataType>.RESOLVE_REQUESTED;
                }
            }

            public void Execute(ResolveRequestDTO req)
            {
                var key = req.key;
                if (string.IsNullOrEmpty(key))
                {
#if UNITY_EDITOR || DEBUG_UNSTRIP
                    Debug.LogWarningFormat(
                        "[{0}] null or empty key passed to resolve for {1}",
                        Time.frameCount, typeof(DataType).Name
                    );
                    Entity<DataType>.ResolveFailed(new ResolveFailedDTO
                    {
                        key = "",
                        error = "null or empty key passed to resolve"
                    });
                    return;
#endif
                }
                if (!req.forceUpdate)
                {
                    var resolveAdvice = ResolveAdviceHelper.AdviseOnAndSendErrorIfCoolingDown(
                        key, hasData, Entity<DataType>.RESOLVE_FAILED, debug: this.debug
                    );
                    switch (resolveAdvice)
                    {
                        case ResolveAdvice.PROCEED:
                            break;
                        default:
#if UNITY_EDITOR || DEBUG_UNSTRIP
                            if (this.debug)
                            {
                                Debug.LogFormat(
                                    "[{0}] will skip resolve for {1} with key {2} on advice {3}",
                                    Time.frameCount, typeof(DataType).Name, key, resolveAdvice
                                );
                            }
#endif
                            return;
                    }
                }
                Resolve(req);
            }

#if NET_4_6
            private async void Resolve(ResolveRequestDTO req)
            {
#if UNITY_EDITOR || DEBUG_UNSTRIP
                if (this.debug)
                {
                    Debug.LogFormat(
                        "[{0}] will resolve {1} with key {2}",
                        Time.frameCount, typeof(DataType).Name, req.key
                    );
                }
#endif
                Entity<DataType>.ResolveStarted(req);
                try
                {
                    var resultItem = await this.resolver.ResolveAsync(req);
                    if (!this.isOkStatus(resultItem.status))
                    {
                        NotificationBus.Send(
                            Entity<DataType>.RESOLVE_FAILED,
                            ResolveFailedDTO.For(req, resultItem.status)
                        );
                        return;
                    }
                    Entity<DataType>.ResolveSucceeded(
                        new ResolveSucceededDTO<DataType>
                        {
                            key = req.key,
                            id = resultItem.id,
                            resolveRequestId = req.resolveRequestId,
                            data = resultItem.data,
                            timestamp = resultItem.timestamp,
                            maxAgeSecs = resultItem.maxAgeSecs
                        });
                }
                catch (Exception e)
                {
                    Entity<DataType>.ResolveFailed(
                        ResolveFailedDTO.For(req, e.Message)
                    );
                }
            }
#else
            private void Resolve(ResolveRequestDTO req)
            {
                Entity<DataType>.ResolveStarted (req);
                this.resolver.Resolve(req, (r =>
                {
                    if (ResolveErrorHelper.HandledError(req.key, r, Entity<DataType>.RESOLVE_FAILED, debug: this.debug))
                    {
                        return;
                    }
                    var resultItem = r.item;
                    if(!this.isOkStatus(resultItem.status)) {
                        NotificationBus.Send(
                            Entity<DataType>.RESOLVE_FAILED,
                            ResolveFailedDTO.For(req, resultItem.status)
                        );
                        return;
                    }
                    Entity<DataType>.ResolveSucceeded(
                        new ResolveSucceededDTO<DataType> {
                        key = req.key,
                        id = resultItem.id,
                        resolveRequestId = req.resolveRequestId,
                        data = resultItem.data,
                        timestamp = resultItem.timestamp,
                        maxAgeSecs = resultItem.maxAgeSecs
                    });
                }));
            }
#endif

            public static bool IsOkDefault(string status)
            {
                return status == ResolveStatusCode.OK;
            }
        }

        private Helper m_helper;
        private Helper helper
        {
            get
            {
                return m_helper
                    ?? (m_helper = new Helper(
                        this.hasData, this.resolver, this.IsOk, m_debug));
            }
        }
    }
}


