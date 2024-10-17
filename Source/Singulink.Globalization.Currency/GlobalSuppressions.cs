// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

[assembly: SuppressMessage("Naming", "CA1710:Identifiers should have correct suffix", Justification = "Collections have correct suffixes")]
[assembly: SuppressMessage("Naming", "CA1725:Parameter names should match base declaration", Justification = "Parameter names have been standardized across codebase")]
[assembly: SuppressMessage("Maintainability", "CA1510: Use ArgumentNullException throw helper", Justification = "Throw helpers not available in .NET Standard")]