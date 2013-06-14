using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Windows.Media.Imaging;
using Newtonsoft.Json.Linq;
using System.IO;

namespace MovieReader
{
    public partial class MovieDetails : PhoneApplicationPage
    {
        public string id = "";
        public string year = "";
        public string title = "";
        public string rating = "";
        public string imgUrl = "";
        public bool favourite = false;
        MainPage mp = new MainPage();

        public MovieDetails()
        {
            InitializeComponent();
            id = PhoneApplicationService.Current.State["id"].ToString();
            GetMovie(id);
        }

        private void FavouritesButton_Click(object sender, RoutedEventArgs e)
        {

            if (rating == "") rating = "-";
            mp.addFavouriteMovie(title, year, rating, imgUrl, id);
            MessageBox.Show("Movie successfully added to your favourites!");
            FavouritesButton.IsEnabled = false;
        }

        public void GetMovie(string id)
        {
            Uri url = new Uri("http://mymovieapi.com/?id=" + id);
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
                JObject movie = JObject.Parse(results);
                FillComponents(movie);
            }
            myResponse.Close();
        }

        public void FillComponents(JObject movie)
        {
            Dispatcher.BeginInvoke(() =>
            {
                Title.Text = movie["title"].ToString();
                title = movie["title"].ToString();
                year = movie["year"].ToString();

                JArray genres = (JArray)movie["genres"];
                JArray cast = (JArray)movie["actors"];
                JArray directors = (JArray)movie["directors"];
                JArray writers = (JArray)movie["writers"];

                if (movie["poster"] != null)
                {
                    Uri uri = new Uri(movie["poster"].ToString(), UriKind.Absolute);
                    Poster.Source = new BitmapImage(uri);
                    imgUrl = movie["poster"].ToString();
                }
                else
                {
                    Uri uri = new Uri("http://images.all-free-download.com/images/graphiclarge/movie_clapper_91343.jpg");
                    Poster.Source = new BitmapImage(uri);
                    imgUrl = "http://images.all-free-download.com/images/graphiclarge/movie_clapper_91343.jpg";
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
                rating = raiting;

                Specs.Text = "Genre: \n" + genres.First.ToString() + "\n\n";
                if (!raiting.Equals(""))
                    Specs.Text += "Rating:\n" + raiting + "\n";
                else
                    Specs.Text += "Rating:\n - \n";


                if (movie["plot_simple"].ToString().Length > 100)
                    Plot.Text = "Plot:\n " + movie["plot_simple"].ToString().Substring(0, 100) + "...";
                else
                    if (movie["plot_simple"] == null) Plot.Text = "Plot:\n ...";
                    else
                        Plot.Text = "Plot:\n " + movie["plot_simple"].ToString();

                favourite = mp.alreadyFavourite(id);
                if (favourite)
                    FavouritesButton.IsEnabled = false;
                else
                    FavouritesButton.IsEnabled = true;

                FavouritesButton.Visibility = System.Windows.Visibility.Visible;
            });
        }
    }
}