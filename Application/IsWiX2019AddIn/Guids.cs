// Guids.cs
// MUST match guids.h
using System;

namespace ISWIXLLC.IsWiX2019AddIn
{
    static class GuidList
    {
        public const string guidIsWiX2019AddInPkgString = "52b70414-66d7-4951-88e0-7f023e0fdce2";
        public const string guidIsWiX2019AddInCmdSetString = "15290db8-dcf3-4198-8526-5f7243c2b05a";

        public static readonly Guid guidIsWiX2019AddInCmdSet = new Guid(guidIsWiX2019AddInCmdSetString);
    };
}