using System.Reflection;
using System.ComponentModel;
using System;

class GameCode : Attribute
{
	internal GameCode ( int value )
	{
		this.Value = value ;
	}
	public int Value { get; private set; }
}

public static class EnumExtenstions  {

    public static string GetDescription(this Enum value)
    {
        FieldInfo fi = value.GetType().GetField(value.ToString());
       	DescriptionAttribute[] attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);
		return attributes.Length > 0 ? attributes [0].Description : value.ToString ();
    }

	public static int GetGameCode(this Enum value)
	{
		FieldInfo fi = value.GetType().GetField(value.ToString());
		GameCode[] attributes = (GameCode[])fi.GetCustomAttributes(typeof(GameCode), false);
		return attributes.Length > 0 ? attributes[0].Value : -1 ;
	}

    public static T GetValueFromDescription<T>(this string description)
    {
        var type = typeof(T);
        if (!type.IsEnum) throw new InvalidOperationException();
        foreach (var field in type.GetFields())
        {
            var attribute = Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) as DescriptionAttribute;
            if (attribute != null)
            {
                if (attribute.Description == description)
                {
                    return (T)field.GetValue(null);
                }
            }
            else
            {
                if (field.Name == description)
                {
                    return (T)field.GetValue(null);
                }
            }
        }
        throw new ArgumentException("Enum description not found.", description);
    }
}
