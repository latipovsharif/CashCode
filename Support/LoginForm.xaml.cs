using System.Windows;
using System.Windows.Controls;

namespace Support {
    /// <summary>
    /// Interaction logic for LoginForm.xaml
    /// </summary>
    public partial class LoginForm : Window {

        private bool _isLoginActive = true;
        public static int CollectorId = 0;

        public LoginForm() {
            InitializeComponent();
        }


        private void BtnNextClick(object sender, RoutedEventArgs e) {

            CollectorId = Collection.FindCollector(login.Text, password.Password);

            if (CollectorId > 0) {
                MainWindow main = new MainWindow ();
                main.ShowDialog();
            } else {
                MessageBox.Show("Неправильный пароль или логин");
            }
        }


        private void BtnBackClick(object sender, RoutedEventArgs e) {
           Application.Current.Shutdown();
        }


        private void BtnClick(object sender, RoutedEventArgs e) {
            Button btn = (Button) sender;
            if (!btn.Content.Equals("<-")) {
                if (_isLoginActive) {
                    login.Text += btn.Content;
                } else {
                    password.Password += btn.Content;
                }
            } else {
                if (_isLoginActive) {
                    if (login.Text.Length > 0) {
                        login.Text = login.Text.Substring(0, login.Text.Length - 1);
                    }
                } else {
                    if (password.Password.Length > 0) {
                        password.Password = password.Password.Substring(0, password.Password.Length - 1);
                    }
                }
            }
        }


        private void PasswordBox1GotFocus(object sender, RoutedEventArgs e) {
            _isLoginActive = false;
        }


        private void TextBox1GotFocus(object sender, RoutedEventArgs e) {
            _isLoginActive = true;
        }
    }
}
