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

using ASPASC.BusinessLayer;
using ASPASC.Model;

namespace ASPASC.UI.Dialogs
{
    /// <summary>
    /// Interaction logic for Application_Add.xaml
    /// </summary>
    public partial class Application_Add : Window
    {
        #region "Properties"

        private BL _BL
        {
            get
            {
                return new BL("/");
            }
        }

        #endregion

        public Application_Add()
        {
            InitializeComponent();
        }

        #region "Button Events"

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (!checkRequiredFields())
            {
                MessageBox.Show("Please enter the required fields highlighed in Red.", "Error", MessageBoxButton.OK);
                return;
            }

            List<ASPASCApplication> apps = _BL.getApplications();

            if (apps.Where(p => p.Name.ToLower() == tbApplicationName.Text.ToLower()).Count() == 0)
            {
                if (_BL.application_add(tbApplicationName.Text))
                {
                    MessageBox.Show("Successfully added application.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    this.DialogResult = true;
                }
                else
                    MessageBox.Show("Could not add application.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                MessageBox.Show("An application with this name is already registered.", "Application Exists", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion

        #region "Helpers"

        private Boolean checkRequiredFields()
        {
            Boolean everythingIsOk = true;

            tbApplicationName.Background = Brushes.White;

            if (tbApplicationName.Text.Length == 0)
            {
                everythingIsOk = false;
                tbApplicationName.Background = Brushes.Red;
            }

            return everythingIsOk;
        }

        #endregion
    }
}
