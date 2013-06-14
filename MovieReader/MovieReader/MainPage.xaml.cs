using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using MovieReader.Resources;
using MovieReader.ViewModelNamespace;
using System.Collections.ObjectModel;
using System.IO;
using HtmlAgilityPack;
using MovieReader.Model;
using Newtonsoft.Json.Linq;
using System.Windows.Media.Imaging;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.ComponentModel;
using System.Collections.ObjectModel;

namespace MovieReader
{
    public partial class MainPage : PhoneApplicationPage, INotifyPropertyChanged
    {

        public List<String> Positions = new List<string>();
        public List<String> Ids = new List<string>();
        public List<String> Titles = new List<string>();
        public List<String> WeekProfits = new List<string>();
        public List<String> TotalProfits = new List<string>();
        public List<String> IconsUrl = new List<string>();
        public List<String> ImdbIds = new List<string>();
        private ViewModel vm;
        public string Location = "";

        // Data context for the local database
        private MovieItemContext MovieDB;

        // Define an observable collection property that controls can bind to.
        private ObservableCollection<MovieItem> _movieItems;
        public ObservableCollection<MovieItem> MovieItems
        {
            get
            {
                return _movieItems;
            }
            set
            {
                if (_movieItems != value)
                {
                    _movieItems = value;
                    NotifyPropertyChanged("MovieItems");
                }
            }
        }

        // Constructor
        public MainPage()
        {
            InitializeComponent();
            vm = new ViewModel();
            GetMovies();
            // Sample code to localize the ApplicationBar
            //BuildLocalizedApplicationBar();    
            
            // Connect to the database and instantiate data context.
            MovieDB = new MovieItemContext(MovieItemContext.DBConnectionString);

            MovieItems = new ObservableCollection<MovieItem>();
            // Data context and observable collection are children of the main page.
            this.DataContext = this;

        }

        private void deleteTaskButton_Click(Object sender, RoutedEventArgs e)
        {
            deleteFavouriteMovie(sender);
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            // Define the query to gather all of the to-do items.
            var movieItemsInDB = from MovieItem movie in MovieDB.MovieItems
                                 select movie;

            // Execute the query and place the results into a collection.
            MovieItems = new ObservableCollection<MovieItem>(movieItemsInDB);
           

            // Call the base method.
            base.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
        {
            // Call the base method.
            base.OnNavigatedFrom(e);

            // Save changes to the database.
            MovieDB.SubmitChanges();
        }

        public void addFavouriteMovie(string Title, string Year, string Rating, string ImageUrl, string ImdbId)
        {
            MovieItem newMovie = new MovieItem { MovieTitle = Title, MovieYear = Year, MovieRating = Rating, MovieImageUrl = ImageUrl, MovieImdbId = ImdbId};
            MovieItems.Add(newMovie);

            MovieDB.MovieItems.InsertOnSubmit(newMovie);
            MovieDB.SubmitChanges();
        }

        public void deleteFavouriteMovie(object sender)
        {
            var button = sender as Button;

            if (button != null)
            {
                // Get a handle for the to-do item bound to the button.
                MovieItem movieToDelete = button.DataContext as MovieItem;

                // Remove the to-do item from the observable collection.
                MovieItems.Remove(movieToDelete);

                // Remove the to-do item from the local database.
                MovieDB.MovieItems.DeleteOnSubmit(movieToDelete);

                // Save changes to the database.
                MovieDB.SubmitChanges();

                // Put the focus back to the main page.
                this.Focus();
            }
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        // Used to notify the app that a property has changed.
        private void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion


        private void ContentPivot_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            if (ContentPivot.SelectedIndex == -1)
                return;

            if (ContentPivot.SelectedIndex == 1)
            {
                TitleTextBox.Text = "Search Movies";
            }
            else if (ContentPivot.SelectedIndex == 2)
            {
                TitleTextBox.Text = "Favourites";
            }
            else
            {
                TitleTextBox.Text = "US box office";
            }
        }
        
        public void GetMovies()
        {
            Uri url = new Uri("http://www.imdb.com/chart");
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);
            request.BeginGetResponse(new AsyncCallback(ReadWebRequestCallback), request);
        }

