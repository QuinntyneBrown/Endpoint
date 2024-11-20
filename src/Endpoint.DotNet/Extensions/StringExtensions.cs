// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Collections.Generic;
using Endpoint.DotNet.Services;
using Endpoint.DotNet.Syntax.Properties;
using Endpoint.DotNet.Syntax.Types;
using Endpoint.DotNet.Syntax.Units;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Formatting;
using static System.Linq.Enumerable;

namespace System;

public static class StringExtensions
{
    private static INamingConventionConverter converter = new NamingConventionConverter();

    public static string FormatCSharp(this string csharpCode)
    {
        var tree = CSharpSyntaxTree.ParseText(csharpCode);

        var syntaxNode = tree.GetRoot();

        return Formatter.Format(syntaxNode, new AdhocWorkspace()).ToFullString();
    }

    public static List<string> FromCsv(this string commaSeparatedString)
    {
        if (string.IsNullOrEmpty(commaSeparatedString))
        {
            return [];
        }

        return commaSeparatedString.Split(',').ToList();
    }

    public static List<KeyValuePair<string,string>> ToKeyValuePairList(this string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return [];
        }

        return input.Split(',').Select(x =>
        {
            var parts = x.Split(":");

            var keyValuePair = new KeyValuePair<string, string>(parts[0], parts.Length > 1 ? parts[1] : string.Empty);

            return keyValuePair;
        })
            .ToList();
    }

    public static string ToPascalCase(this string value)
    {
        return converter.Convert(NamingConvention.PascalCase, value);
    }

    public static string ToCamelCase(this string value)
    {
        return converter.Convert(NamingConvention.CamelCase, value);
    }

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

    public static PropertyModel ToString(this string value, TypeDeclarationModel model, string? defaultValue = null)
    {
        return new PropertyModel(
            model,
            AccessModifier.Public,
            new TypeModel("string"),
            value,
            PropertyAccessorModel.GetSet)
        {
            DefaultValue = defaultValue,
        };
    }
}
