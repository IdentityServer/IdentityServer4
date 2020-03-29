// // Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// // Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
//
//
// using Newtonsoft.Json;
// using System;
// using System.Collections.Specialized;
// using System.Text.Json;
// using JsonSerializer = Newtonsoft.Json.JsonSerializer;
//
// #pragma warning disable 1591
//
// namespace IdentityServer4.Infrastructure
// {
//     public class NameValueCollectionJsonConverter : System.Text.Json.Serialization.JsonConverter<NameValueCollection>
//     {
//         public class NameValueCollectionItem
//         {
//             public string Key { get; set; }
//             public string[] Values { get; set; }
//         }
//         
//         public override NameValueCollection Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
//         {
//             throw new NotImplementedException();
//         }
//
//         public override void Write(Utf8JsonWriter writer, NameValueCollection value, JsonSerializerOptions options)
//         {
//             
//             {
//                 var items = new NameValueCollectionItem[value.AllKeys.Length];
//                 
//                 var index = 0;
//                 foreach (var key in value.AllKeys)
//                 {
//                     
//                     
//                     
//                     
//                     
//                     items[index++] = new NameValueCollectionItem
//                     {
//                         Key = key,
//                         Values = value.GetValues(key)
//                     };
//                 }
//
//                 writer.writes
//                 
//                 serializer.Serialize(writer, items);
//             }
//         }
//     }
//     
//     
//     public class NameValueCollectionConverter : JsonConverter
//     {
//         public class NameValueCollectionItem
//         {
//             public string Key { get; set; }
//             public string[] Values { get; set; }
//         }
//
//         public override bool CanConvert(Type objectType)
//         {
//             return objectType == typeof(NameValueCollection);
//         }
//
//         public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
//         {
//             var collection = existingValue as NameValueCollection;
//             if (collection == null) collection = new NameValueCollection();
//
//             var items = serializer.Deserialize<NameValueCollectionItem[]>(reader);
//             if (items != null)
//             {
//                 foreach (var item in items)
//                 {
//                     if (item.Values != null)
//                     {
//                         foreach (var value in item.Values)
//                         {
//                             collection.Add(item.Key, value);
//                         }
//                     }
//                 }
//             }
//
//             return collection;
//         }
//
//         public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
//         {
//             var collection = (NameValueCollection)value;
//             if (collection != null)
//             {
//                 var items = new NameValueCollectionItem[collection.AllKeys.Length];
//                 var index = 0;
//                 foreach (var key in collection.AllKeys)
//                 {
//                     items[index++] = new NameValueCollectionItem
//                     {
//                         Key = key,
//                         Values = collection.GetValues(key)
//                     };
//                 }
//
//                 serializer.Serialize(writer, items);
//             }
//         }
//     }
// }