        public void ReadWebRequestCallback(IAsyncResult callbackResult)
        {
            ObservableCollection<Movie> a = new ObservableCollection<Movie>();
            HttpWebRequest myRequest = (HttpWebRequest)callbackResult.AsyncState;
            HttpWebResponse myResponse = (HttpWebResponse)myRequest.EndGetResponse(callbackResult);

            using (StreamReader httpwebStreamReader = new StreamReader(myResponse.GetResponseStream()))
            {
                string results = httpwebStreamReader.ReadToEnd();
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(results);
                HtmlNodeCollection cellNodes = doc.DocumentNode.SelectNodes("//div[@id=\"main\"]/table[1]//td");
                //a.Add(new Movie() { Position = "#", IconUrl = "", Title = "Title", Id = "", WeekProfit = "", TotalProfit = "Profit" });

                String tids = "";
                for (int i = 0; i < cellNodes.Count; i += 6)
                {
                    Positions.Add(cellNodes[i].ChildNodes["b"].InnerHtml);
                    IconsUrl.Add(cellNodes[i + 1].ChildNodes["img"].Attributes["src"].Value);
                    Titles.Add(cellNodes[i + 2].ChildNodes["a"].InnerText);
                    Ids.Add(cellNodes[i + 2].ChildNodes["a"].Attributes["href"].Value.ToString().Substring(7, 9));
                    WeekProfits.Add(cellNodes[i + 3].InnerHtml);
                    TotalProfits.Add(cellNodes[i + 4].InnerHtml);
                    int j = i / 6;

                    a.Add(new Movie() { Position = Positions[j], IconUrl = IconsUrl[j], Title = Titles[j], Id = Ids[j], WeekProfit = WeekProfits[j], TotalProfit = TotalProfits[j] });
                    tids += Ids[j] + " ";
                }

                PhoneApplicationService.Current.State["Ids"] = tids;
                vm.Movies = a;
                Dispatcher.BeginInvoke(() =>
                {
                    ItemViewOnPage.DataContext = from Movie in vm.Movies select Movie;
                });
            }
            myResponse.Close();
        }

        public void FillComponents(JObject movie)
        {
            Dispatcher.BeginInvoke(() =>
            {
                Title.Text = movie["title"].ToString();

                JArray genres = (JArray)movie["genres"];
                JArray cast = (JArray)movie["actors"];
                JArray directors = (JArray)movie["directors"];
                JArray writers = (JArray)movie["writers"];

                if (movie["poster"] != null)
                {
                    Uri uri = new Uri(movie["poster"].ToString(), UriKind.Absolute);
                    Poster.Source = new BitmapImage(uri);
                }
                else
                {
                    Uri uri = new Uri("http://images.all-free-download.com/images/graphiclarge/movie_clapper_91343.jpg");
                    Poster.Source = new BitmapImage(uri);
                }



                Cast.Text = "Cast:\n";
                foreach (string author in cast)
                {
                    Cast.Text += author + "\n";
                }

                Directors.Text = "Director: \n";

                Directors.Text += directors.First.ToString(); ;


                string raiting = "";
                if (movie["rating"] != null)
                    raiting = movie["rating"].ToString();

                Specs.Text = "Genre: \n" + genres.First.ToString() + "\n\n";
                if (!raiting.Equals(""))
                    Specs.Text += "Rating:\n" + raiting + "\n";
                else
                    Specs.Text += "Rating:\n - \n";

                Plot.Text = "Plot:\n " + movie["plot_simple"].ToString().Substring(0, 100) + "...";

                if (movie["filming_locations"]!=null)
                Location = movie["filming_locations"].ToString();

                LocationsButton.Visibility = System.Windows.Visibility.Visible;                
            });
        }

        public void GetMovie(string title)
        {
            Uri url = new Uri("http://mymovieapi.com/?q=" + title);
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);
            request.BeginGetResponse(new AsyncCallback(ReadWebRequestForMovieCallback), request);

        }

        private void ReadWebRequestForMovieCallback(IAsyncResult callbackResult)
        {
            HttpWebRequest myRequest = (HttpWebRequest)callbackResult.AsyncState;
            HttpWebResponse myResponse = (HttpWebResponse)myRequest.EndGetResponse(callbackResult);

            using (StreamReader httpwebStreamReader = new StreamReader(myResponse.GetResponseStream()))
            {
                string results = httpwebStreamReader.ReadToEnd();
                JArray movies = JArray.Parse(results);
                JObject movie = (JObject)movies[0];
                FillComponents(movie);
            }
            myResponse.Close();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (SearchTextBox.Text.Trim().Equals(""))
                MessageBox.Show("Enter Movie title!");
            else
                GetMovie(SearchTextBox.Text);
        }

        private void LocationsButton_Click(object sender, RoutedEventArgs e)
        {
            PhoneApplicationService.Current.State["location"] = Location;
            NavigationService.Navigate(new Uri("/MovieLocations.xaml", UriKind.Relative));
            
        }

