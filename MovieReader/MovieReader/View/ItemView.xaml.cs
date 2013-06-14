using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using MovieReader;

namespace MovieReader.View
{
    public partial class ItemView : UserControl
    {
        public ItemView()
        {
            InitializeComponent();
        }

        private void ItemListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ItemListBox.SelectedIndex != 0 && ItemListBox.SelectedIndex != -1)
            {
                string [] Ids = (PhoneApplicationService.Current.State["Ids"].ToString()).Split(' ');
                PhoneApplicationService.Current.State["id"] = Ids[ItemListBox.SelectedIndex];
                ((PhoneApplicationFrame)Application.Current.RootVisual).Navigate(new Uri("/MovieDetails.xaml", UriKind.Relative));

            }
        }                    
    }
}
