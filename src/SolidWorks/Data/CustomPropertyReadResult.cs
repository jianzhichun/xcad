//*********************************************************************
//xCAD solidworksz fork patch
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

namespace Xarial.XCad.SolidWorks.Data
{
    /// <summary>
    /// Outcome of a single non-cached custom-property read
    /// </summary>
    public class CustomPropertyReadResult
    {
        /// <summary>True when the manager reports the property is not present</summary>
        public bool NotPresent { get; }

        /// <summary>Raw stored text (expression or literal), as for ICustomPropertyManager raw output</summary>
        public string Raw { get; }

        /// <summary>Resolved/evaluated text as reported by SOLIDWORKS</summary>
        public string Resolved { get; }

        /// <summary>True when SOLIDWORKS resolved the expression (blank expressions resolve to true with empty text)</summary>
        public bool WasResolved { get; }

        /// <summary>True when the value is linked to another property (swCustomInfoGetResult link flag)</summary>
        public bool LinkToProperty { get; }

        public CustomPropertyReadResult(bool notPresent, string raw, string resolved,
            bool wasResolved, bool linkToProperty)
        {
            NotPresent = notPresent;
            Raw = raw;
            Resolved = resolved;
            WasResolved = wasResolved;
            LinkToProperty = linkToProperty;
        }
    }
}
