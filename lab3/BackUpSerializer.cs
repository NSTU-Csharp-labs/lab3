using System;
using System.IO;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using lab3.Controls.MainWindow;

namespace lab3;

public class BackUpSerializer : IBackUpSerializer
{
    private readonly string _pathToFile;
    private readonly XmlSerializer _serializer;

    public BackUpSerializer(string pathToFile)
    {
        _pathToFile = pathToFile;
        _serializer = new XmlSerializer(typeof(MainWindowViewModel));
    }

    public async Task BackUp(MainWindowViewModel viewModel)
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
        _serializer.Serialize(writer, viewModel);
        writer.Close();
        fs.Close();
    }


    public MainWindowViewModel LoadBackUp()
    {
        MainWindowViewModel? viewModel = null;

        var s = GenerateStreamFromString(File.ReadAllText(_pathToFile));
        try
        {
            viewModel = (MainWindowViewModel)_serializer.Deserialize(s)!;
        }
        catch (Exception e)
        {
            viewModel = new MainWindowViewModel();
        }
        finally
        {
            if (viewModel is null) viewModel = new MainWindowViewModel();
            else viewModel.PictureManager.SetPicture();
            s.Close();
        }

        return viewModel;
    }

    public Task<string> WriteAsync(MainWindowViewModel viewModel)
    {
        using var stringWriter = new StringWriter();
        using var xmlWriter = XmlWriter.Create(stringWriter);
        var serializer =
            new DataContractSerializer(typeof(MainWindowViewModel));
        serializer.WriteObject(xmlWriter, viewModel);
        return Task.FromResult(stringWriter.ToString());
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