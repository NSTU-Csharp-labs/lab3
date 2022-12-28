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
        _serializer = new XmlSerializer(typeof(FilterManagerState));
    }

    public async Task BackUp(FilterManagerState state)
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
        _serializer.Serialize(writer, state);
        writer.Close();
        fs.Close();
    }


    public FilterManagerState LoadBackUp()
    {
        FilterManagerState? state = null;

        var s = GenerateStreamFromString(File.ReadAllText(_pathToFile));
        try
        {
            state = (FilterManagerState)_serializer.Deserialize(s)!;
        }
        catch (Exception e)
        {
            state = new FilterManagerState();
        }
        finally
        {
            // переделать блин !!
            if (state is null) state = new FilterManagerState();
            s.Close();
        }

        return state;
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