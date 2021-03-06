﻿using System;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Security;
using DevExpress.ExpressApp.Security.ClientServer;
using DevExpress.ExpressApp.Xpo;
using DevExpress.Persistent.Base;
using DevExpress.Xpo;
using DevExpress.Xpo.DB;
using Xpand.Persistent.Base.General.Model;
using Xpand.Xpo;

namespace Xpand.Persistent.Base.General {
    public class XpandObjectSpaceProvider : XPObjectSpaceProvider, IXpandObjectSpaceProvider {
        readonly ISelectDataSecurityProvider _selectDataSecurityProvider;
        IDataLayer _dataLayer;
        bool _allowICommandChannelDoWithSecurityContext;
        ClientSideSecurity? _clientSideSecurity;


        public new IXpoDataStoreProxy DataStoreProvider { get; set; }

        public XpandObjectSpaceProvider(IXpoDataStoreProxy provider, ISelectDataSecurityProvider selectDataSecurityProvider,bool threadSafe=false)
            : base(provider, threadSafe) {
            _selectDataSecurityProvider = selectDataSecurityProvider;
            DataStoreProvider = provider;
            Tracing.Tracer.LogVerboseValue(GetType().FullName,Environment.StackTrace);
        }

        public ISelectDataSecurityProvider SelectDataSecurityProvider {
            get { return _selectDataSecurityProvider; }
        }

        public new IDataLayer WorkingDataLayer {
            get { return _dataLayer; }
        }

        public bool AllowICommandChannelDoWithSecurityContext {
            get { return _allowICommandChannelDoWithSecurityContext; }
            set { _allowICommandChannelDoWithSecurityContext = value; }
        }

        protected override IObjectSpace CreateObjectSpaceCore() {
            return new XpandObjectSpace(TypesInfo, XpoTypeInfoSource, CreateUnitOfWork);
        }

        IObjectSpace IObjectSpaceProvider.CreateUpdatingObjectSpace(Boolean allowUpdateSchema) {
            return CreateObjectSpace();
        }

        public void SetClientSideSecurity(ClientSideSecurity? clientSideSecurity) {
            _clientSideSecurity = clientSideSecurity;
        }
        public event EventHandler<CreatingWorkingDataLayerArgs> CreatingWorkingDataLayer;

        protected void OnCreatingWorkingDataLayer(CreatingWorkingDataLayerArgs e) {
            EventHandler<CreatingWorkingDataLayerArgs> handler = CreatingWorkingDataLayer;
            if (handler != null) handler(this, e);
        }
        protected override IDataLayer CreateDataLayer(IDataStore dataStore) {
            var creatingWorkingDataLayerArgs = new CreatingWorkingDataLayerArgs(dataStore);
            OnCreatingWorkingDataLayer(creatingWorkingDataLayerArgs);
            _dataLayer = creatingWorkingDataLayerArgs.DataLayer ?? base.CreateDataLayer(dataStore);
            return _dataLayer;
        }

        private XpandUnitOfWork CreateUnitOfWork() {
            var uow = new XpandUnitOfWork(DataLayer);

            if (SelectDataSecurityProvider == null)
                return uow;
            if (!_clientSideSecurity.HasValue || _clientSideSecurity.Value == ClientSideSecurity.UIlevel)
                return uow;
            var currentObjectLayer = new SecuredSessionObjectLayer(_allowICommandChannelDoWithSecurityContext, uow, true, null, new SecurityRuleProvider(XPDictionary, _selectDataSecurityProvider.CreateSelectDataSecurity()), null);
            return new XpandUnitOfWork(currentObjectLayer, uow);
        }
    }

}