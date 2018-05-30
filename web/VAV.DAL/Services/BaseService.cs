using System.Collections.Generic;

namespace VAV.DAL.Services
{
    public class BaseService
    {
        private static Dictionary<string, object> VAVCache = new Dictionary<string, object>();

        public static void ResetVAVCache()
        {
            VAVCache = new Dictionary<string, object>();
        }

        protected static T CacheGet<T>(string key)
        {
            if (VAVCache.ContainsKey(key))
                return (T) VAVCache[key];
            
            return default(T);
        }

        protected static T CacheSet<T>(string key, T data)
        {
            if (!VAVCache.ContainsKey(key))
                VAVCache.Add(key, data);
            return (T)VAVCache[key];
        }

        protected static void CacheRemove(string key)
        {
            if (VAVCache.ContainsKey(key))
                VAVCache.Remove(key);
        }
    }
}
