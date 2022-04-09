using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

namespace System.Application.Services.Implementation
{
    /// <inheritdoc cref="IModelValidator"/>
    public class ModelValidator : IModelValidator
    {
        static readonly MethodInfo matchValidateMethod = typeof(ModelValidator).GetMethod(nameof(MatchValidate), BindingFlags.NonPublic | BindingFlags.Static)!;
        static readonly IDictionary<Type, object> validators = new Dictionary<Type, object>();

        public static void AddColumnValidate<T>(Func<T, string?> validate)
            => validators.TryAdd(typeof(T), validate);

        const string I = "I";
        const string IReadOnly = "IReadOnly";
        const string IReadOnlyNullable = "IReadOnlyNullable";
        static readonly string[] StartsWithSubstrings = new[] { IReadOnlyNullable, IReadOnly, I };

        static string GetColumnName(Type type)
        {
            foreach (var item in StartsWithSubstrings)
            {
                if (type.Name.StartsWith(item))
                {
                    return type.Name[item.Length..];
                }
            }
            return type.Name;
        }

        static string? MatchValidate<T>(
            Func<T, string?> validate,
            object model,
            bool hasIgnores,
            Type[] ignores,
            Func<string[]> ignoreNames)
        {
            if (hasIgnores)
            {
                var type = typeof(T);
                if (ignores.Contains(type) || ignoreNames().Contains(GetColumnName(type)))
                {
                    return null;
                }
            }
            if (model is T t)
            {
                return validate(t);
            }
            return null;
        }

        public bool Validate(object model, [NotNullWhen(false)] out string? errorMessage, params Type[] ignores)
        {
            string[]? ignoreNames_ = null;
            Func<string[]> ignoreNames = GetIgnoreNames;
            string[] GetIgnoreNames()
            {
                if (ignoreNames_ == null)
                {
                    ignoreNames_ = ignores.Select(GetColumnName).ToArray();
                }
                return ignoreNames_;
            }
            var hasIgnores = ignores.Any_Nullable();
            if (!validators.Any())
            {
                // 验证组不能为空集合！
                throw new NotSupportedException("validators cannot be empty.");
            }
            foreach (var item in validators)
            {
                var parameters = new object?[] { item.Value, model, hasIgnores, ignores, ignoreNames };
                errorMessage = (string)matchValidateMethod.MakeGenericMethod(item.Key).Invoke(null, parameters);
                if (errorMessage != null) return false;
            }

            errorMessage = null;
            return true;
        }
    }
}