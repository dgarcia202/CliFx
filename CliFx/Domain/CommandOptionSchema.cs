﻿using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;
using CliFx.Attributes;
using CliFx.Internal.Extensions;

namespace CliFx.Domain
{
    internal partial class CommandOptionSchema : CommandArgumentSchema
    {
        public string? Name { get; }

        public char? ShortName { get; }

        public string? EnvironmentVariableName { get; }

        public bool IsRequired { get; }

        public CommandOptionSchema(
            PropertyInfo? property,
            string? name,
            char? shortName,
            string? environmentVariableName,
            bool isRequired,
            string? description,
            Type? converterType,
            Type[] validatorTypes)
            : base(property, description, converterType, validatorTypes)
        {
            Name = name;
            ShortName = shortName;
            EnvironmentVariableName = environmentVariableName;
            IsRequired = isRequired;
        }

        public bool MatchesName(string? name) =>
            !string.IsNullOrWhiteSpace(Name) &&
            string.Equals(Name, name, StringComparison.OrdinalIgnoreCase);

        public bool MatchesShortName(char? shortName) =>
            ShortName != null &&
            ShortName == shortName;

        public bool MatchesNameOrShortName(string alias) =>
            MatchesName(alias) ||
            alias.Length == 1 && MatchesShortName(alias.Single());

        public bool MatchesEnvironmentVariableName(string environmentVariableName) =>
            !string.IsNullOrWhiteSpace(EnvironmentVariableName) &&
            string.Equals(EnvironmentVariableName, environmentVariableName, StringComparison.Ordinal);

        public string GetUserFacingDisplayString()
        {
            var buffer = new StringBuilder();

            if (!string.IsNullOrWhiteSpace(Name))
            {
                buffer
                    .Append("--")
                    .Append(Name);
            }

            if (!string.IsNullOrWhiteSpace(Name) && ShortName != null)
            {
                buffer.Append('|');
            }

            if (ShortName != null)
            {
                buffer
                    .Append('-')
                    .Append(ShortName);
            }

            return buffer.ToString();
        }

        public string GetInternalDisplayString() => $"{Property?.Name ?? "<implicit>"} ('{GetUserFacingDisplayString()}')";

        [ExcludeFromCodeCoverage]
        public override string ToString() => GetInternalDisplayString();
    }

    internal partial class CommandOptionSchema
    {
        public static CommandOptionSchema? TryResolve(PropertyInfo property)
        {
            var attribute = property.GetCustomAttribute<CommandOptionAttribute>();
            if (attribute == null)
                return null;

            // The user may mistakenly specify dashes, thinking it's required, so trim them
            var name = attribute.Name?.TrimStart('-');

            // If neither command option's name nor short name are provided generate a regular name based on property's name
            if (string.IsNullOrEmpty(name) && !attribute.ShortName.HasValue)
                name = property.Name.PascalToKebabCase();

            return new CommandOptionSchema(
                property,
                name,
                attribute.ShortName,
                attribute.EnvironmentVariableName,
                attribute.IsRequired,
                attribute.Description,
                attribute.Converter,
                attribute.Validators
            );
        }
    }

    internal partial class CommandOptionSchema
    {
        public static CommandOptionSchema HelpOption { get; } = new(
            null,
            "help",
            'h',
            null,
            false,
            "Shows help text.",
            null,
            Array.Empty<Type>()
        );

        public static CommandOptionSchema VersionOption { get; } = new(
            null,
            "version",
            null,
            null,
            false,
            "Shows version information.",
            null,
            Array.Empty<Type>()
        );
    }
}