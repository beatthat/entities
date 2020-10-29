using System;
using System.Collections.Generic;
using BeatThat.Bindings;
using BeatThat.Notifications;
using BeatThat.Pools;
using BeatThat.Requests;
using UnityEngine;

namespace BeatThat.Entities
{
    using N = NotificationBus;
    using Opts = NotificationReceiverOptions;

    /// <summary>
    /// Wrapper for all Entity types that contains the data
    /// for the entity (if resolved) in addition to the resolve status/metadata.
    /// </summary>
    public struct Entity<DataType>
    {
        public string id;
        public DataType data;
        public ResolveStatus status;

        /// <summary>
        /// hacky flag to enable debugging (usually temporarily) for a single entity type
        /// </summary>
        public static bool DEBUG = false;
        private static int RESOLVE_REQUEST_SEQ = 0;

        /// <summary>
        /// resolve request ids allow notification listeners
        /// to determine if a specific resolve request is completed, etc.
        /// </summary>
        /// <returns>The resolve request identifier.</returns>
        private static int NextResolveRequestId()
        {
            RESOLVE_REQUEST_SEQ =
                RESOLVE_REQUEST_SEQ < int.MaxValue
                                         ? RESOLVE_REQUEST_SEQ + 1 : 0;

            return RESOLVE_REQUEST_SEQ;
        }

        /// <summary>
        /// Gets the data wrapped within the entity if and only if the data has been resolved.
        /// </summary>
        /// <returns><c>true</c>, if data was gotten, <c>false</c> otherwise.</returns>
        /// <param name="data">Data.</param>
        public bool GetData(out DataType data)
        {
            if (!this.status.hasResolved)
            {
                data = default(DataType);
                return false;
            }
            data = this.data;
            return true;
        }

        private static ResolveRequestDTO NewResolveRequest(string loadKey, bool forceResolve = false)
        {
            var reqId = Entity<DataType>.NextResolveRequestId();
            return new ResolveRequestDTO
            {
                key = loadKey,
                forceUpdate = forceResolve,
                resolveRequestId = reqId
            };
        }

        /// <summary>
        /// Triggers an entity to resolve 
        /// (generally there is a ResolveEntityCmd command listening
        /// for this notification)
        /// </summary>
        /// <param name="dto">Dto.</param>
        /// <param name="opts">Opts.</param>
        public static int RequestResolve(
            string key,
            bool forceResolve = false,
            Opts opts = Opts.RequireReceiver
        )
        {
            return RequestResolve(NewResolveRequest(key, forceResolve));
        }

        public static int RequestResolve(
            ResolveRequestDTO req,
            Opts opts = Opts.RequireReceiver
        )
        {
            N.Send(RESOLVE_REQUESTED, req, opts);
            return req.resolveRequestId;
        }
        public static readonly string RESOLVE_REQUESTED = typeof(DataType).FullName + "_RESOLVE_REQUESTED";

        /// <summary>
        /// Called at the beginning of an Entity::Resolve request
        /// and triggers the EntityStore for the associated data type
        /// to update its resolve status to IN_PROGRESS for the entity id/key
        /// </summary>
        /// <param name="dto">Dto.</param>
        /// <param name="opts">Opts.</param>
        public static void ResolveStarted(ResolveRequestDTO req, Opts opts = Opts.RequireReceiver)
        {
            N.Send(RESOLVE_STARTED, req, opts);
        }
        public static readonly string RESOLVE_STARTED = typeof(DataType).FullName + "_RESOLVE_STARTED";

        /// <summary>
        /// Called at the end of a successful Entity::Resolve request
        /// and triggers the EntityStore for the associated data type
        /// to store/update the newly resolved entity.
        /// </summary>
        /// <param name="dto">Dto.</param>
        /// <param name="opts">Opts.</param>
        public static void ResolveSucceeded(ResolveSucceededDTO<DataType> dto, Opts opts = Opts.RequireReceiver)
        {
            N.Send(RESOLVE_SUCCEEDED, dto, opts);
        }
        public static readonly string RESOLVE_SUCCEEDED = typeof(DataType).FullName + "_RESOLVE_SUCCEEDED";

