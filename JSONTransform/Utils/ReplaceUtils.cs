using Newtonsoft.Json.Linq;
using System;
using System.Text.RegularExpressions;

namespace JSONTransform.Utils
{
    public static class ReplaceUtils
    {
        public static readonly string TokenRegex = "{{(.+?)}}";

        public static object TransformString(string target, JObject resource)
        {
            // Attempting to transform string

            string returnString = target;

            foreach (Match match in Regex.Matches(target, TokenRegex))
            {
                // Match found

                string cleanedMatch = StringUtils.CleanProperty(match.Value);

                JToken matchedObject = GetProperty(cleanedMatch, resource);

                if (matchedObject != null)
                {
                    if (matchedObject.Type == JTokenType.String) returnString = returnString.Replace(match.Value, (string)matchedObject);
                    else return matchedObject;
                }
                else return null;
            }

            return returnString;
        }

        public static JToken GetProperty(string target, JObject resource)
        {
            string[] matchPath = target.Split(new char[] { '.' });
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

            return matchedObject;
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

                    int arrayValue = Int32.Parse(StringUtils.CleanArrayProperty(arrayMatch.Value));

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
    }
}
