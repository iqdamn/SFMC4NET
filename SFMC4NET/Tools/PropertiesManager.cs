using SFMC4NET.Attributes;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace SFMC4NET.Tools
{
    internal class PropertiesManager
    {
        /// <summary>
        /// Gets all public properties and generates part of the request message based on the properties names
        /// </summary>
        /// <returns>The name of the properties inside a Properties tag</returns>
        public IList<string> GetDataExtensionFields<T>()
        {
            List<string> propList = new List<string>();

            foreach (var prop in typeof(T).GetProperties())
            {
                var ignoreAttribute = prop.GetCustomAttributes(typeof(IgnoreColumnAttribute), false);

                if (ignoreAttribute != null && ignoreAttribute.Length > 0)
                    continue;

                var columnAttribute = prop.GetCustomAttributes(typeof(DEColumnAttribute), false);
                var propertyName = prop.Name;

                //Checking if this field has been decorated with DEColumnAttribute, if not, the property name is used
                //otherwise, we use the name passed in the attribute
                if (columnAttribute != null && columnAttribute.Length > 0)
                {
                    var attr = (DEColumnAttribute)columnAttribute[0];
                    propertyName = attr.Name;
                }

                propList.Add(propertyName);
            }

            return propList;
        }

        public Dictionary<string, string> GetKeys(object item)
        {
            Dictionary<string, string> keys = new Dictionary<string, string>();
            
            foreach (var property in item.GetType().GetProperties())
            {
                var ignoreColumnProperty = property.GetCustomAttributes(typeof(IgnoreColumnAttribute), false);

                if (ignoreColumnProperty != null && ignoreColumnProperty.Length > 0)
                    continue;

                string columnName = property.Name;

                var customNameProperty = property.GetCustomAttributes(typeof(DEColumnAttribute), false);
                if (customNameProperty != null && customNameProperty.Length > 0)
                {
                    columnName = ((DEColumnAttribute)customNameProperty[0]).Name;
                }

                var attributes = property.GetCustomAttributes(typeof(KeyColumnAttribute), false);

                if(attributes != null && attributes.Length > 0)
                {
                    keys.Add(columnName, property.GetValue(item).ToString());
                }
            }


            return keys;
        }

        public Dictionary<string, string> GeValues(object item)
        {
            Dictionary<string, string> keys = new Dictionary<string, string>();

            foreach (var property in item.GetType().GetProperties())
            {
                var ignoreColumnProperty = property.GetCustomAttributes(typeof(IgnoreColumnAttribute), false);

                if (ignoreColumnProperty != null && ignoreColumnProperty.Length > 0)
                    continue;

                string columnName = property.Name;

                var customNameProperty = property.GetCustomAttributes(typeof(DEColumnAttribute), false);
                if (customNameProperty != null && customNameProperty.Length > 0)
                {
                    columnName = ((DEColumnAttribute)customNameProperty[0]).Name;
                }

                var attributes = property.GetCustomAttributes(typeof(KeyColumnAttribute), false);

                if (attributes != null && attributes.Length > 0)
                    continue;

                var propValue = property.GetValue(item);
                string strValue = string.Empty;

                if(propValue != null)
                { 
                    //SFMC requires a very specific format for DateTime
                    if (propValue.GetType() == typeof(DateTime))
                    {
                        DateTime dateTime = (DateTime)propValue;
                        strValue = dateTime.ToString("yyyy-MM-ddTHH:mm:ss");
                    }
                    else
                        strValue = propValue.ToString();
                }

                keys.Add(columnName, strValue);
                
            }

            return keys;
        }

        /// <summary>
        /// Creates an instance of T taking into consideration the Data Extension attributes
        /// </summary>
        /// <typeparam name="T">Entity type</typeparam>
        /// <param name="properties">Dictionary containing all properties that has to be mapped to T</param>
        /// <returns>Instance of T</returns>
        public void CreateInstance<T>(T obj, Dictionary<string, string> properties)
        {
            var propertyList = obj.GetType().GetProperties();

            foreach (var currentProperty in propertyList)
            {
                string columnName = currentProperty.Name;

                var ignoreColumnProperty= currentProperty.GetCustomAttributes(typeof(IgnoreColumnAttribute), false);

                if (ignoreColumnProperty != null && ignoreColumnProperty.Length > 0)
                    continue;

                var customNameProperty = currentProperty.GetCustomAttributes(typeof(DEColumnAttribute), false);
                if (customNameProperty != null && customNameProperty.Length > 0)
                {
                    columnName = ((DEColumnAttribute)customNameProperty[0]).Name;
                }

                if(properties.ContainsKey(columnName))
                {
                    Type t = Nullable.GetUnderlyingType(currentProperty.PropertyType) ?? currentProperty.PropertyType;

                    var propValue = properties[columnName];

                    if (propValue != null && !string.IsNullOrEmpty(propValue.ToString()))
                    {
                        if(t == typeof(DateTime))
                        {
                            currentProperty.SetValue(obj, DateTime.ParseExact(propValue, "M/d/yyyy h:mm:ss tt", CultureInfo.InvariantCulture));
                        }
                        else
                        {
                            var propertyValue = Convert.ChangeType(propValue, t);
                            currentProperty.SetValue(obj, propertyValue);
                        }
                    }
                    else
                    {
                        if (Nullable.GetUnderlyingType(currentProperty.PropertyType) != null)
                            currentProperty.SetValue(obj, null);
                        else
                            currentProperty.SetValue(obj, propValue);
                    }
                }
            }
        }
    }
}
