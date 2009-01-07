// Guids.cs
// MUST match guids.h
using System;

namespace Company.YAMLLanguage
{
    static class GuidList
    {
        public const string guidYAMLLanguagePkgString = "43fab734-0c27-4a61-b19d-757e43a7721c";
        public const string guidYAMLLanguageCmdSetString = "5efd1df5-30e5-473e-8a46-27a91d7ca6b8";

        public static readonly Guid guidYAMLLanguageCmdSet = new Guid(guidYAMLLanguageCmdSetString);
    };
}