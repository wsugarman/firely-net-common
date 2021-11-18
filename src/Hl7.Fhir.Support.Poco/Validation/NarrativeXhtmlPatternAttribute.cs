﻿/* 
 * Copyright (c) 2014, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/FirelyTeam/firely-net-sdk/master/LICENSE
 */

using Hl7.Fhir.Model;
using System;
using System.ComponentModel.DataAnnotations;

#nullable enable

namespace Hl7.Fhir.Validation
{
    /// <summary>
    /// Validates an xhtml value against the FHIR rules for xhtml.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public class NarrativeXhtmlPatternAttribute : ValidationAttribute
    {
        /// <inheritdoc />
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext) =>
            value switch
            {
                null => ValidationResult.Success,
                string s when XHtml.IsValidValue(s) => ValidationResult.Success,
                string _ => DotNetAttributeValidation.BuildResult(validationContext, "Xml can not be parsed or is not valid according to the (limited) FHIR scheme."),
                _ => throw new ArgumentException("CodePatternAttribute can only be applied to string properties.")
            };
    }
}

#nullable restore