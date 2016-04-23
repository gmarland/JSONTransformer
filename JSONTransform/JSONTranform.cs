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

        private static JObject TransformObject(JObject source, JObject transformation, JObject iterationSource = null)
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
                                    JObject child = TransformObject(source, (JObject)serializedTransformation[property], iterationSource);

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
                                if (String.IsNullOrEmpty(logicalMask)) throw new Exception("Each statements require an AS statement");

                                IDictionary<string, string> eachProperties = LogicalUtils.ParseEachProperties(cleanedProperty);

                                JToken requestedEnumerate = ReplaceUtils.GetProperty(eachProperties["property"], source);

                                JArray iterationArray = new JArray();

                                if (requestedEnumerate.GetType() == typeof(JArray))
                                {
                                    if (serializedTransformation[property].GetType() == typeof(JObject))
                                    {
                                        foreach (JToken child in requestedEnumerate.Children())
                                        {
                                            JObject childObject = new JObject();
                                            childObject.Add(eachProperties["child"], child);

                                            iterationArray.Add(TransformObject(source, (JObject)serializedTransformation[property], childObject));
                                        }
                                    }
                                    else if (serializedTransformation[property].GetType() == typeof(JArray))
                                    {
                                        foreach (JToken child in requestedEnumerate.Children())
                                        {
                                            JObject childObject = new JObject();
                                            childObject.Add(eachProperties["child"], child);

                                            iterationArray.Add(TransformObject(source, (JArray)serializedTransformation[property], childObject));
                                        }
                                    }
                                }
                                else if (requestedEnumerate.GetType() == typeof(JObject))
                                {
                                    JObject childObject = new JObject();
                                    childObject.Add(eachProperties["child"], requestedEnumerate);

                                    if (serializedTransformation[property].GetType() == typeof(JObject)) iterationArray.Add(TransformObject(source, (JObject)serializedTransformation[property], childObject));
                                    else if (serializedTransformation[property].GetType() == typeof(JArray)) iterationArray.Add(TransformObject(source, (JArray)serializedTransformation[property], childObject));
                                }

                                returnJSON.Add(logicalMask, iterationArray);
                            }
                        }
                        else
                        {
                            string propertyName;

                            if (iterationSource != null)
                            {
                                object replaced = ReplaceUtils.TransformString(property, iterationSource);
                                if (replaced == null) replaced = ReplaceUtils.TransformString(property, source);

                                if (replaced == null) throw new Exception("Unable to match value \"" + property + "\"");
                                else propertyName = replaced.ToString();
                            }
                            else propertyName = ReplaceUtils.TransformString(property, source).ToString();

                            returnJSON.Add(propertyName, BuildResponseObject(source, property, serializedTransformation[property], iterationSource));
                        }

                    }
                }
                else
                {
                    string propertyName;

                    if (iterationSource != null)
                    {
                        object replaced = ReplaceUtils.TransformString(property, iterationSource);
                        if (replaced == null) replaced = ReplaceUtils.TransformString(property, source);

                        if (replaced == null) throw new Exception("Unable to match value \"" + property + "\"");
                        else propertyName = replaced.ToString();
                    }
                    else propertyName = ReplaceUtils.TransformString(property, source).ToString();

                    returnJSON.Add(propertyName, BuildResponseObject(source, property, serializedTransformation[property], iterationSource));
                }
            }

            return JObject.FromObject(returnJSON);
        }

        private static JArray TransformObject(JObject source, JArray transformation, JObject iterationSource = null)
        {
            List<object> returnJSON = new List<object>();

            foreach (JToken child in transformation.Children())
            {
                if (child.Type == JTokenType.String) returnJSON.Add(ReplaceUtils.TransformString((string)child, source));
                else if (child.GetType() == typeof(JObject)) returnJSON.Add(TransformObject(source, (JObject)child, iterationSource));
                else returnJSON.Add(child);
            }

            return JArray.FromObject(returnJSON);
        }

        private static object BuildResponseObject(JObject source, string propertyName, object propertyValue, JObject iterationSource = null)
        {
            // Building response JSON
            if (propertyValue.GetType() == typeof(string))
            {
                string property = (string)propertyValue;

                if (iterationSource != null)
                {
                    object replaced = ReplaceUtils.TransformString(property, iterationSource);
                    if (replaced == null) replaced = ReplaceUtils.TransformString(property, source);

                    if (replaced == null) throw new Exception("Unable to match value \"" + property + "\"");
                    else return replaced;
                }
                else
                {
                    object replaced = ReplaceUtils.TransformString(property, source);

                    if (replaced == null) throw new Exception("Unable to match value \"" + property + "\"");
                    else return replaced;
                }
            }
            else if (propertyValue.GetType() == typeof(JObject)) return TransformObject(source, (JObject)propertyValue, iterationSource);
            else if (propertyValue.GetType() == typeof(JArray)) return TransformObject(source, (JArray)propertyValue, iterationSource);
            else return propertyValue;
        }
    }
}
