﻿using System;
using DevExpress.ExpressApp.Editors;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using eXpand.ExpressApp.AdditionalViewControlsProvider.Model;

namespace eXpand.ExpressApp.AdditionalViewControlsProvider.Editors {
    public interface IModelAdditionalViewControlsItem : IModelDetailViewItem
    {
        [DataSourceProperty("Application.AdditionalViewControls.Rules")]
        [Required]
        IModelAdditionalViewControlsRule Rule { get; set; }
    }

    public abstract class AdditionalViewControlsItem:ViewItem {
        readonly IModelAdditionalViewControlsItem _model;

        protected AdditionalViewControlsItem(Type objectType, string id) : base(objectType, id) {
        }
        protected AdditionalViewControlsItem(IModelAdditionalViewControlsItem model, Type objectType)
            : base(objectType, model != null ? model.Id : string.Empty)
        {
            _model = model;    
        }

        public IModelAdditionalViewControlsItem Model {
            get { return _model; }
        }
    }
}