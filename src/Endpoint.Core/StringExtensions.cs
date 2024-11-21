// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Microsoft.CodeAnalysis;
using static System.Linq.Enumerable;

namespace System;

public static class StringExtensions
{

    public static string Indent(this string value, int indent, int spaces = 4)
    {
        try
        {
            if (value.Contains(Environment.NewLine))
            {
                string[] values = value.Split(Environment.NewLine);

                return string.Join(Environment.NewLine, values.Select(v => string.IsNullOrEmpty(v) ? v : $"{string.Join(string.Empty, Range(1, spaces * indent).Select(i => ' '))}{v}"));
            }
            else
            {
                return string.IsNullOrEmpty(value) ? value : $"{string.Join(string.Empty, Range(1, spaces * indent).Select(i => ' '))}{value}";
            }
        }
        catch (Exception e)
        {
            throw;
        }
    }

    public static string Remove(this string value, string item) => value.Replace(item, string.Empty);

    public static string? GetResourceName(this string[] collection, string name)
        => collection.SingleOrDefault(x => x.EndsWith(name)) == null ?
            collection.SingleOrDefault(x => x.EndsWith($".{name}.txt"))
            : collection.SingleOrDefault(x => x.EndsWith(name));

    public static Tuple<string, string> GetNameAndType(this string value)
    {
        var parts = value.Split(':');

        if (parts.Length == 1)
        {
            return new Tuple<string, string>(value, string.Empty);
        }

        return new Tuple<string, string>(parts[0], parts[1]);
    }

}
