using System.Windows;
using System.Data.SqlServerCe;

namespace Support {
    /// <summary>
    /// Interaction logic for MakeCollection.xaml
    /// </summary>
    public partial class MakeCollection : Window {

        public MakeCollection() {
            InitializeComponent();
        }

        private void CancelButtonClick(object sender, RoutedEventArgs e) {
            Close();
        }

        private void MakeCollectionButtonClick(object sender, RoutedEventArgs e) {
            Collection collection = new Collection();
            collection.MakeCollection();
        }
    }
}
