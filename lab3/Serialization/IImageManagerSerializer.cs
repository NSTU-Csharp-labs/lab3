using System.Threading.Tasks;
using lab3.Controls.MainWindow;

namespace lab3.Serialization;

public interface IImageManagerSerializer
{
    public Task BackUp(ImageManagerState state);
    public ImageManagerState LoadBackUp();
}