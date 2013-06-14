using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Device.Location;
using Microsoft.Phone.Maps.Controls;
using Microsoft.Phone.Maps.Services;
using System.Text;
using System.Windows.Shapes;
using System.Windows.Media;

namespace MovieReader
{
    public partial class MovieLocations : PhoneApplicationPage
    {
        public MovieLocations()
        {
            InitializeComponent();

            //string navigationMessage = "";
            //this.NavigationContext.QueryString.TryGetValue("location", out navigationMessage);

            Maps_GeoCoding(PhoneApplicationService.Current.State["location"].ToString());
            
        }

        private void Maps_GeoCoding(string locationAddress)
        {
            GeocodeQuery query = new GeocodeQuery()
            {
                GeoCoordinate = new GeoCoordinate(0, 0),
                SearchTerm = locationAddress
            };
            query.QueryCompleted += query_QueryCompleted;
            query.QueryAsync();
        }

        void query_QueryCompleted(object sender, QueryCompletedEventArgs<IList<MapLocation>> e)
        {
            var item = e.Result[0];
                LocMap.Center = new GeoCoordinate(item.GeoCoordinate.Latitude, item.GeoCoordinate.Longitude);
                LocMap.ZoomLevel = 10;
                MapLayer layer0 = new MapLayer();

                Grid MyGrid = new Grid();
                MyGrid.RowDefinitions.Add(new RowDefinition());
                MyGrid.RowDefinitions.Add(new RowDefinition());
                MyGrid.Background = new SolidColorBrush(Colors.Transparent);

                //Creating a Rectangle
                Rectangle MyRectangle = new Rectangle();
                MyRectangle.Fill = new SolidColorBrush(Colors.Red);
                MyRectangle.Height = 20;
                MyRectangle.Width = 20;
                MyRectangle.SetValue(Grid.RowProperty, 0);
                MyRectangle.SetValue(Grid.ColumnProperty, 0);

                //Adding the Rectangle to the Grid
                MyGrid.Children.Add(MyRectangle);

                MapOverlay MyOverlay = new MapOverlay();
                MyOverlay.Content = MyGrid;

                MyOverlay.GeoCoordinate = new GeoCoordinate(item.GeoCoordinate.Latitude, item.GeoCoordinate.Longitude);
                MyOverlay.PositionOrigin = new Point(0, 0.5);

                MapLayer MyLayer = new MapLayer();
                MyLayer.Add(MyOverlay);
                LocMap.Layers.Add(MyLayer);
        }
    }
}