        /// <summary>
        /// Triggered when multiple entities have been resolved 
        /// in response to a single request
        /// </summary>
        /// <param name="dto">Dto.</param>
        /// <param name="opts">Opts.</param>
        public static void ResolvedMultiple(ResolvedMultipleDTO<DataType> dto, Opts opts = Opts.RequireReceiver)
        {
            N.Send(RESOLVED_MULTIPLE, dto, opts);
        }
        public static readonly string RESOLVED_MULTIPLE = typeof(DataType).FullName + "_RESOLVED_MULTIPLE";

        public static readonly string RESOLVE_FAILED = typeof(DataType).FullName + "_RESOLVE_FAILED";
        public static void ResolveFailed(ResolveFailedDTO dto, Opts opts = Opts.RequireReceiver)
        {
            N.Send(RESOLVE_FAILED, dto, opts);
        }


        /// <summary>
        /// Store/update an entity, e.g. in the associated EntityStore for the data type.
        /// 
        /// This has largely the same effect as Entity::ResolveSucceeded;
        /// the different is that Entity::Store is generally called
        /// when some actor wants to update an Entity and it is not within the 
        /// context of an Entity::Resolve request.
        /// 
        /// </summary>
        /// <param name="dto">Dto.</param>
        /// <param name="opts">Opts.</param>
        public static void Store(StoreEntityDTO<DataType> dto, Opts opts = Opts.RequireReceiver)
        {
            N.Send(STORE, dto, opts);
        }
        public static readonly string STORE = typeof(DataType).FullName + "_STORE";


        /// <summary>
        /// Sent to store/update multiple entities.
        /// Similar to RESOLVE_MULTIPLE but sent when there is no 
        /// triggering resolve request.
        /// </summary>
        /// <param name="dto">Dto.</param>
        /// <param name="opts">Opts.</param>
        public static void StoreMultiple(StoreMultipleDTO<DataType> dto, Opts opts = Opts.RequireReceiver)
        {
            N.Send(STORE_MULTIPLE, dto, opts);
        }
        public static readonly string STORE_MULTIPLE = typeof(DataType).FullName + "_STORE_MULTIPLE";



        /// <summary>
        /// Sent by an EntityStore immediately BEFORE 
        /// an entity is removed from the store.
        /// The entity will still be present in the store 
        /// when this notification is sent, and listeners may get/inspect it.
        /// </summary>
        /// <param name="id">Identifier.</param>
        /// <param name="opts">Opts.</param>
        public static void WillRemove(string id, Opts opts = Opts.DontRequireReceiver)
        {
            N.Send(WILL_REMOVE, id, opts);
        }
        public static readonly string WILL_REMOVE = typeof(DataType).FullName + "_WILL_REMOVE";


        /// <summary>
        /// Sent by an EntityStore immediately AFTER 
        /// an entity is removed from the store.
        /// The entity will NO LONGER be present in the store when listeners
        /// receive this notification.
        /// </summary>
        /// <param name="id">Identifier.</param>
        /// <param name="opts">Opts.</param>
        public static void DidRemove(string id, Opts opts = Opts.DontRequireReceiver)
        {
            N.Send(DID_REMOVE, id, opts);
        }
        public static readonly string DID_REMOVE = typeof(DataType).FullName + "_DID_REMOVE";

        public static readonly string UNLOAD_ALL_REQUESTED = typeof(DataType).FullName + "_UNLOAD_ALL_REQUESTED";
        public static void RequestUnloadAll(bool sendNotifications = true, Opts opts = Opts.RequireReceiver)
        {
            N.Send(UNLOAD_ALL_REQUESTED, sendNotifications, opts);
        }

        public static readonly string WILL_UNLOAD_ALL = typeof(DataType).FullName + "_WILL_UNLOAD_ALL";
        public static void WillUnloadAll(Opts opts = Opts.DontRequireReceiver)
        {
            N.Send(WILL_UNLOAD_ALL, opts);
        }

