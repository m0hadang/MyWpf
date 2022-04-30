using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace MyCustomControl.Helper
{
    public class ResourceHelper
    {
        public static string GetString(string key) 
            => Application.Current.TryFindResource(key) as string;
        public static string GetString(string separator = ";", params string[] keyArr) 
            => string.Join(separator, keyArr.Select(key => Application.Current.TryFindResource(key) as string).ToList());
        public static List<string> GetStringList(params string[] keyArr) 
            => keyArr.Select(key => Application.Current.TryFindResource(key) as string).ToList();

        public static T GetResource<T>(string key)
        {
            if (Application.Current.TryFindResource(key) is T resource)
            {
                return resource;
            }

            return default;
        }
        public static ResourceDictionary GetResourceDic(string assemblyName, string path)
        {
            try
            {
                var uri = new Uri($"pack://application:,,,/{assemblyName};component{path}");
                return new ResourceDictionary
                {
                    Source = uri
                };
            }
            catch
            {
                return null;
            }
        }
        public static ResourceDictionary GetResourceDic(string path)
        {
            try
            {
                var uri = new Uri($"pack://application:,,,/;component{path}");
                return new ResourceDictionary
                {
                    Source = uri
                };
            }
            catch
            {
                return null;
            }
        }
    }

}
