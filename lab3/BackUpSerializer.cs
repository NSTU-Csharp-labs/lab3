using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using lab3.Controls.MainWindow;

namespace lab3;

public class BackUpSerializer
{
    
    private string _pathToFile;
    private XmlSerializer _serializer;
    public BackUpSerializer(string pathToFile)
    {
        _pathToFile = pathToFile;
        _serializer = new XmlSerializer(typeof(MainWindowViewModel));
    }

    public void BackUp(MainWindowViewModel viewModel)
    {
        using var fs = File.Create(_pathToFile);
        
        var writer = XmlWriter.Create(fs, new XmlWriterSettings(){
            Indent = true, 
            IndentChars = "    ",
        });
        _serializer.Serialize(writer, viewModel);
        writer.Close();
        fs.Close();
    }

    public MainWindowViewModel LoadBackUp()
    {
        MainWindowViewModel viewModel;

        Stream s = GenerateStreamFromString(File.ReadAllText(_pathToFile));
        // using var fs 
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
                viewModel.SetPicture();
            }
        }
        catch (Exception e)
        {
            s.Close();
            viewModel = new MainWindowViewModel();
        }
        finally
        {
            s.Close();
        }
        
        return viewModel;
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