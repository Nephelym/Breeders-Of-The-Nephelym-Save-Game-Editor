using System.Reflection.PortableExecutable;
using System.Xml.Linq;

namespace UnrealEngine.Gvas.FProperties;

public class FStructProperty : FProperty
{
    private Guid structureGuid = System.Guid.Empty;

    public string? TypeName { get; set; }
    public List<(string, FProperty)> Fields { get; } = new();

    public T GetField<T>(string name) where T : FProperty
    {
        return (T)Fields.First(x => x.Item1 == name).Item2;
    }

    internal override void Read(BinaryReader reader, string? propertyName, long fieldLength, bool bodyOnly = false)
    {
        if (!bodyOnly)
        {
            TypeName = reader.ReadFString();
            if (fieldLength == 0)
                return;
            structureGuid = new Guid(reader.ReadBytes(16));
            reader.ReadBytes(1);
        }

        if (TypeName == "GameplayTagContainer")
        {
            var prop = new FArrayProperty();
            var start = reader.BaseStream.Position + (!bodyOnly ? 17 : 0);
            var addedLength = fieldLength - (!bodyOnly ? 17 : 0);

            while (start + addedLength > reader.BaseStream.Position)
            {
                var count = reader.ReadInt32();
                prop.Elements.Add(new FIntProperty() { Value = count, Name = "Count" });
                for (int i = 0; i < count; i++)
                {
                    prop.Elements.Add(new FStrProperty() { Value = reader.ReadFString() });

                }
            }
            Fields.Add(("Tags", prop));
        }
        else if (TypeName == "Vector")
        {
            Fields.Add(("X", new FFloatProperty { Name = "X", Value = reader.ReadSingle() }));
            Fields.Add(("Y", new FFloatProperty { Name = "Y", Value = reader.ReadSingle() }));
            Fields.Add(("Z", new FFloatProperty { Name = "Z", Value = reader.ReadSingle() }));
        }
        else if (TypeName == "Rotator")
        {
            Fields.Add(("Pitch", new FFloatProperty { Name = "Pitch", Value = reader.ReadSingle() }));
            Fields.Add(("Yaw", new FFloatProperty { Name = "Yaw", Value = reader.ReadSingle() }));
            Fields.Add(("Roll", new FFloatProperty { Name = "Roll", Value = reader.ReadSingle() }));
        }
        else if (TypeName == "Quat")
        {
            Fields.Add(("X", new FFloatProperty { Name = "X", Value = reader.ReadSingle() }));
            Fields.Add(("Y", new FFloatProperty { Name = "Y", Value = reader.ReadSingle() }));
            Fields.Add(("Z", new FFloatProperty { Name = "Z", Value = reader.ReadSingle() }));
            Fields.Add(("W", new FFloatProperty { Name = "W", Value = reader.ReadSingle() }));
        }
        else if (TypeName == "DateTime")
            Fields.Add(("Ticks", new FInt64Property { Name = "Ticks", Value = reader.ReadInt64() }));
        else if (TypeName == "IntPoint")
        {
            Fields.Add(("X", new FIntProperty { Name = "X", Value = reader.ReadInt32() }));
            Fields.Add(("Y", new FIntProperty { Name = "Y", Value = reader.ReadInt32() }));
        }
        else if (TypeName == "Guid")
        {
            if (!bodyOnly)
            {
                Fields.Add(("A", new FIntProperty { Name = "A", Value = reader.ReadInt32() }));
                Fields.Add(("B", new FIntProperty { Name = "B", Value = reader.ReadInt32() }));
                Fields.Add(("C", new FIntProperty { Name = "C", Value = reader.ReadInt32() }));
                Fields.Add(("D", new FIntProperty { Name = "D", Value = reader.ReadInt32() }));
            }
        }
        else if (TypeName == "LinearColor")
        {
            Fields.Add(("RawData", new FByteProperty { Payload = reader.ReadBytes(16) }));

        }
        else
        {
            FProperty? field;
            while ((field = ReadFrom(reader).First()) != NoneProperty)
                Fields.Add((field.Name!, field));
        }
    }

    internal override void Write(BinaryWriter writer, bool skipHeader)
    {
        if (!skipHeader)
        {
            writer.WriteFString(TypeName);
            writer.Write(structureGuid.ToByteArray());
            writer.Write((byte)0);
        }

        if (TypeName == "GameplayTagContainer")
        {
            var props = (Fields.First(x => x.Item1 == "Tags").Item2 as FArrayProperty)!;
            foreach (var item in props.Elements)
            {
                if (item is FIntProperty fi)
                    writer.Write(fi.Value);
                else if (item is FStrProperty fs)
                {
                    writer.WriteFString(fs.Value);

                }
            }
        }
        else if (TypeName == "Vector")
        {
            writer.Write((Fields.First(x => x.Item1 == "X").Item2 as FFloatProperty)!.Value);
            writer.Write((Fields.First(x => x.Item1 == "Y").Item2 as FFloatProperty)!.Value);
            writer.Write((Fields.First(x => x.Item1 == "Z").Item2 as FFloatProperty)!.Value);
        }
        else if (TypeName == "Rotator")
        {
            writer.Write((Fields.First(x => x.Item1 == "Pitch").Item2 as FFloatProperty)!.Value);
            writer.Write((Fields.First(x => x.Item1 == "Yaw").Item2 as FFloatProperty)!.Value);
            writer.Write((Fields.First(x => x.Item1 == "Roll").Item2 as FFloatProperty)!.Value);
        }
        else if (TypeName == "Quat")
        {
            writer.Write((Fields.First(x => x.Item1 == "X").Item2 as FFloatProperty)!.Value);
            writer.Write((Fields.First(x => x.Item1 == "Y").Item2 as FFloatProperty)!.Value);
            writer.Write((Fields.First(x => x.Item1 == "Z").Item2 as FFloatProperty)!.Value);
            writer.Write((Fields.First(x => x.Item1 == "W").Item2 as FFloatProperty)!.Value);
        }
        else if (TypeName == "DateTime")
            writer.Write((Fields.First(x => x.Item1 == "Ticks").Item2 as FInt64Property)!.Value);
        else if (TypeName == "IntPoint")
        {
            writer.Write((Fields.First(x => x.Item1 == "X").Item2 as FIntProperty)!.Value);
            writer.Write((Fields.First(x => x.Item1 == "Y").Item2 as FIntProperty)!.Value);
        }
        else if (TypeName == "Guid")
        {
            writer.Write((Fields.First(x => x.Item1 == "A").Item2 as FIntProperty)!.Value);
            writer.Write((Fields.First(x => x.Item1 == "B").Item2 as FIntProperty)!.Value);
            writer.Write((Fields.First(x => x.Item1 == "C").Item2 as FIntProperty)!.Value);
            writer.Write((Fields.First(x => x.Item1 == "D").Item2 as FIntProperty)!.Value);
        }
        else if (TypeName == "LinearColor")
        {
            writer.Write((Fields.First(x => x.Item1 == "RawData").Item2 as FByteProperty)!.Payload!);
        }
        else
        {
            foreach (var (_, field) in Fields)
                field.WriteTo(writer);
            writer.WriteFString("None");
        }
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