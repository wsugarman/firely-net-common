﻿/*
  Copyright (c) 2011+, HL7, Inc.
  All rights reserved.
  
  Redistribution and use in source and binary forms, with or without modification, 
  are permitted provided that the following conditions are met:
  
   * Redistributions of source code must retain the above copyright notice, this 
     list of conditions and the following disclaimer.
   * Redistributions in binary form must reproduce the above copyright notice, 
     this list of conditions and the following disclaimer in the documentation 
     and/or other materials provided with the distribution.
   * Neither the name of HL7 nor the names of its contributors may be used to 
     endorse or promote products derived from this software without specific 
     prior written permission.
  
  THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND 
  ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED 
  WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. 
  IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, 
  INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT 
  NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR 
  PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, 
  WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) 
  ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE 
  POSSIBILITY OF SUCH DAMAGE.
  

*/

using Hl7.Fhir.Introspection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Text;


namespace Hl7.Fhir.Model
{
    /// <summary>
    /// A reference to a code defined by a terminology system
    /// </summary>
    [FhirType("Coding")]
    [DataContract]
    [DebuggerDisplay(@"\{{DebuggerDisplay,nq}}")]
    public class Coding : DataType
    {
        public override string TypeName { get { return "Coding"; } }

        public Coding()
        {
        }

        public Coding(string system, string code)
        {
            this.System = system;
            this.Code = code;
        }

        public Coding(string system, string code, string display)
        {
            this.System = system;
            this.Code = code;
            this.Display = display;
        }

        /// <summary>
        /// Identity of the terminology system
        /// </summary>
        [FhirElement("system", InSummary = true, Order = 30)]
        [DataMember]
        public Hl7.Fhir.Model.FhirUri SystemElement
        {
            get { return _SystemElement; }
            set { _SystemElement = value; OnPropertyChanged("SystemElement"); }
        }

        private Hl7.Fhir.Model.FhirUri _SystemElement;

        /// <summary>
        /// Identity of the terminology system
        /// </summary>
        /// <remarks>This uses the native .NET datatype, rather than the FHIR equivalent</remarks>
        [IgnoreDataMemberAttribute]
        public string System
        {
            get => SystemElement?.Value;
            set
            {
                SystemElement = value == null ? null : new Hl7.Fhir.Model.FhirUri(value);
                OnPropertyChanged("System");
            }
        }

        /// <summary>
        /// Version of the system - if relevant
        /// </summary>
        [FhirElement("version", InSummary = true, Order = 40)]
        [DataMember]
        public Hl7.Fhir.Model.FhirString VersionElement
        {
            get { return _VersionElement; }
            set { _VersionElement = value; OnPropertyChanged("VersionElement"); }
        }

        private Hl7.Fhir.Model.FhirString _VersionElement;

        /// <summary>
        /// Version of the system - if relevant
        /// </summary>
        /// <remarks>This uses the native .NET datatype, rather than the FHIR equivalent</remarks>
        [IgnoreDataMemberAttribute]
        public string Version
        {
            get { return VersionElement?.Value; }
            set
            {
                VersionElement = value == null ? null : new FhirString(value);
                OnPropertyChanged("Version");
            }
        }

        /// <summary>
        /// Symbol in syntax defined by the system
        /// </summary>
        [FhirElement("code", InSummary = true, Order = 50)]
        [DataMember]
        public Hl7.Fhir.Model.Code CodeElement
        {
            get { return _CodeElement; }
            set { _CodeElement = value; OnPropertyChanged("CodeElement"); }
        }

        private Hl7.Fhir.Model.Code _CodeElement;

        /// <summary>
        /// Symbol in syntax defined by the system
        /// </summary>
        /// <remarks>This uses the native .NET datatype, rather than the FHIR equivalent</remarks>
        [IgnoreDataMemberAttribute]
        public string Code
        {
            get { return CodeElement?.Value; }
            set
            {
                CodeElement = value == null ? null : new Hl7.Fhir.Model.Code(value);
                OnPropertyChanged("Code");
            }
        }

        /// <summary>
        /// Representation defined by the system
        /// </summary>
        [FhirElement("display", InSummary = true, Order = 60)]
        [DataMember]
        public Hl7.Fhir.Model.FhirString DisplayElement
        {
            get { return _DisplayElement; }
            set { _DisplayElement = value; OnPropertyChanged("DisplayElement"); }
        }

        private Hl7.Fhir.Model.FhirString _DisplayElement;

        /// <summary>
        /// Representation defined by the system
        /// </summary>
        /// <remarks>This uses the native .NET datatype, rather than the FHIR equivalent</remarks>
        [IgnoreDataMember]
        public string Display
        {
            get { return DisplayElement?.Value; }
            set
            {
                DisplayElement = value == null ? null : new Hl7.Fhir.Model.FhirString(value);
                OnPropertyChanged("Display");
            }
        }

