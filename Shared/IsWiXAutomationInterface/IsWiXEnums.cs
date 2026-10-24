namespace IsWiXAutomationInterface
{
    // Attribute values shared by the WiX v4+ automation classes and the designers.
    // They used to live in the WiX v3 classes (IsWiXFeatures, IsWiXPackage), which were removed.

    public enum AllowAdvertise { no, system, yes }
    public enum InstallDefault { followParent, local, source }
    public enum TypicalDefault { advertise, install }
    public enum Display { collapse, expand, hidden }

    public enum YesNo
    {
        yes,
        no
    } ;
}