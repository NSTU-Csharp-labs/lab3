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
    private string _pathToFile;
    private XmlSerializer _serializer;

    public BackUpSerializer(string pathToFile)
    {
        _pathToFile = pathToFile;
        _serializer = new XmlSerializer(typeof(MainWindowViewModel));
    }

    public async Task BackUp(MainWindowViewModel viewModel)
    {
        await File.WriteAllTextAsync(_pathToFile, "",CancellationToken.None);
        
        await using var fs = new FileStream(
            _pathToFile,
            FileMode.OpenOrCreate,
            FileAccess.Write,
            FileShare.Write, 
            4096,
            FileOptions.Asynchronous
        );
        
        
        var writer = XmlWriter.Create(fs, new XmlWriterSettings()
        {
            Indent = true,
            IndentChars = "    ",
        });
        _serializer.Serialize(writer, viewModel);
        writer.Close();
        fs.Close();
        // await WriteAsync(viewModel);

    }

    public Task<MainWindowViewModel?> DeserializeAsync()
    {
        using StringReader reader = new StringReader(
            File.ReadAllTextAsync(
                _pathToFile,
                CancellationToken.None
                ).Result
            );
        using XmlReader xmlReader = XmlReader.Create(reader);
        DataContractSerializer serializer =
            new DataContractSerializer(typeof(MainWindowViewModel));
        MainWindowViewModel theObject = (MainWindowViewModel)serializer.ReadObject(xmlReader);
        return Task.FromResult(theObject);
    }
    
    public Task<string> WriteAsync(MainWindowViewModel viewModel)
    {
        using var stringWriter = new StringWriter();
        using XmlWriter xmlWriter = XmlWriter.Create(stringWriter);
        DataContractSerializer serializer =
            new DataContractSerializer(typeof(MainWindowViewModel));
        serializer.WriteObject(xmlWriter, viewModel);
        return Task.FromResult(stringWriter.ToString());
    }
    


    public MainWindowViewModel LoadBackUp()
    {
        MainWindowViewModel viewModel;
        
        Stream s = GenerateStreamFromString(File.ReadAllText(_pathToFile));
        try
        {
            object? deserialized = (_serializer.Deserialize(s));
        
            if (deserialized is null)
            {
                viewModel = new MainWindowViewModel();
            }
            else
            {
                viewModel = (MainWindowViewModel)deserialized;
                viewModel.ImageManager.SetPicture();
            }
        }
        catch (Exception e)
        {
            viewModel = new MainWindowViewModel();
        }
        finally
        {
            s.Close();
        }
        
        return viewModel;
        // return DeserializeAsync().Result!;
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