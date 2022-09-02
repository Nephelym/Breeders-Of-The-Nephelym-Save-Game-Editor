using System.Xml.Linq;

using UnrealEngine.Gvas.FProperties;

namespace UnrealEngine.Gvas;

public class SaveGameFile
{
    public SaveGameHeader? Header { get; set; }
    public FStructProperty? Root { get; set; }
    private byte[]? restData;

    public static SaveGameFile LoadFrom(string path)
    {
        using var fileStream = File.OpenRead(path);
        using var reader = new DebugBinaryReader(fileStream);

        var saveGameFile = new SaveGameFile();
        saveGameFile.Header = SaveGameHeader.ReadFrom(reader);

        var root = new FStructProperty();
        FProperty property;
        while ((property = FProperty.ReadFrom(reader).First()) != FProperty.NoneProperty)
        {
            root.Fields.Add((property.Name!, property));
            if (root.Fields.Count == 7)
            {
                saveGameFile.restData = reader.ReadBytes((int)(reader.BaseStream.Length - reader.Position));
                break;
            }
        }
        saveGameFile.Root = root;

        
        return saveGameFile;
    }

    public void Save(string path)
    {
        using var fileStream = File.Open(path, FileMode.Create, FileAccess.ReadWrite, FileShare.Read);
        using var writer = new BinaryWriter(fileStream);

        try
        {
            Header!.WriteTo(writer);
            foreach (var field in Root!.Fields)
                field.Item2.WriteTo(writer);
            if (restData is not null)
                writer.Write(restData);
            else
            {
                writer.WriteFString("None");
                writer.Write(new byte[4]);

            }
        }
        catch (Exception ex)
        {
            Console.Out.WriteLine(ex);
        }
        finally
        {
            writer.Flush();
            writer.Close();
        }
    }

    public XElement Serialize()
    {
        var element = new XElement("SaveData");
        foreach (var property in Root!.Fields)
            element.Add(property.Item2.SerializeProperty());
        return element;
    }
}