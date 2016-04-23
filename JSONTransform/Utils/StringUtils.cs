namespace JSONTransform.Utils
{
    public static class StringUtils
    {
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
