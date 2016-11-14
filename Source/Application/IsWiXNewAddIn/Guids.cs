// Guids.cs
// MUST match guids.h
using System;

namespace ISWIXLLC.IsWiXNewAddIn
{
    static class GuidList
    {
        public const string guidIsWiXNewAddInPkgString = "52b70414-66d7-4951-88e0-7f023e0fdce2";
        public const string guidIsWiXNewAddInCmdSetString = "15290db8-dcf3-4198-8526-5f7243c2b05a";

        public static readonly Guid guidIsWiXNewAddInCmdSet = new Guid(guidIsWiXNewAddInCmdSetString);
    };
}