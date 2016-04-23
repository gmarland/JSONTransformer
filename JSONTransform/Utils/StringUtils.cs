using System;

namespace JSONTransform.Utils
{
    public static class StringUtils
    {
        public static string GetAsMask(string property)
        {
            if (property.Contains("AS"))
            {
                string[] propertyParts = property.Split(new string[] { "AS "}, StringSplitOptions.None);

                return propertyParts[(propertyParts.Length - 1)].Trim();
            }
            return String.Empty;
        }

        public static string GetPropertyWithoutMask(string property)
        {
            if (property.Contains("AS"))
            {
                string[] propertyParts = property.Split(new string[] { "AS " }, StringSplitOptions.None);

                return propertyParts[0].Trim();
            }
            return String.Empty;
        }

        public static string CleanArrayProperty(string property)
        {
            string propertyMatch = property.Substring(1);
            propertyMatch = propertyMatch.Substring(0, propertyMatch.Length - 1);

            return propertyMatch.Trim();
        }

        public static string CleanProperty(string property)
        {
            string propertyMatch = property.Substring(2);
            propertyMatch = propertyMatch.Substring(0, propertyMatch.Length - 2);

            return propertyMatch.Trim();
        }
    }
}
