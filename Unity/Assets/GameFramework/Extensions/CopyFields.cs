using System;
using System.Linq;
using System.Reflection;
/// <summary>
/// A static class for reflection type functions
/// </summary>
public static partial class Reflection
{
    /// <summary>
    /// Extension for 'Object' that copies the fields to a destination object.
    /// </summary>
    /// <param name="source">Source.</param>
    /// <param name="destination">Destination.</param>
    public static void CopyFields(this object source, object destination)
    {
        // If any this null throw an exception
        if (source == null || destination == null)  return;
        //throw new Exception("Source or/and Destination Objects are null");

        // Getting the Types of the objects
        Type typeDest = destination.GetType();
        Type typeSrc = source.GetType();
        // Collect all the valid properties to map
        var results = from srcField in typeSrc.GetFields()
            let targetField = typeDest.GetField(srcField.Name)
                where //srcField.CanRead
            targetField != null
            && targetField.FieldType.IsAssignableFrom(srcField.FieldType)
            && !targetField.IsStatic
            && !targetField.IsNotSerialized
            //&& (targetField.GetSetMethod(true) != null && !targetField.GetSetMethod(true).IsPrivate)
            //&& (targetField.GetSetMethod().Attributes & MethodAttributes.Static) == 0
            //&& targetField.PropertyType.IsAssignableFrom(srcField.PropertyType)
            select new { sourceField = srcField, targetField = targetField };
        //map the properties
        foreach (var fields in results)
        {
            fields.targetField.SetValue(destination, fields.sourceField.GetValue(source));
        }
    }
}