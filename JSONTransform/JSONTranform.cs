using JSONTransform.Models;
using JSONTransform.Utils;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace JSONTranform
{
    public static class Transformer
    {
        public static JToken TransformJSON(JToken source, JToken transformation)
        {
           // Starting transformation

            JToken returnJSON = null;

            if ((source != null) && (transformation != null))
            {
                // Valid source and transformation documents supplied

                if (source.GetType() == typeof(JObject))
                {
                    // Source of document determined to be JObject

                    returnJSON = TransformToken((JObject)source, transformation);
                }
                else if (source.GetType() == typeof(JArray))
                {
                    // Source of document determined to be JArray

                    returnJSON = new JArray();

                    foreach (JToken child in source.Children())
                    {
                        if (child.GetType() == typeof(JObject))
                        {
                            JToken transformedToken = TransformToken((JObject)source, transformation);

                            if (transformedToken != null) ((JArray)returnJSON).Add(transformedToken);
                        }
                    }
                }
            }

            return returnJSON;
        }

        private static JToken TransformToken(JObject source, JToken transformation)
        {
            // Parsing transformation token

            JToken returnJSON = null;

            if (transformation.GetType() == typeof(JObject))
            {
                // Transformation determined to be JObject

                returnJSON = TransformObject((JObject)source, (JObject)transformation);
            }
            else if (transformation.GetType() == typeof(JArray))
            {
               // Transformation determined to be JArray

                returnJSON = new JArray();

                foreach (JToken child in transformation.Children())
                {
                    if (child.GetType() == typeof(JObject))
                    {
                        JObject transformedObject = TransformObject((JObject)source, (JObject)child);

                        if (transformedObject != null) ((JArray)returnJSON).Add(transformedObject);
                    }
                }
            }

            return returnJSON;
        }

        private static JObject TransformObject(JObject source, JObject transformation)
        {
            // Parsing transformation object

            Dictionary<string, object> returnJSON = new Dictionary<string, object>();
            Dictionary<string, object> serializedTransformation = transformation.ToObject<Dictionary<string, object>>();
            
            foreach (string property in serializedTransformation.Keys)
            {
                Match match = Regex.Match(property, ReplaceUtils.TokenRegex);

                if (match.Success)
                {
                    if (match.Value.Length == property.Length)
                    {
                        string cleanedProperty = StringUtils.CleanProperty(property);

                        LogicalType? logicalType = LogicalUtils.GetLogicalType(cleanedProperty);

                        if (logicalType.HasValue)
                        {
                            string logicalMask = StringUtils.GetAsMask(cleanedProperty);
                            if (!String.IsNullOrEmpty(logicalMask)) cleanedProperty = StringUtils.GetPropertyWithoutMask(cleanedProperty);

                            if (logicalType == LogicalType.IF)
                            {
                                if (LogicalUtils.ParseValidIfCondition(cleanedProperty, source))
                                {
                                    JObject child = TransformObject(source, (JObject)serializedTransformation[property]);

                                    if (child != null)
                                    {
                                        if (String.IsNullOrEmpty(logicalMask))
                                        {
                                            Dictionary<string, object> serializedChild = child.ToObject<Dictionary<string, object>>();

                                            foreach (string childProperty in serializedChild.Keys)
                                            {
                                                returnJSON.Add(childProperty, serializedChild[childProperty]);
                                            }
                                        }
                                        else returnJSON.Add(logicalMask, child);
                                    }
                                    else return null;
                                }
                                else return null;
                            }
                            else if (logicalType == LogicalType.EACH)
                            {

                            }
                        }
                        else returnJSON.Add(ReplaceUtils.TransformString(property, source).ToString(), BuildResponseObject(source, property, serializedTransformation[property]));
                    }
                }
                else returnJSON.Add(ReplaceUtils.TransformString(property, source).ToString(), BuildResponseObject(source, property, serializedTransformation[property]));
            }

            return JObject.FromObject(returnJSON);
        }

        private static object BuildResponseObject(JObject source, string propertyName, object propertyValue)
        {
            // Building response JSON
            if (propertyValue.GetType() == typeof(string)) return ReplaceUtils.TransformString((string)propertyValue, source);
            else if (propertyValue.GetType() == typeof(JObject)) return TransformObject(source, (JObject)propertyValue);
            else if (propertyValue.GetType() == typeof(JArray)) return TransformObject(source, (JArray)propertyValue);
            else return propertyValue;
        }

        private static JArray TransformObject(JObject source, JArray transformation)
        {
            List<object> returnJSON = new List<object>();

            foreach (JToken child in transformation.Children())
            {
                if (child.Type == JTokenType.String) returnJSON.Add(ReplaceUtils.TransformString((string)child, source));
                else if (child.GetType() == typeof(JObject)) returnJSON.Add(TransformObject(source, (JObject)child));
                else returnJSON.Add(child);
            }

            return JArray.FromObject(returnJSON);
        }
    }
}
