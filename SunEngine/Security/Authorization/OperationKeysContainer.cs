﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SunEngine.DataBase;

namespace SunEngine.Security.Authorization
{
    /// <summary>
    /// This is registered Singleton Service. This container made for Main module. Others modules
    /// must have different name like: ModuleNameOperationKeysContainer
    /// </summary>
    public class OperationKeysContainer
    {
        public OperationKeysContainer(IDataBaseFactory dbFactory)
        {
            using (DataBaseConnection db = dbFactory.CreateDb())
            {
                Dictionary<string, int> dictionary = db.OperationKeys
                    .ToDictionary(x => x.Name, x => x.OperationKeyId);

                foreach (var propertyInfo in typeof(OperationKeysContainer).GetProperties())
                {
                    propertyInfo.SetValue(this, dictionary[propertyInfo.Name]);
                }
            }
        }


        public int MaterialAndMessagesRead { get; private set; }
        public int MaterialWrite { get; private set; }
        public int MaterialEditOwn { get; private set; }
        public int MaterialEditOwnIfTimeNotExceeded { get; private set; }
        public int MaterialEditOwnIfHasReplies { get; private set; }
        public int MaterialDeleteOwn { get; private set; }
        public int MaterialDeleteOwnIfTimeNotExceeded { get; private set; }
        public int MaterialDeleteOwnIfHasReplies { get; private set; }


        public int MessageWrite { get; private set; }
        public int MessageEditOwn { get; private set; }
        public int MessageEditOwnIfTimeNotExceeded { get; private set; }
        public int MessageEditOwnIfHasReplies { get; private set; }
        public int MessageDeleteOwn { get; private set; }
        public int MessageDeleteOwnIfTimeNotExceeded { get; private set; }
        public int MessageDeleteOwnIfHasReplies { get; private set; }


        // moderator

        [IsSuper] public int MaterialEditAny { get; private set; }
        [IsSuper] public int MaterialDeleteAny { get; private set; }

        [IsSuper] public int MessageEditAny { get; private set; }
        [IsSuper] public int MessageDeleteAny { get; private set; }

        /*
         
MaterialAndMessagesRead|MaterialWrite|MaterialEditOwn|MaterialEditOwnIfTimeNotExceeded|MaterialEditOwnIfHasReplies|MaterialDeleteOwn|MaterialDeleteOwnIfTimeNotExceeded|MaterialDeleteOwnIfHasReplies|MessageWrite|MessageEditOwn|MessageEditOwnIfTimeNotExceeded|MessageEditOwnIfHasReplies|MessageDeleteOwn|MessageDeleteOwnIfTimeNotExceeded|MessageDeleteOwnIfHasReplies|MaterialEditAny|MaterialDeleteAny|MessageEditAny|MessageDeleteAny



         */
        
        

        public static IList<string> GetAllOperationKeys()
        {
            var allKeys = typeof(OperationKeysContainer).GetProperties();
            return allKeys.Select(propertyInfo => propertyInfo.Name).ToList();
        }

        public static IList<string> GetAllSuperKeys()
        {
            var allSuperKeys = typeof(OperationKeysContainer).GetProperties();
            return allSuperKeys.Where(x => x.GetCustomAttribute<IsSuperAttribute>() != null)
                .Select(propertyInfo => propertyInfo.Name).ToList();
        }
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class IsSuperAttribute : Attribute
    {
        
    }
}