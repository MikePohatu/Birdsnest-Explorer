using Microsoft.ConfigurationManagement.ManagementProvider;

namespace CMScanner.CmConverter
{
    public static class ResultObjectHandler
    {
        /// <summary>
        /// Attempts to return the value of a property, returns null on error
        /// </summary>
        /// <param name="resource"></param>
        /// <param name="property"></param>
        /// <returns></returns>
        public static string GetString(IResultObject resource, string property)
        {
            try
            {
                string s = resource[property].StringValue;
                return s;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Attempts to return the value of a propery, returns -1 on error
        /// </summary>
        /// <param name="resource"></param>
        /// <param name="property"></param>
        /// <returns></returns>
        public static int GetInt(IResultObject resource, string property)
        {
            try
            {
                int i = resource[property].IntegerValue;
                return i;
            }
            catch
            {
                return -1;
            }
        }

        /// <summary>
        /// Attempts to return the value of a propery, returns false on error
        /// </summary>
        /// <param name="resource"></param>
        /// <param name="property"></param>
        /// <returns></returns>
        public static bool GetBool(IResultObject resource, string property)
        {
            try
            {
                bool b = resource[property].BooleanValue;
                return b;
            }
            catch
            {
                return false;
            }
        }
    }
}
