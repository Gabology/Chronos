using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ChronosCore;
using System.Text.RegularExpressions;

namespace Chronos {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        private Dictionary<string, string> _timeMap = new Dictionary<string, string> { 
            { "Vanlig", "Regular" },
            { "Övertid", "Overtime" },
            { "Sjukdom", "Sickness" },
            { "Semester", "Vacation" },
        };

        public MainWindow() {
            InitializeComponent();
        }

        private void saveEntryBtn_Click(object sender, RoutedEventArgs e) {
            var name = Environment.MachineName;
            var date = datePicker.SelectedDate.Value;
            var duration = Tuple.Create(Int32.Parse(hoursTb.Text), Int32.Parse(minutesTb.Text));
            var billType = _timeMap[(string)billAsLb.SelectionBoxItem];
            var description = descriptionTb.Text;
            Parser.Parse(name, date, duration, billType, description);
            MessageBox.Show("Tidsrapport skapad!");
            Close();
        }

        private void durationTb_PreviewTextInput(object sender, TextCompositionEventArgs e) {
            if (!char.IsDigit(e.Text, e.Text.Length - 1))
                e.Handled = true;
        }
    }
}
