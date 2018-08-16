using System;
using BeatThat.Bindings;
using BeatThat.Notifications;
using BeatThat.Requests;

namespace BeatThat.Entities
{
    using N = NotificationBus;
    using Opts = NotificationReceiverOptions;
    public struct Entity<DataType> 
    {
        
        public DataType data;
        public ResolveStatus status;

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


        /// <summary>
        /// Allows you to request an entity (from the store) and get a callback when load succeeds or fails.
        /// If the entity is not initially loaded, sends the usual notifications and then listens for updates
        /// </summary>
        public static Request<DataType> Resolve(
            string loadKey, 
            HasEntities<DataType> store, 
            Action<Request<DataType>> callback)
        {
            if(string.IsNullOrEmpty(loadKey)) {
                var err = new LocalRequest<DataType>("Load key cannot be null or empty");
                err.Execute(callback);
                return err;
            }

            var r = new EntityRequest(loadKey, store);
            r.Execute(callback);
            return r;
        }

        class EntityRequest : RequestBase, Request<DataType>
        {
            public EntityRequest(string loadKey, HasEntities<DataType> store)
            {
                this.store = store;
                this.loadKey = loadKey;
            }

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
            private string loadKey { get; set; }

        }
    }
}
