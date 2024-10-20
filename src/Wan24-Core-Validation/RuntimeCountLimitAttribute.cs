﻿using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using wan24.ObjectValidation;

namespace wan24.Core
{
    /// <summary>
    /// Runtime count limitation attribute
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class RuntimeCountLimitAttribute : ValidationAttributeBase
    {
        /// <summary>
        /// Minimum
        /// </summary>
        protected readonly long? _Min = null;
        /// <summary>
        /// Minimum getter
        /// </summary>
        protected readonly PropertyInfoExt? _MinGetter = null;
        /// <summary>
        /// Maximum getter
        /// </summary>
        protected readonly PropertyInfoExt _MaxGetter = null!;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="maxGetter">Maximum getter static property</param>
        /// <param name="minGetter">Minimum getter static property</param>
        public RuntimeCountLimitAttribute(string maxGetter, string? minGetter = null) : this(maxGetter, minGetter, min: null) { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="maxGetter">Maximum getter static property</param>
        /// <param name="min">Minimum</param>
        public RuntimeCountLimitAttribute(string maxGetter, long min) : this(maxGetter, minGetter: null, min) { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="maxGetter">Maximum getter static property</param>
        /// <param name="minGetter">Minimum getter static property</param>
        /// <param name="min">Minimum</param>
        protected RuntimeCountLimitAttribute(in string maxGetter, in string? minGetter, in long? min) : base()
        {
            _Min = min;
            MinGetter = minGetter;
            MaxGetter = maxGetter;
            string[] info;
            if (minGetter is not null)
            {
                info = minGetter.Split('.');
                if (info.Length == 1) throw new ArgumentException("Invalid min. getter", nameof(minGetter));
                Type type = TypeHelper.Instance.GetType(string.Join('.', info.Take(info.Length - 1)))
                    ?? throw new ArgumentException($"Can't load type \"{string.Join('.', info.Take(info.Length - 1))}\"", nameof(minGetter));
                _MinGetter = type.GetPropertyCached(info[^1], BindingFlags.Public | BindingFlags.Static)
                    ?? throw new ArgumentException("Can't load public static property", nameof(minGetter));
            }
            {
                info = maxGetter.Split('.');
                if (info.Length == 1) throw new ArgumentException("Invalid max. getter", nameof(maxGetter));
                Type type = TypeHelper.Instance.GetType(string.Join('.', info.Take(info.Length - 1)))
                    ?? throw new ArgumentException($"Can't load type \"{string.Join('.', info.Take(info.Length - 1))}\"", nameof(maxGetter));
                _MaxGetter = type.GetPropertyCached(info[^1], BindingFlags.Public | BindingFlags.Static)
                    ?? throw new ArgumentException("Can't load public static property", nameof(maxGetter));
            }
        }

        /// <summary>
        /// Minimum
        /// </summary>
        public long? Min
        {
            get
            {
                if (_MinGetter is null) return _Min;
                object? value = _MinGetter.Getter is null ? null : _MinGetter.Getter(arg: null);
                if (value is null) return null;
                return (long)Convert.ChangeType(value, typeof(long));
            }
        }

        /// <summary>
        /// Maximum
        /// </summary>
        public long Max
            => (long)Convert.ChangeType((_MaxGetter.Getter is null ? null : _MaxGetter.Getter(arg: null)) ?? throw new InvalidProgramException("Maximum getter returned NULL"), typeof(long));

        /// <summary>
        /// Minimum getter static property
        /// </summary>
        public string? MinGetter { get; }

        /// <summary>
        /// Maximum getter static property
        /// </summary>
        public string MaxGetter { get; }

        /// <summary>
        /// Get the error message
        /// </summary>
        /// <param name="count">Count</param>
        /// <param name="member">Member</param>
        /// <returns>Error message</returns>
        public string? GetErrorMessage(long? count, string? member)
            => count is null || ((Min is null || count >= Min) && count <= Max)
                ? null
                : member is null
                        ? (Min is null
                            ? ErrorMessage ?? $"Maximum count is {Max} ({count})"
                            : ErrorMessage ?? $"Count must be between {Min} and {Max} ({count})")
                        : (Min is null
                            ? (ErrorMessage is null
                                ? $"Maximum count of the property {member} is {Max} ({count})"
                                : $"{member}: {ErrorMessage}")
                            : (ErrorMessage is null
                                ? $"Count of the property {member} must be between {Min} and {Max} ({count})"
                                : $"{member}: {ErrorMessage}"));

        /// <inheritdoc/>
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
            => GetErrorMessage(
                    value switch
                    {
                        IDictionary dict => dict.Count,
                        Array arr => arr.LongLength,
                        IList list => list.Count,
                        ICollection col => col.Count,
                        _ => null
                    },
                    validationContext.MemberName
                    ) is string error
                ? this.CreateValidationResult(error, validationContext)
                : null;
    }
}
