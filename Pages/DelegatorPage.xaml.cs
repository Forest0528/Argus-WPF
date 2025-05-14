using System.Windows.Controls;

namespace Argus_WPF.Pages
{
    public partial class DelegatorPage : Page
    {
        public DelegatorPage()
        {
            InitializeComponent();
            DataContext = new ViewModels.DelegatorViewModel();
        }
    }
}
