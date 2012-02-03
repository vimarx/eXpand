using System;
using System.Linq;
using DevExpress.ExpressApp;
using DevExpress.Xpo.DB;
using Xpand.ExpressApp.WorldCreator.Core;
using Xpand.Persistent.Base.PersistentMetaData;
using Xpand.Persistent.Base.PersistentMetaData.PersistentAttributeInfos;

namespace Xpand.ExpressApp.WorldCreator.DBMapper {
    public class MemberAttributeGenerator {
        readonly IPersistentMemberInfo _persistentMemberInfo;
        readonly DBColumn _column;
        readonly bool _isPrimaryKey;
        readonly IObjectSpace _objectSpace;
        readonly DBTable _dbTable;


        public MemberAttributeGenerator(MemberGeneratorInfo memberGeneratorInfo, ClassGeneratorInfo classGeneratorInfo) {
            _objectSpace = ObjectSpace.FindObjectSpaceByObject(memberGeneratorInfo.PersistentMemberInfo);
            _persistentMemberInfo = memberGeneratorInfo.PersistentMemberInfo;
            _column = memberGeneratorInfo.DbColumn;
            _isPrimaryKey = CalculatePrimaryKey(memberGeneratorInfo, classGeneratorInfo);
            _dbTable = classGeneratorInfo.DbTable;
        }

        bool CalculatePrimaryKey(MemberGeneratorInfo memberGeneratorInfo, ClassGeneratorInfo classGeneratorInfo) {
            if (classGeneratorInfo.DbTable.PrimaryKey != null && memberGeneratorInfo.DbColumn != null)
                return classGeneratorInfo.DbTable.PrimaryKey.Columns.Contains(memberGeneratorInfo.DbColumn.Name);
            return false;
        }

        public void Create() {
            var isCompoundPrimaryKey = IsCompoundPrimaryKey();
            var persistentAttributeInfos = _persistentMemberInfo.TypeAttributes;
            if (!isCompoundPrimaryKey && _column.ColumnType == DBColumnType.String) {
                persistentAttributeInfos.Add(GetPersistentSizeAttribute());
            }
            if (_isPrimaryKey && _persistentMemberInfo.Owner.CodeTemplateInfo.CodeTemplate.TemplateType != TemplateType.Struct) {
                persistentAttributeInfos.Add(GetPersistentKeyAttribute());
            }
            if (IsSimpleForeignKey() || ((isCompoundPrimaryKey && IsForeignKey())))
                persistentAttributeInfos.Add(GetPersistentAssociationAttribute());
            var persistentPersistentAttribute = GetPersistentPersistentAttribute();
            if (((IsCompoundForeignKey())) || isCompoundPrimaryKey)
                persistentPersistentAttribute.MapTo = String.Empty;
            persistentAttributeInfos.Add(persistentPersistentAttribute);
        }

        bool IsForeignKey() {
            return _dbTable.ForeignKeys.Where(key => key.Columns.Contains(_column.Name)).FirstOrDefault() != null && !_isPrimaryKey;
        }

        bool IsCompoundForeignKey() {
            return _dbTable.ForeignKeys.Where(key => key.Columns.Contains(_column.Name) && key.Columns.Count > 1).FirstOrDefault() != null;
        }

        IPersistentPersistentAttribute GetPersistentPersistentAttribute() {
            var persistentPersistentAttribute = _objectSpace.CreateWCObject<IPersistentPersistentAttribute>();
            persistentPersistentAttribute.MapTo = _column.Name;
            return persistentPersistentAttribute;
        }

        IPersistentAssociationAttribute GetPersistentAssociationAttribute() {
            var persistentAssociationAttribute = _objectSpace.CreateWCObject<IPersistentAssociationAttribute>();
            persistentAssociationAttribute.AssociationName = String.Format("{0}-{1}s", _persistentMemberInfo.Name, _persistentMemberInfo.Owner.Name + _persistentMemberInfo.Name + "s");
            return persistentAssociationAttribute;
        }

        bool IsSimpleForeignKey() {
            if (IsCompoundPrimaryKey())
                return false;
            return _dbTable.ForeignKeys.Where(key => key.Columns.Count == 1 && key.Columns.Contains(_column.Name)).FirstOrDefault() != null;
        }

        bool IsCompoundPrimaryKey() {
            if (_persistentMemberInfo is IPersistentReferenceMemberInfo && ((IPersistentReferenceMemberInfo)_persistentMemberInfo).ReferenceClassInfo.CodeTemplateInfo.CodeTemplate.TemplateType == TemplateType.Struct)
                return true;
            return false;
        }

        IPersistentKeyAttribute GetPersistentKeyAttribute() {
            var persistentAttributeInfo = _objectSpace.CreateWCObject<IPersistentKeyAttribute>();
            persistentAttributeInfo.AutoGenerated = _column.IsIdentity;
            return persistentAttributeInfo;
        }

        IPersistentSizeAttribute GetPersistentSizeAttribute() {
            var persistentSizeAttribute = _objectSpace.CreateWCObject<IPersistentSizeAttribute>();
            persistentSizeAttribute.Size = _column.Size;
            return persistentSizeAttribute;
        }

    }
}