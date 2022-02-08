// Guids.cs
// MUST match guids.h
using System;

namespace ISWIXLLC.IsWiXNewNewAddIn
{
    static class GuidList
    {
        public const string guidIsWiXNewNewAddInPkgString = "52b70414-66d7-4951-88e0-7f023e0fdce2";
        public const string guidIsWiXNewNewAddInCmdSetString = "15290db8-dcf3-4198-8526-5f7243c2b05a";

        public static readonly Guid guidIsWiXNewNewAddInCmdSet = new Guid(guidIsWiXNewNewAddInCmdSetString);
    };
}