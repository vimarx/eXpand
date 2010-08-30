﻿using System;
using DevExpress.ExpressApp.Updating;
using DevExpress.Xpo;
using FeatureCenter.Module.BaseObjects;

namespace FeatureCenter.Module.Navigation {
    public class Updater:ModuleUpdater {
        public Updater(Session session, Version currentDBVersion) : base(session, currentDBVersion) {
        }
        public override void UpdateDatabaseAfterUpdateSchema()
        {
            base.UpdateDatabaseAfterUpdateSchema();
            new CustomerBuilder<DetailViewNavigationObjectCustomer>(Session).CreateCustomers();
        }
    }
}