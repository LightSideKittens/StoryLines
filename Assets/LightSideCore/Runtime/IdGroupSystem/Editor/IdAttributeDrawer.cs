﻿using System.Collections.Generic;
using System.Linq;

namespace LSCore
{
    public class IdAttributeDrawer : BaseValueDropdownDrawer<IdAttribute>
    {
        protected override object GetValue()
        {
            IEnumerable<Id> source;
            var names = Attribute.groupNames;
            var type = Attribute.groupType;
            
            if (names is { Length: > 0 } && type != null && typeof(IdGroup).IsAssignableFrom(type))
            {
                source = (IdGroup)AssetDatabaseUtils.LoadAny(type, names[0]);
                
                for (int i = 1; i < names.Length; i++)
                {
                    source = source.Concat((IdGroup)AssetDatabaseUtils.LoadAny(type, names[i]));
                }
            }
            else if(names is { Length: > 0 })
            {
                source = AssetDatabaseUtils.LoadAny<IdGroup>(names[0]);
                
                for (int i = 1; i < names.Length; i++)
                {
                    source = source.Concat(AssetDatabaseUtils.LoadAny<IdGroup>(names[i]));
                }
            }
            else if(type != null && typeof(IdGroup).IsAssignableFrom(type))
            {
                source = (IdGroup)AssetDatabaseUtils.LoadAny(type);
            }
            else
            {
                source = AssetDatabaseUtils.LoadAllAssets<Id>();
            }

            return source;
        }
    }
}