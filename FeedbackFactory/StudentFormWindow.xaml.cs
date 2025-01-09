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
using System.Windows.Shapes;

namespace FeedbackFactory
{

    public partial class StudentFormWindow : Window
    {
        private Dictionary<string, UserControl> _keyViewMapping;
        public StudentFormWindow(string inputKey)
        {
            InitializeComponent();


            _keyViewMapping = new Dictionary<string, UserControl>
            {
                { "Key1", new UnterrichtsBeurteilung() },
                { "Key2", new Zielscheibe() }
            };

            if (_keyViewMapping.TryGetValue(inputKey, out UserControl selectedView))
            {

                ViewContentControl.Content = selectedView;
            }
            else
            {
                MessageBox.Show("Ungültiger Schlüssel. Bitte versuchen Sie es erneut.");
            }




        }
    }
}