        /// <summary>
        /// If this coding was chosen directly by the user
        /// </summary>
        [FhirElement("userSelected", InSummary = true, Order = 70)]
        [DataMember]
        public Hl7.Fhir.Model.FhirBoolean UserSelectedElement
        {
            get { return _UserSelectedElement; }
            set { _UserSelectedElement = value; OnPropertyChanged("UserSelectedElement"); }
        }

        private Hl7.Fhir.Model.FhirBoolean _UserSelectedElement;

        /// <summary>
        /// If this coding was chosen directly by the user
        /// </summary>
        /// <remarks>This uses the native .NET datatype, rather than the FHIR equivalent</remarks>
        [IgnoreDataMember]
        public bool? UserSelected
        {
            get { return UserSelectedElement?.Value; }
            set
            {
                UserSelectedElement = !value.HasValue ? null : new Hl7.Fhir.Model.FhirBoolean(value);
                OnPropertyChanged("UserSelected");
            }
        }

        public override IDeepCopyable CopyTo(IDeepCopyable other)
        {
            var dest = other as Coding;

            if (dest != null)
            {
                base.CopyTo(dest);
                if (SystemElement != null) dest.SystemElement = (Hl7.Fhir.Model.FhirUri)SystemElement.DeepCopy();
                if (VersionElement != null) dest.VersionElement = (Hl7.Fhir.Model.FhirString)VersionElement.DeepCopy();
                if (CodeElement != null) dest.CodeElement = (Hl7.Fhir.Model.Code)CodeElement.DeepCopy();
                if (DisplayElement != null) dest.DisplayElement = (Hl7.Fhir.Model.FhirString)DisplayElement.DeepCopy();
                if (UserSelectedElement != null) dest.UserSelectedElement = (Hl7.Fhir.Model.FhirBoolean)UserSelectedElement.DeepCopy();
                return dest;
            }
            else
                throw new ArgumentException("Can only copy to an object of the same type", "other");
        }

        public override IDeepCopyable DeepCopy()
        {
            return CopyTo(new Coding());
        }

        public override bool Matches(IDeepComparable other)
        {
            var otherT = other as Coding;
            if (otherT == null) return false;

            if (!base.Matches(otherT)) return false;
            if (!DeepComparable.Matches(SystemElement, otherT.SystemElement)) return false;
            if (!DeepComparable.Matches(VersionElement, otherT.VersionElement)) return false;
            if (!DeepComparable.Matches(CodeElement, otherT.CodeElement)) return false;
            if (!DeepComparable.Matches(DisplayElement, otherT.DisplayElement)) return false;
            if (!DeepComparable.Matches(UserSelectedElement, otherT.UserSelectedElement)) return false;

            return true;
        }

        public override bool IsExactly(IDeepComparable other)
        {
            var otherT = other as Coding;
            if (otherT == null) return false;

            if (!base.IsExactly(otherT)) return false;
            if (!DeepComparable.IsExactly(SystemElement, otherT.SystemElement)) return false;
            if (!DeepComparable.IsExactly(VersionElement, otherT.VersionElement)) return false;
            if (!DeepComparable.IsExactly(CodeElement, otherT.CodeElement)) return false;
            if (!DeepComparable.IsExactly(DisplayElement, otherT.DisplayElement)) return false;
            if (!DeepComparable.IsExactly(UserSelectedElement, otherT.UserSelectedElement)) return false;

            return true;
        }

        public override IEnumerable<Base> Children
        {
            get
            {
                foreach (var item in base.Children) yield return item;
                if (SystemElement != null) yield return SystemElement;
                if (VersionElement != null) yield return VersionElement;
                if (CodeElement != null) yield return CodeElement;
                if (DisplayElement != null) yield return DisplayElement;
                if (UserSelectedElement != null) yield return UserSelectedElement;
            }
        }

        public override IEnumerable<ElementValue> NamedChildren
        {
            get
            {
                foreach (var item in base.NamedChildren) yield return item;
                if (SystemElement != null) yield return new ElementValue("system", SystemElement);
                if (VersionElement != null) yield return new ElementValue("version", VersionElement);
                if (CodeElement != null) yield return new ElementValue("code", CodeElement);
                if (DisplayElement != null) yield return new ElementValue("display", DisplayElement);
                if (UserSelectedElement != null) yield return new ElementValue("userSelected", UserSelectedElement);
            }
        }


        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public string DebuggerDisplay
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                if (!string.IsNullOrEmpty(this.Code))
                    sb.AppendFormat(" Code=\"{0}\"", Code);
                if (!string.IsNullOrEmpty(this.Display))
                    sb.AppendFormat(" Display=\"{0}\"", Display);
                if (!string.IsNullOrEmpty(this.System))
                    sb.AppendFormat(" System=\"{0}\"", System);

                return sb.ToString();
            }
        }
    }
}