        public static readonly string DID_UNLOAD_ALL = typeof(DataType).FullName + "_DID_UNLOAD_ALL";
        public static void DidUnloadAll(Opts opts = Opts.DontRequireReceiver)
        {
            N.Send(DID_UNLOAD_ALL, opts);
        }

        /// <summary>
        /// Sent by an EntityStore when a single entity has been updated, 
        /// passing the id of the updated entity.
        /// </summary>
        public static void Updated(string id, Opts opts = Opts.DontRequireReceiver)
        {
            N.Send(UPDATED, id, opts);
        }
        public static readonly string UPDATED = typeof(DataType).FullName + "_UPDATED";

        public static bool RequestResolveIfExpiredOrUnresolved(string key, HasEntities<DataType> entities)
        {
            ResolveStatus status;
            if (!entities.GetResolveStatus(key, out status))
            {
                RequestResolve(key);
                return true;
            }
            if (status.IsExpiredAt(DateTimeOffset.Now))
            {
                RequestResolve(key);
                return true;
            }
            return false;
        }
#if NET_4_6
        /// <summary>
        /// Resolve the data for a key from an async method using await.
        /// NOTE: this will throw an exception if the item cannot be found.
        /// If you want to handle 'not found' or other errors without exceptions,
        /// use @see ResolveAsync instead; it returns a ResolveResult<DataType> 
        /// that would include details on failed resolves.
        /// </summary>
        public static async System.Threading.Tasks.Task<DataType> ResolveOrThrowAsync(
            string loadKey,
            HasEntities<DataType> store)
        {
            return await Resolve(loadKey, store);
        }

        /// <summary>
        /// Get or resolve the data for a set of entity keys
        /// with these policies:
        /// 
        /// - data for a key will be returned from local store if it is there (regardless of expiration status)
        /// - any key that fails to resolve (with error or otherwise) will simply be left out of results
        /// 
        /// The reason to use this over, say, ResolveAll, 
        /// is to avoid relatively expensive refreshes of data
        /// if what you really want is 'any valid copy of the data'
        /// </summary>
        /// <returns>The or resolve ignoring errors async.</returns>
        /// <param name="keys">Keys.</param>
        /// <param name="store">Store.</param>
        /// <param name="results">Results.</param>
        public static async System.Threading.Tasks.Task FindAllAsync(
            IEnumerable<string> keys,
            HasEntities<DataType> store,
            ICollection<DataType> results
        )
        {
            if (keys == null)
            {
                return;
            }

            foreach (var k in keys)
            {
                DataType cur;
                if (!store.GetData(k, out cur))
                {
                    try
                    {
                        cur = await Entity<DataType>.ResolveOrThrowAsync(k, store);
                    }
                    catch (Exception e)
                    {
#if UNITY_EDITOR || DEBUG_UNSTRIP
                        Debug.LogWarning("error resolving " + k
                                         + ": " + e.Message
                                         + "\n" + e.StackTrace);
#endif
                        continue;
                    }
                }
                results.Add(cur);
            }
        }

        /// <summary>
        /// Resolve the data and metadata for a key from an async method using await.
        /// NOTE: this method will return a ResolveResult<DataType> even if the item
        /// fails to load, e.g. either not found or resolve error.
        /// Use this version if you want to handle failed responses without try/catch.
        /// If you'd rather an exception be thrown any time the data isn't available,
        /// use @see ResolveOrThrowAsync.
        /// </summary>
        /// <returns>The async.</returns>
        public static async System.Threading.Tasks.Task<ResolveResultDTO<DataType>> ResolveAsync(
            string loadKey,
            HasEntities<DataType> store)
        {
            var req = NewResolveRequest(loadKey);
            if (string.IsNullOrEmpty(loadKey))
            {
                return ResolveResultDTO<DataType>.ResolveError(
                    req,
                    "Load key cannot be null or empty"
                );
            }
            var r = new ResolveResultRequest(req, store);
            try
            {
                await r.ExecuteAsync();
                return r.item;
            }
            catch (Exception e)
            {
#if UNITY_EDITOR || DEBUG_UNSTRIP
                var ae = e as AggregateException;
                if (ae != null)
                {
                    foreach (var ie in ae.InnerExceptions)
                    {
                        Debug.LogError("error on execute async for type "
                                       + typeof(DataType).Name
                                       + " and load key '" + loadKey
                                       + ": " + ie.Message
                                       + "\n" + ie.StackTrace
                                      );
                    }
                }
                else
                {
                    Debug.LogError("error on execute async for type "
                                   + typeof(DataType).Name
                                   + " and load key '" + loadKey
                                   + ": " + e.Message
                                  );
                }
#endif
                return ResolveResultDTO<DataType>.ResolveError(
                    req, e.Message
                );
            }
        }

