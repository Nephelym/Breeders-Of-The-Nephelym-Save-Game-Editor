using System.Xml.Linq;
namespace UnrealEngine.Gvas.FProperties;

[OptionalGuid]
public class FTextProperty : FProperty
{
    public string? Owner { get; set; }
    public string? Identifier { get; set; }
    public string? Value { get; set; }
    public int Unknown1 { get; set; }
    public byte Flags { get; set; }
    
    internal override void Read(BinaryReader reader, string? propertyName, long fieldLength, bool bodyOnly = false)
    {
        long startPos = reader.BaseStream.Position;
        Unknown1 = reader.ReadInt32();
        Flags = reader.ReadByte();
        if (fieldLength == 5)
        {
          if (Flags != 255)
          {
            throw new NotImplementedException();
          }
          return;
        }
        if(Flags == 2)
        {
            var val = reader.ReadBytes(6);
            return;
        }
        Owner = reader.ReadFString();
        if (Flags == 255)
        {
          // no extra fields
        }
        else if (Flags == 11)
        {
            Identifier = reader.ReadFString();
        }
        else if (Flags == 0)
        {
            Identifier = reader.ReadFString();
            Value = reader.ReadFString();
        }
        else if(Flags == 2)
        {

        }
        else
        {
          throw new NotImplementedException();
        }
    }

    protected override IEnumerable<object> SerializeContent()
    {
        yield return Value ?? string.Empty;
    }

    protected override void ModifyXmlNode(XElement element)
    {
        if (Owner is {Length: > 0})
            element.SetAttributeValue("Owner", Owner);
        if (Identifier is {Length: > 0})
            element.SetAttributeValue("Identifier", Identifier);
    }
}