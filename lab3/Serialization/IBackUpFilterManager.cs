using System.Threading.Tasks;
using lab3.Controls.MainWindow;

namespace lab3.Serialization;

public interface IBackUpFilterManager
{
    public Task BackUp(ImageManager manager);
    public ImageManager LoadBackUp();
}