        public static async System.Threading.Tasks.Task<ResolveMultipleResultDTO<DataType>> ResolveAllAsync(
            IEnumerable<string> keys,
            HasEntities<DataType> store)
        {
            return await ResolveAll(keys, store);
        }
#endif
        /// <summary>
        /// Allows you to request an entity (from the store) and get a callback when load succeeds or fails.
        /// If the entity is not initially loaded, sends the usual notifications and then listens for updates
        /// </summary>
        public static Request<DataType> Resolve(
            string loadKey,
            HasEntities<DataType> store,
            Action<Request<DataType>> callback = null)
        {
            if (string.IsNullOrEmpty(loadKey))
            {
                var err = new LocalRequest<DataType>("Load key cannot be null or empty");
                err.Execute(callback);
                return err;
            }
            var r = new DataRequest(NewResolveRequest(loadKey), store);
            r.Execute(callback);
            return r;
        }

        /// <summary>
        /// Allows you to request an entity (from the store) and get a callback when load succeeds or fails.
        /// If the entity is not initially loaded, sends the usual notifications and then listens for updates
        /// </summary>
        public static Request<ResolveMultipleResultDTO<DataType>> ResolveAll(
            IEnumerable<string> keys,
            HasEntities<DataType> store,
            Action<Request<ResolveMultipleResultDTO<DataType>>> callback = null)
        {
            var promise = new Promise<ResolveMultipleResultDTO<DataType>>((resolve, reject, cancel, attach) =>
            {
                var all = new JoinRequests();
                foreach (var k in keys)
                {
                    all.Add(new DataRequest(NewResolveRequest(k), store));
                }
                all.Execute(result =>
                {
                    var allResult = result as JoinRequests;
                    using (var resultRequests = ListPool<Request>.Get())
                    using (var resultItems = ListPool<ResolveResultDTO<DataType>>.Get())
                    {
                        allResult.GetResults(resultRequests);
                        foreach (var rr in resultRequests)
                        {
                            var key = (rr as DataRequest).loadKey;
                            var reqDTO = (rr as DataRequest).requestDTO;
                            if (rr.hasError)
                            {
                                resultItems.Add(
                                    ResolveResultDTO<DataType>.ResolveError(
                                        reqDTO, rr.error
                                    )
                                );
                                continue;
                            }
                            Entity<DataType> entity;
                            if (!store.GetEntity(key, out entity))
                            {
                                resultItems.Add(
                                    ResolveResultDTO<DataType>.ResolveError(
                                        reqDTO,
                                        "enity not in store after resolve"
                                    )
                                );
                                continue;
                            }
                            var status = entity.status;
                            resultItems.Add(ResolveResultDTO<DataType>.ResolveSucceeded(
                                reqDTO, entity.id, entity.data, status.maxAgeSecs, status.timestamp
                            ));
                        }
                        resolve(new ResolveMultipleResultDTO<DataType>
                        {
                            entities = resultItems.ToArray()
                        });
                    }
                });
            });
            promise.Execute(callback);
            return promise;

        }

        class ResolveResultRequest : RequestBase, Request<ResolveResultDTO<DataType>>
        {
            public ResolveResultRequest(ResolveRequestDTO req, HasEntities<DataType> store)
            {
                this.store = store;
                this.requestDTO = req;
            }

            public ResolveRequestDTO requestDTO { get; private set; }
            public int resolveRequestId { get { return this.requestDTO.resolveRequestId; } }
            public string loadKey { get { return this.requestDTO.key; } }
            public ResolveResultDTO<DataType> item { get; private set; }

