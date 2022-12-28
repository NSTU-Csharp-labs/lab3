using System.Threading.Tasks;
using lab3.Controls.MainWindow;

namespace lab3.Serialization;

public interface IBackUpFilterManager
{
    public Task BackUp(FilterManager manager);
    public FilterManager LoadBackUp();
}