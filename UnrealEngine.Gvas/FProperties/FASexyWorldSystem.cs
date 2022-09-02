using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace UnrealEngine.Gvas.FProperties;
internal class FASexyWorldSystem : FProperty
{
    private Guid structureGuid = System.Guid.Empty;

    public string? TypeName { get; set; }
    public List<(string, FProperty)> Fields { get; } = new();

    internal override void Read(BinaryReader reader, string? propertyName, long fieldLength, bool bodyOnly = false)
    {
        var req = reader.ReadFString();
        if(req == "TraitRequirementSttat")
        {
            var first = reader.ReadFString();

        }
        FProperty? field;
        while ((field = ReadFrom(reader).First()) != NoneProperty)
            Fields.Add((field.Name!, field));
    }

    internal override void Write(BinaryWriter writer, bool skipHeader)
    {

        foreach (var (_, field) in Fields)
            field.WriteTo(writer);
        writer.WriteFString("None");
    }

    protected override IEnumerable<object> SerializeContent()
    {
        foreach (var (_, field) in Fields)
            yield return field.SerializeProperty();
    }

    protected override void ModifyXmlNode(XElement element)
    {
        element.SetAttributeValue("Type", TypeName);
    }


    public override string ToString() => $"{Name}: {TypeName}";
}
