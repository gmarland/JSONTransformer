using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace JSONTranform
{
    public static class Transformer
    {
        private static readonly string _tokenRegex = "{{(.+?)}}";

        #region Methods for transforming JSON

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

            // Checking for transformation condition

            bool hasCondition = false;
            string condition = String.Empty;

            foreach (string property in serializedTransformation.Keys)
            {
                if (property.ToLower() == "==")
                {
                    // Transformation condition found

                    hasCondition = true;
                    condition = TransformString((string)serializedTransformation[property], source).ToString();
                    break;
                }
            }

            if (!hasCondition) returnJSON = BuildResponseJSON(source, serializedTransformation);
            else
            {
                bool isValid = false;

                if (condition.Contains("!="))
                {
                    // Transformation condition is NEQ

                    string[] conditionParts = condition.Split(new char[] { '=' });

                    string leftSide = conditionParts[0].TrimEnd(new char[] { '!' }).Replace("'", String.Empty).Replace("\"", String.Empty).Trim();
                    string rightSide = conditionParts[1].Replace("'", String.Empty).Replace("\"", String.Empty).Trim();
                    
                    isValid = leftSide.ToLower() != rightSide.ToLower();
                }
                else if (condition.Contains("="))
                {
                    // Transformation condition is EQ

                    string[] conditionParts = condition.Split(new char[] { '=' });

                    string leftSide = conditionParts[0].Replace("'", String.Empty).Replace("\"", String.Empty).Trim();
                    string rightSide = conditionParts[1].Replace("'", String.Empty).Replace("\"", String.Empty).Trim();
                    
                    isValid = leftSide.ToLower() == rightSide.ToLower();
                }

                if (isValid) returnJSON = BuildResponseJSON(source, serializedTransformation);
                else return null;
            }

            return JObject.FromObject(returnJSON);
        }

        private static Dictionary<string, object> BuildResponseJSON(JObject source, Dictionary<string, object> serializedTransformation)
        {
            // Building response JSON

            Dictionary<string, object> returnJSON = new Dictionary<string, object>();

            foreach (string property in serializedTransformation.Keys)
            {
                if (property.ToLower() != "condition")
                {
                    if (serializedTransformation[property].GetType() == typeof(string)) returnJSON.Add(TransformString(property, source).ToString(), TransformString((string)serializedTransformation[property], source));
                    else if (serializedTransformation[property].GetType() == typeof(JObject)) returnJSON.Add(TransformString(property, source).ToString(), TransformObject(source, (JObject)serializedTransformation[property]));
                    else if (serializedTransformation[property].GetType() == typeof(JArray)) returnJSON.Add(TransformString(property, source).ToString(), TransformObject(source, (JArray)serializedTransformation[property]));
                    else returnJSON.Add(TransformString(property, source).ToString(), serializedTransformation[property]);
                }
            }

            return returnJSON;
        }

        private static JArray TransformObject(JObject source, JArray transformation)
        {
            List<object> returnJSON = new List<object>();

            foreach (JToken child in transformation.Children())
            {
                if (child.Type == JTokenType.String) returnJSON.Add(TransformString((string)child, source));
                else if (child.GetType() == typeof(JObject)) returnJSON.Add(TransformObject(source, (JObject)child));
                else returnJSON.Add(child);
            }

            return JArray.FromObject(returnJSON);
        }

        private static object TransformString(string target, JObject resource)
        {
            // Attempting to transform string

            string returnString = target;

            foreach (Match match in Regex.Matches(target, _tokenRegex))
            {
                // Match found

                string cleanedMatch = match.Value.Substring(2);
                cleanedMatch = cleanedMatch.Substring(0, cleanedMatch.Length - 2);

                string[] matchPath = cleanedMatch.Trim().Split(new char[] { '.' });
                JToken matchedObject = resource;

                if (matchPath.Length == 1) matchedObject = GetObjectProperty(matchPath[0], matchedObject);
                else
                {
                    foreach (string matchPathPart in matchPath)
                    {
                        matchedObject = GetObjectProperty(matchPathPart, matchedObject);

                        if (matchedObject == null) break;
                    }
                }

                if (matchedObject != null)
                {
                    if (matchedObject.Type == JTokenType.String) returnString = returnString.Replace(match.Value, (string)matchedObject);
                    else return matchedObject;
                }
                else throw new Exception("Unable to match value \"" + match.Value + "\"");
            }

            return returnString;
        }

        private static JToken GetObjectProperty(string property, JToken obj)
        {
            // Checking to see if an array is specified for matching

            Match arrayMatch = Regex.Match(property, @"\[[(0-9)]\]");

            if (arrayMatch.Success)
            {
                // An array match is required

                string propertyNameCleaned = property.Replace(arrayMatch.Value, String.Empty);

                // Checking that the requested property to match on is a array

                if (((JObject)obj)[propertyNameCleaned].GetType() == typeof(JArray))
                {
                    // Matched property is array

                    JArray objectArray = (JArray)((JObject)obj)[propertyNameCleaned];

                    string cleanedArrayMatch = arrayMatch.Value.Substring(1);
                    cleanedArrayMatch = cleanedArrayMatch.Substring(0, cleanedArrayMatch.Length - 1);

                    int arrayValue = Int32.Parse(cleanedArrayMatch);

                    if (arrayValue < objectArray.Count) return objectArray[arrayValue];
                    else return null;
                }
                else
                {
                    // Matched property is not an array

                    return null;
                }
            }
            else return ((JObject)obj)[property];
        }

        #endregion
    }
}
