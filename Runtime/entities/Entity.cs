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
    public struct Entity<DataType> 
    {
        public string id;
        public DataType data;
        public ResolveStatus status;

        /// <summary>
        /// hacky flag to enable debugging (usually temporarily) for a single entity type
        /// </summary>
        public static bool DEBUG = false;


        public bool GetData(out DataType data)
        {
            if(!this.status.hasResolved) {
                data = default(DataType);
                return false;
            }
            data = this.data;
            return true;
        }


        public static readonly string RESOLVE_REQUESTED = typeof(DataType).FullName + "_RESOLVE_REQUESTED";
        public static void RequestResolve(string key, Opts opts = Opts.RequireReceiver)
        {
            RequestResolve(new ResolveRequestDTO { key = key }, opts);
        }

        public static void RequestResolve(ResolveRequestDTO dto, Opts opts = Opts.RequireReceiver)
        {
            N.Send(RESOLVE_REQUESTED, dto, opts);
        }

        public static readonly string RESOLVE_STARTED = typeof(DataType).FullName + "_RESOLVE_STARTED";
        public static void ResolveStarted(string id, Opts opts = Opts.RequireReceiver)
        {
            N.Send(RESOLVE_STARTED, id, opts);
        }

        public static readonly string RESOLVE_SUCCEEDED = typeof(DataType).FullName + "_RESOLVE_SUCCEEDED";
        public static void ResolveSucceeded(ResolveSucceededDTO<DataType> dto, Opts opts = Opts.RequireReceiver)
        {
            N.Send(RESOLVE_SUCCEEDED, dto, opts);
        }

        public static readonly string RESOLVED_MULTIPLE = typeof(DataType).FullName + "_RESOLVED_MULTIPLE";
        public static void ResolvedMultiple(ResolvedMultipleDTO<DataType> dto, Opts opts = Opts.RequireReceiver)
        {
            N.Send(RESOLVED_MULTIPLE, dto, opts);
        }

        public static readonly string RESOLVE_FAILED = typeof(DataType).FullName + "_RESOLVE_FAILED";
        public static void ResolveFailed(ResolveFailedDTO dto, Opts opts = Opts.RequireReceiver)
        {
            N.Send(RESOLVE_FAILED, dto, opts);
        }

        public static readonly string WILL_REMOVE = typeof(DataType).FullName + "_WILL_REMOVE";
        public static void WillRemove(string id, Opts opts = Opts.DontRequireReceiver)
        {
            N.Send(WILL_REMOVE, id, opts);
        }

        public static readonly string DID_REMOVE = typeof(DataType).FullName + "_DID_REMOVE";
        public static void DidRemove(string id, Opts opts = Opts.DontRequireReceiver)
        {
            N.Send(DID_REMOVE, id, opts);
        }

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

        public static readonly string UPDATED = typeof(DataType).FullName + "_UPDATED";
        public static void Updated(string id, Opts opts = Opts.DontRequireReceiver)
        {
            N.Send(UPDATED, id, opts);
        }

        public static bool RequestResolveIfExpiredOrUnresolved(string key, HasEntities<DataType> entities)
        {
            ResolveStatus status;
            if(!entities.GetResolveStatus(key, out status)) {
                RequestResolve(key);
                return true;
            }

            if(status.IsExpiredAt(DateTimeOffset.Now)) {
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
            if (string.IsNullOrEmpty(loadKey))
            {
                return ResolveResultDTO<DataType>.ResolveError(loadKey, "Load key cannot be null or empty");
            }

            try
            {
                var r = new ResolveResultRequest(loadKey, store);
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
                                       + typeof(DataType).Name + " and load key '" + loadKey + ": " + ie.Message + "\n" + ie.StackTrace);
                    }
                }
                else {
                    Debug.LogError("error on execute async for type "
                                   + typeof(DataType).Name + " and load key '" + loadKey + ": " + e.Message);
                }
#endif
                return ResolveResultDTO<DataType>.ResolveError(loadKey, e.Message);
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
            if(string.IsNullOrEmpty(loadKey)) {
                var err = new LocalRequest<DataType>("Load key cannot be null or empty");
                err.Execute(callback);
                return err;
            }

            var r = new DataRequest(loadKey, store);
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
                foreach(var k in keys) {
                    all.Add(new DataRequest(k, store));
                }

                all.Execute(result => {
                    var allResult = result as JoinRequests;

                    //ResolveResultDTO<DataType> cur;

                    using(var resultRequests = ListPool<Request>.Get())
                    using(var resultItems = ListPool<ResolveResultDTO<DataType>>.Get()) {
                        allResult.GetResults(resultRequests);
                        foreach (var rr in resultRequests)
                        {
                            var key = (rr as DataRequest).loadKey;
                            if (rr.hasError)
                            {
                                resultItems.Add(new ResolveResultDTO<DataType>
                                {
                                    id = key,
                                    key = key,
                                    status = ResolveStatusCode.ERROR,
                                    message = rr.error
                                });
                                continue;
                            }

                            Entity<DataType> entity;
                            if (!store.GetEntity(key, out entity))
                            {
                                resultItems.Add(new ResolveResultDTO<DataType>
                                {
                                    id = key,
                                    key = key,
                                    status = ResolveStatusCode.ERROR,
                                    message = "enity not in store after resolve"
                                });
                                continue;
                            }

                            var status = entity.status;

                            resultItems.Add(new ResolveResultDTO<DataType>
                            {
                                id = key,
                                key = key,
                                status = ResolveStatusCode.OK,
                                data = entity.data,
                                maxAgeSecs = status.maxAgeSecs,
                                timestamp = status.timestamp
                            });
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
            public ResolveResultRequest(string loadKey, HasEntities<DataType> store)
            {
                this.store = store;
                this.loadKey = loadKey;
            }

            public string loadKey { get; private set; }
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

                if (TryComplete(false))
                {
                    return;
                }

                CleanupBinding();
                this.storeBinding = N.Add<string>(UPDATED, this.OnStoreUpdate);
                Entity<DataType>.RequestResolve(loadKey);
            }

            private bool TryComplete(bool completeOnErrorOrNotFound = true)
            {
                Entity<DataType> entity;
                if(!store.GetEntity(this.loadKey, out entity)) {
                    return false;
                }

                ResolveStatus resolveStatus = entity.status;

                if(resolveStatus.isResolveInProgress) {
                    return false;
                }

                if(resolveStatus.hasResolved) {
                    item = new ResolveResultDTO<DataType>
                    {
                        status = ResolveStatusCode.OK,
                        id = entity.id,
                        key = this.loadKey,
                        data = entity.data,
                        timestamp = resolveStatus.timestamp,
                        maxAgeSecs = resolveStatus.maxAgeSecs
                    };
                    CompleteRequest();
                    return true;
                }

                if (!completeOnErrorOrNotFound)
                {
                    return false;
                }

                if (!string.IsNullOrEmpty(resolveStatus.resolveError))
                {
                    this.item = ResolveResultDTO<DataType>.ResolveError(
                        this.loadKey, resolveStatus.resolveError);
                    
                    CompleteWithError(resolveStatus.resolveError);
                    return true;
                }

                this.item = ResolveResultDTO<DataType>.ResolveNotFound(this.loadKey);
                return true;
            }

            private void OnStoreUpdate(string id)
            {
                if (id != this.loadKey)
                {
                    return;
                }

                TryComplete(true);
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
            public DataRequest(string loadKey, HasEntities<DataType> store)
            {
                this.store = store;
                this.loadKey = loadKey;
            }

            public string loadKey { get; private set; }
            public DataType item { get; private set; }

            public object GetItem()
            {
                return this.item;
            }

            protected override void ExecuteRequest()
            {
                if(string.IsNullOrEmpty(this.loadKey)) {
                    CompleteWithError("load key cannot be null or empty");
                    return;
                }

                if(TryComplete(false)) {
                    return;
                }

                CleanupBinding();
                this.storeBinding = N.Add<string>(UPDATED, this.OnStoreUpdate);
                Entity<DataType>.RequestResolve(loadKey);
            }

            private bool TryComplete(bool failOnError = true)
            {
                DataType data;
                if (this.store.GetData(this.loadKey, out data))
                {
                    this.item = data;
                    CompleteRequest();
                    return true;
                }

                if(!failOnError) {
                    return false;
                }

                ResolveStatus loadStatus;
                if (!store.GetResolveStatus(loadKey, out loadStatus))
                {
                    return false;
                }

                if(loadStatus.isResolveInProgress) {
                    return false;
                } 

                if(!string.IsNullOrEmpty(loadStatus.resolveError))
                {
                    CompleteWithError(loadStatus.resolveError);
                    return true;
                }

                return false;
            }

            private void OnStoreUpdate(string id)
            {
                if(id != this.loadKey) {
                    return;
                }

                TryComplete(true);
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
