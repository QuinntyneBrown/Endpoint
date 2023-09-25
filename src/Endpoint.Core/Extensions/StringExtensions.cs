// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Core.Services;
using Endpoint.Core.Syntax.Documents;
using static System.Linq.Enumerable;

namespace System;

public static class StringExtensions
{
    public static string Indent(this string value, int indent, int spaces = 4)
    {
        try
        {
            string[] values = value.Split(Environment.NewLine);

            var result = string.Join(Environment.NewLine, values.Select(v => string.IsNullOrEmpty(v) ? v : $"{string.Join(string.Empty, Range(1, spaces * indent).Select(i => ' '))}{v}"));

            return result;
        }
        catch (Exception e)
        {
            throw;
        }
    }

    public static string Remove(this string value, string item) => value.Replace(item, string.Empty);

    public static string GetResourceName(this string[] collection, string name)
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

    public static RequestType ToRequestType(this string value)
    {
        return value.ToLower() switch
        {
            "get" => RequestType.Get,
            "getbyId" => RequestType.GetById,
            "page" => RequestType.Page,
            "create" => RequestType.Create,
            "delete" => RequestType.Delete,
            "update" => RequestType.Update,
            _ => throw new InvalidOperationException()
        };
    }

    public static string ToRequestName(this string value, INamingConventionConverter namingConventionConverter)
    {
        return value.ToLower() switch
        {
            "get" => $"Get{namingConventionConverter.Convert(NamingConvention.PascalCase, value, pluralize: true)}",
            _ => throw new InvalidOperationException()
        };
    }
}