            public object GetItem()
            {
                return this.item;
            }

            protected override void ExecuteRequest()
            {
                if (string.IsNullOrEmpty(this.loadKey))
                {
                    CompleteWithError("load key cannot be null or empty");
                    return;
                }
                CleanupBinding();
                this.storeBinding = N.Add<string>(UPDATED, this.OnStoreUpdate);
                Entity<DataType>.RequestResolve(this.requestDTO);
            }

            private void OnStoreUpdate(string id)
            {
                if (id != this.loadKey)
                {
                    return;
                }
                Entity<DataType> entity;
                if (!store.GetEntity(this.loadKey, out entity))
                {
                    var err = "entity not tracked in store (expected in progress or complete)";
                    this.item = ResolveResultDTO<DataType>.ResolveError(
                        this.requestDTO, err
                    );
                    CompleteWithError(err);
                    return;
                }
                ResolveStatus stat = entity.status;
                if (stat.isResolveInProgress)
                {
                    return;
                }
                if (!stat.hasResolved)
                {
                    var err = stat.resolveError ?? "not found";
                    this.item = ResolveResultDTO<DataType>.ResolveError(
                        this.requestDTO, err
                    );
                    CompleteWithError(err);
                    return;
                }
                this.item = ResolveResultDTO<DataType>.ResolveSucceeded(
                    this.requestDTO, entity.id, entity.data,
                    stat.maxAgeSecs, stat.timestamp
                );
                CompleteRequest();
            }

            protected override void DisposeRequest()
            {
                CleanupBinding();
                base.DisposeRequest();
            }

            protected override void AfterCompletionCallback()
            {
                CleanupBinding();
                base.AfterCompletionCallback();
            }

            private void CleanupBinding()
            {
                if (this.storeBinding != null)
                {
                    this.storeBinding.Unbind();
                    this.storeBinding = null;
                }
            }
            private Binding storeBinding { get; set; }
            private HasEntities<DataType> store { get; set; }

        }

        class DataRequest : RequestBase, Request<DataType>
        {
            public DataRequest(ResolveRequestDTO req, HasEntities<DataType> store)
            {
                this.store = store;
                this.requestDTO = req;
            }

            public ResolveRequestDTO requestDTO { get; private set; }
            public int resolveRequestId { get { return this.requestDTO.resolveRequestId; } }
            public string loadKey { get { return this.requestDTO.key; } }
            public DataType item { get; private set; }

            public object GetItem()
            {
                return this.item;
            }

            protected override void ExecuteRequest()
            {
                if (string.IsNullOrEmpty(this.loadKey))
                {
                    CompleteWithError("load key cannot be null or empty");
                    return;
                }
                CleanupBinding();
                this.storeBinding = N.Add<string>(UPDATED, this.OnStoreUpdate);
                Entity<DataType>.RequestResolve(this.requestDTO);
            }

            private void OnStoreUpdate(string id)
            {
                if (id != this.loadKey)
                {
                    return;
                }
                Entity<DataType> entity;
                if (!store.GetEntity(this.loadKey, out entity))
                {
                    CompleteWithError(
                        "entity not tracked in store (expected in progress or complete)"
                    );
                    return;
                }
                ResolveStatus stat = entity.status;
                if(stat.isResolveInProgress) {
                    return;
                }
                if (!stat.hasResolved)
                {
                    CompleteWithError(stat.resolveError ?? "not found");
                    return;
                }
                this.item = entity.data;
                CompleteRequest();
            }

            protected override void DisposeRequest()
            {
                CleanupBinding();
                base.DisposeRequest();
            }

            protected override void AfterCompletionCallback()
            {
                CleanupBinding();
                base.AfterCompletionCallback();
            }

            private void CleanupBinding()
            {
                if (this.storeBinding != null)
                {
                    this.storeBinding.Unbind();
                    this.storeBinding = null;
                }
            }

            private Binding storeBinding { get; set; }
            private HasEntities<DataType> store { get; set; }
        }
    }
}
