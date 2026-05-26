using System;

[AttributeUsage(AttributeTargets.Field)]
public class AutoBindAttribute : Attribute
{
    public string customName;

    public AutoBindAttribute()
    {
    }

    public AutoBindAttribute(string customName)
    {
        this.customName = customName;
    }
}
