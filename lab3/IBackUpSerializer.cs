using System.Threading.Tasks;

namespace lab3.Controls.MainWindow;

public interface IBackUpSerializer
{
    public Task BackUp(MainWindowViewModel viewModel);
    public MainWindowViewModel LoadBackUp();
}