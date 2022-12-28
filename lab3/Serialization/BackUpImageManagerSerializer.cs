using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using lab3.Controls.MainWindow;

namespace lab3.Serialization;

public class BackUpImageManagerSerializer : IImageManagerSerializer
{
    private readonly string _pathToFile;
    private readonly XmlSerializer _serializer;

    public BackUpImageManagerSerializer(string pathToFile)
    {
        _pathToFile = pathToFile;
        _serializer = new XmlSerializer(typeof(ImageManager));
    }

    public async Task BackUp(ImageManager manager)
    {
        await File.WriteAllTextAsync(_pathToFile, "", CancellationToken.None);

        await using var fs = new FileStream(
            _pathToFile,
            FileMode.OpenOrCreate,
            FileAccess.Write,
            FileShare.Write,
            4096,
            FileOptions.Asynchronous
        );


        var writer = XmlWriter.Create(fs, new XmlWriterSettings
        {
            Indent = true,
            IndentChars = "    "
        });
        _serializer.Serialize(writer, manager);
        writer.Close();
        fs.Close();
    }


    public ImageManager LoadBackUp()
    {
        ImageManager? manager = null;

        var s = GenerateStreamFromString(File.ReadAllText(_pathToFile));
        try
        {
            manager = (ImageManager)_serializer.Deserialize(s)!;
        }
        catch (Exception e)
        {
            manager = new ImageManager();
        }
        finally
        {
            if (manager is null) manager = new ImageManager();
            else manager.SetPicture();
            s.Close();
        }

        return manager;
    }

    private static Stream GenerateStreamFromString(string s)
    {
        var stream = new MemoryStream();
        var writer = new StreamWriter(stream);
        writer.Write(s);
        writer.Flush();
        stream.Position = 0;
        return stream;
    }
}