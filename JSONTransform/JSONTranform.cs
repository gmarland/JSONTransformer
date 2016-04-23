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
        // Entry point to the method. The main purpose is to determine if this is an object or an array.
        // Objects only have the transformation applied to them. An array has the transformation applied to each entry.
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

        // The real start to the transformation. This takes the object and the transformation to be applied.
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

                // loop through the trnaformation and replace in the entries for the source
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

        // This method takes the current position in the transformation and applied the source object to it.
        // If this is part of an iteration loop then the object currently being iterated on is also passed for translation
        private static JObject TransformObject(JObject source, JObject transformation, JObject iterationSource = null)
        {
            // Parsing transformation object

            Dictionary<string, object> returnJSON = new Dictionary<string, object>();
            Dictionary<string, object> serializedTransformation = transformation.ToObject<Dictionary<string, object>>();
            
            // Loop through all the propertied of the transformation at this level
            foreach (string property in serializedTransformation.Keys)
            {
                // search the current transformation object for any replacement keys in the form {{ ... }}.
                // There may only be 1 replacement on the property level
                Match match = Regex.Match(property, ReplaceUtils.TokenRegex);

                if (match.Success)
                {
                    // Check if the match length is the same as the property length. This is a higher percentage of being a conditional property
                    if (match.Value.Length == property.Length)
                    {
                        string cleanedProperty = StringUtils.CleanProperty(property);

                        // Check if this is a conditional property. Eg is this an IF or an each statement
                        LogicalType? logicalType = LogicalUtils.GetLogicalType(cleanedProperty);

                        if (logicalType.HasValue)
                        {
                            string logicalMask = StringUtils.GetAsMask(cleanedProperty);
                            if (!String.IsNullOrEmpty(logicalMask)) cleanedProperty = StringUtils.GetPropertyWithoutMask(cleanedProperty);

                            if (logicalType == LogicalType.IF)
                            {
                                // Parse the if to see if it validates out true or false
                                if (LogicalUtils.ParseValidIfCondition(cleanedProperty, source))
                                {
                                    JObject child = TransformObject(source, (JObject)serializedTransformation[property], iterationSource);

                                    if (child != null)
                                    {
                                        // Check if there is a logical mask to be applied. By this, does it have an AS statement included.
                                        // If there is an AS statement then the child object is appended to a poperty named that. If not, it is attached to the parent
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
                                // Check if there is an AS in the each statement. Eaches need a property to be applied to.
                                if (String.IsNullOrEmpty(logicalMask)) throw new Exception("Each statements require an AS statement");

                                IDictionary<string, string> eachProperties = LogicalUtils.ParseEachProperties(cleanedProperty);

                                // Retrieve the property to be iterated on in the each
                                JToken requestedEnumerate = ReplaceUtils.GetProperty(eachProperties["property"], source);

                                JArray iterationArray = new JArray();

                                // Loop through the retrieved object and apply the translation to each entry
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

                                // Attach the transated array to a property with the name in the AS statement
                                returnJSON.Add(logicalMask, iterationArray);
                            }
                        }
                        else
                        {
                            string propertyName;

                            // Check if this is part of an EACH statement. If so check that for properties first
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

                    // Check if this is part of an EACH statement. If so check that for properties first
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

        // This is for when the target translation piece is an array. We need to iterate through it and apply the translation to it children
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

        // This does the replacement at the property level. It tries to replace in the object in the source to the translation
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
