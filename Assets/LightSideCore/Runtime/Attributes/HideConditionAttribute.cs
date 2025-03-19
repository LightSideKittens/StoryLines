﻿namespace LSCore.Attributes
{
    using System;

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class HideConditionAttribute : Attribute
    {
    }

}