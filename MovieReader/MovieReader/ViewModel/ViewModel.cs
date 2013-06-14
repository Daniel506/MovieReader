using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MovieReader.Model;
using System.IO.IsolatedStorage;
using System.Runtime.Serialization.Json;
using System.Net;
using System.Windows;
using HtmlAgilityPack;
using System.IO;
using System.Xml;
using Microsoft.Phone.Net.NetworkInformation;

namespace MovieReader.ViewModelNamespace
{
    class ViewModel
    {
        public ObservableCollection<Movie> Movies { get; set; }
    }
}
