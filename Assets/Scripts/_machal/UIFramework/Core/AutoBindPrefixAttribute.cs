using System;

[AttributeUsage(AttributeTargets.Class)]
public class AutoBindPrefixAttribute : Attribute
{
    public string prefix;

    public AutoBindPrefixAttribute(string prefix)
    {
        this.prefix = prefix;
    }
}
