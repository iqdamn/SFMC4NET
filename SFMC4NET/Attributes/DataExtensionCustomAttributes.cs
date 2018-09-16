using System;

namespace SFMC4NET.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class DataExtensionAttribute : Attribute
    {
        public DataExtensionAttribute()
        {
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class DEColumnAttribute : Attribute
    {
        /// <summary>
        /// Name of the column in SFMC Data Extension
        /// </summary>
        public string Name { get; set; }
        public DEColumnAttribute()
        {
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class IgnoreColumnAttribute : Attribute
    {
        public IgnoreColumnAttribute()
        {
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class KeyColumnAttribute : Attribute
    {
        public KeyColumnAttribute()
        {
        }
    }
}
