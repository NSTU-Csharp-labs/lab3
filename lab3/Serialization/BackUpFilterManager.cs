using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using lab3.Controls.MainWindow;

namespace lab3.Serialization;

public class BackUpFilterManager : IBackUpFilterManager
{
    private readonly string _pathToFile;
    private readonly XmlSerializer _serializer;

    public BackUpFilterManager(string pathToFile)
    {
        _pathToFile = pathToFile;
        _serializer = new XmlSerializer(typeof(FilterManager));
    }

    public async Task BackUp(FilterManager manager)
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


    public FilterManager LoadBackUp()
    {
        FilterManager? manager = null;

        var s = GenerateStreamFromString(File.ReadAllText(_pathToFile));
        try
        {
            manager = (FilterManager)_serializer.Deserialize(s)!;
        }
        catch (Exception e)
        {
            manager = new FilterManager();
        }
        finally
        {
            // переделать блин !!
            if (manager is null) manager = new FilterManager();
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