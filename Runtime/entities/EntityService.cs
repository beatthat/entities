using BeatThat.DependencyInjection;
using BeatThat.Requests;
using System;

#if NET_4_6
using System.Threading.Tasks;
#endif

namespace BeatThat.Entities
{
    public class EntityService<DataType>
        : EntityStore<DataType>, EntityResolver<DataType>
    {
        sealed override protected void BindEntityStore()
        {
            Bind<ResolveRequestDTO>(
                ResolveEntityCmd<DataType>.Helper.notificationType,
                this.OnResolve);
            BindEntityService();
        }

        virtual protected void BindEntityService() { }

        virtual protected ResolveResultDTO<DataType> GetStoredEntityAsResolveResult(ResolveRequestDTO req)
        {
            return this.helper.GetStoredEntityAsResolveResult(req);
        }

#if NET_4_6
#pragma warning disable 1998
        virtual public async Task<ResolveResultDTO<DataType>> ResolveAsync(
            ResolveRequestDTO req
        )
#pragma warning restore 1998
        {
            return GetStoredEntityAsResolveResult(req);
        }
#endif

        private ResolveEntityCmd<DataType>.Helper m_cmd = null;
        private ResolveEntityCmd<DataType>.Helper cmd
        {
            get
            {
                return m_cmd ?? (m_cmd = new ResolveEntityCmd<DataType>.Helper(
                    this, this.resolver));
            }
        }
        private void OnResolve(ResolveRequestDTO req)
        {
            this.cmd.Execute(req);
        }

        virtual public Request<ResolveResultDTO<DataType>> Resolve(
            ResolveRequestDTO req,
            Action<Request<ResolveResultDTO<DataType>>> callback = null)
        {
            return this.helper.Resolve(req, callback);
        }

        private DefaultEntityResolver<DataType>.Helper m_helper;
        private DefaultEntityResolver<DataType>.Helper helper
        {
            get
            {
                return m_helper
                    ?? (m_helper = new DefaultEntityResolver<DataType>.Helper(this));
            }
        }
        [Inject] EntityResolver<DataType> resolver { get; set; }
    }
}