        private void favouritesList_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            var movieItemsInDB = from MovieItem movie in MovieDB.MovieItems
                                 select movie.MovieImdbId;

            // Execute the query and place the results into a collection.
            ObservableCollection<string>  MovieItemsIds = new ObservableCollection<string>(movieItemsInDB);
            foreach (var item in MovieItemsIds)
            {
                ImdbIds.Add(item);
            }
            PhoneApplicationService.Current.State["id"] = ImdbIds[favouritesList.SelectedIndex].ToString();
            ((PhoneApplicationFrame)Application.Current.RootVisual).Navigate(new Uri("/MovieDetails.xaml", UriKind.Relative));
        }

        public Boolean alreadyFavourite(string id)
        {
            var movieItemsInDB = from MovieItem movie in MovieDB.MovieItems
                                 where movie.MovieImdbId==id
                                 select movie.MovieImdbId;

            ObservableCollection<string> ol = new ObservableCollection<string>(movieItemsInDB);
            if (ol.Count > 0) 
                return true;
            return false;
        }


    }

    [Table]
    public class MovieItem : INotifyPropertyChanged, INotifyPropertyChanging
    {
        // Define ID: private field, public property and database column.
        private int _movieItemId;

        [Column(IsPrimaryKey = true, IsDbGenerated = true, DbType = "INT NOT NULL Identity", CanBeNull = false, AutoSync = AutoSync.OnInsert)]
        public int MovieItemId
        {
            get
            {
                return _movieItemId;
            }
            set
            {
                if (_movieItemId != value)
                {
                    NotifyPropertyChanging("MovieItemId");
                    _movieItemId = value;
                    NotifyPropertyChanged("MovieItemId");
                }
            }
        }

        // Define item name: private field, public property and database column.
        private string _movieTitle;

        [Column]
        public string MovieTitle
        {
            get
            {
                return _movieTitle;
            }
            set
            {
                if (_movieTitle != value)
                {
                    NotifyPropertyChanging("MovieTitle");
                    _movieTitle = value;
                    NotifyPropertyChanged("MovieTitle");
                }
            }
        }

        private string _movieYear;

        [Column]
        public string MovieYear
        {
            get
            {
                return _movieYear;
            }
            set
            {
                if (_movieYear != value)
                {
                    NotifyPropertyChanging("MovieYear");
                    _movieYear = value;
                    NotifyPropertyChanged("MovieYear");
                }
            }
        }

        private string _movieRating;

        [Column]
        public string MovieRating
        {
            get
            {
                return _movieRating;
            }
            set
            {
                if (_movieRating != value)
                {
                    NotifyPropertyChanging("MovieRating");
                    _movieRating = value;
                    NotifyPropertyChanged("MovieRating");
                }
            }
        }

        private string _movieImageUrl;

        [Column]
        public string MovieImageUrl
        {
            get
            {
                return _movieImageUrl;
            }
            set
            {
                if (_movieImageUrl != value)
                {
                    NotifyPropertyChanging("MovieImageUrl");
                    _movieImageUrl = value;
                    NotifyPropertyChanged("MovieImageUrl");
                }
            }
        }

        private string _movieImdbId;

        [Column]
        public string MovieImdbId
        {
            get
            {
                return _movieImdbId;
            }
            set
            {
                if (MovieImdbId != value)
                {
                    NotifyPropertyChanging("MovieImdbId");
                    _movieImdbId = value;
                    NotifyPropertyChanged("MovieImdbId");
                }
            }
        }

        // Version column aids update performance.
        [Column(IsVersion = true)]
        private Binary _version;

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        // Used to notify the page that a data context property changed
        private void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion

        #region INotifyPropertyChanging Members

        public event PropertyChangingEventHandler PropertyChanging;

        // Used to notify the data context that a data context property is about to change
        private void NotifyPropertyChanging(string propertyName)
        {
            if (PropertyChanging != null)
            {
                PropertyChanging(this, new PropertyChangingEventArgs(propertyName));
            }
        }

        #endregion
    }

    public class MovieItemContext : DataContext
    {
        // Specify the connection string as a static, used in main page and app.xaml.
        public static string DBConnectionString = "Data Source=isostore:/MovieItem.sdf";

        // Pass the connection string to the base class.
        public MovieItemContext(string connectionString)
            : base(connectionString)
        { }

        // Specify a single table for the to-do items.
        public Table<MovieItem> MovieItems;
    }



}