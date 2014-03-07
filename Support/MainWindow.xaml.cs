using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Support {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {


        public MainWindow() {
            InitializeComponent();

            DataGridTextColumn textColumn0 = new DataGridTextColumn {Header = "id", Visibility = Visibility.Hidden, Binding = new Binding("Id")};
            DataGridTextColumn textColumn1 = new DataGridTextColumn { Header = "Номер", Binding = new Binding("Number") };
            DataGridTextColumn textColumn2 = new DataGridTextColumn { Header = "Дата", Binding = new Binding("Date") };
            DataGridTextColumn textColumn3 = new DataGridTextColumn { Header = "Инкассатор", Binding = new Binding("Collector") };
            DataGridTextColumn textColumn4 = new DataGridTextColumn { Header = "Статус", Binding = new Binding("State") };
            DataGridTextColumn textColumn5 = new DataGridTextColumn { Header = "Сумма", Binding = new Binding("Sum") };
            DataGridTextColumn textColumn6 = new DataGridTextColumn { Header = "Кол-во купюр", Binding = new Binding("Notes") };

            textColumn0.Width = 30;
            textColumn1.Width = 150;
            textColumn2.Width = 150;
            textColumn3.Width = 100;
            textColumn4.Width = 100;
            textColumn5.Width = 100;
            textColumn6.Width = 100;

            encashmentDataGrid.Columns.Add(textColumn0);
            encashmentDataGrid.Columns.Add(textColumn1);
            encashmentDataGrid.Columns.Add(textColumn2);
            encashmentDataGrid.Columns.Add(textColumn3);
            encashmentDataGrid.Columns.Add(textColumn4);
            encashmentDataGrid.Columns.Add(textColumn5);
            encashmentDataGrid.Columns.Add(textColumn6);

            IEnumerable<Collection> colections = Collection.GetCollections();

            foreach (var collection in colections) {
                encashmentDataGrid.Items.Add(collection);
            }

        }

        private void EncashmentDataGridSelectionChanged(object sender, SelectionChangedEventArgs e) {

            checkPreview.Content = string.Empty;

            Collection collection = (Collection)encashmentDataGrid.SelectedItems[0];

            foreach (var str in Collection.get_stat(collection)) {
                checkPreview.Content += str + "\r\n";
            }
        }



        private void ExitButtonClick(object sender, RoutedEventArgs e) {
            Application.Current.Shutdown();
        }

        private void PrintCheckButtonClick(object sender, RoutedEventArgs e) {

        }

        private void MakeCollectionButtonClick(object sender, RoutedEventArgs e) {
            MakeCollection makeCollection = new MakeCollection();
            makeCollection.ShowDialog();
        }
    }
}
