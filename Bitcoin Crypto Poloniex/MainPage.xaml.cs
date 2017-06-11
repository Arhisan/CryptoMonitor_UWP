using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Notifications;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Windows.Web.Http;
using Microsoft.Toolkit.Uwp.Notifications;

// Документацию по шаблону элемента "Пустая страница" см. по адресу https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x419

namespace Bitcoin_Crypto_Poloniex
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private ObservableCollection<CurrencyInfo> _items = 
            new ObservableCollection<CurrencyInfo>();
        public ObservableCollection<CurrencyInfo> Items
        {
            get { return this._items; }
        }


        public List<string> CurrencyNames = 
            new List<string>
            {
                "Bitcoin",
                "Etherium",
                "Litecoin",
                "Stellar",
                "ZCash",
                "Etherium Classic",
                "DigitalCash"
            };
        public List<string> CurrencyBriefNames = 
            new List<string>
            {
                "BTC",
                "ETH",
                "LTC",
                "STR",
                "ZEC",
                "ETC",
                "DASH"
            };

        public bool ToMonitor = true;
        public MainPage()
        {
            this.InitializeComponent();
            Monitoring(_items);
            Initialize();



        }

        public async Task<TileContent> GenerateTileContent()
        {
            return new TileContent()
            {
                Visual = new TileVisual()
                {
                    TileSmall = new TileBinding()
                    {
                        Content = new TileBindingContentAdaptive()
                        {
                            Children = {
                                new AdaptiveText()
                                {
                                    Text = "Bitcoin"
                                },

                                new AdaptiveText()
                                {
                                    Text = Items[0].BriefName,
                                    HintStyle = AdaptiveTextStyle.CaptionSubtle
                                },

                                new AdaptiveText()
                                {
                                    Text = Items[0].CurrentRateText,
                                    HintStyle = AdaptiveTextStyle.CaptionSubtle
                                }
                            }
                        }
                    },

                    TileMedium = new TileBinding()
                    {
                        Content = new TileBindingContentAdaptive()
                        {
                            Children = {
                                new AdaptiveText()
                                {
                                    Text = "Bitcoin"
                                },

                                new AdaptiveText()
                                {
                                    Text = Items[0].BriefName,
                                    HintStyle = AdaptiveTextStyle.CaptionSubtle
                                },

                                new AdaptiveText()
                                {
                                    Text = Items[0].CurrentRateText,
                                    HintStyle = AdaptiveTextStyle.CaptionSubtle
                                }
                            }
                        }
                    },

                    TileWide = new TileBinding()
                    {
                        Content = new TileBindingContentAdaptive()
                        {
                            Children =
                            {
                                new AdaptiveText()
                                {
                                    Text = "Bitcoin"
                                },

                                new AdaptiveText()
                                {
                                    Text = Items[0].BriefName,
                                    HintStyle = AdaptiveTextStyle.CaptionSubtle
                                },

                                new AdaptiveText()
                                {
                                    Text = Items[0].CurrentRateText,
                                    HintStyle = AdaptiveTextStyle.CaptionSubtle
                                }
                            }
                        }
                    },

                    TileLarge = new TileBinding()
                    {
                        Content = new TileBindingContentAdaptive()
                        {
                            Children =
                            {
                                new AdaptiveText()
                                {
                                    Text = "Bitcoin"
                                },

                                new AdaptiveText()
                                {
                                    Text = Items[0].BriefName,
                                    HintStyle = AdaptiveTextStyle.CaptionSubtle
                                },

                                new AdaptiveText()
                                {
                                    Text = Items[0].CurrentRateText,
                                    HintStyle = AdaptiveTextStyle.CaptionSubtle
                                }
                            }
                        }
                    }
                }
            };
        }
        public async void Initialize()
        {
            for (int i = 0; i < 7; i++)
            {
                Items.Add(new CurrencyInfo(CurrencyNames[i % 7], CurrencyBriefNames[i % 7]));
                await Task.Delay(1000);
            }
            var notification = new TileNotification((await GenerateTileContent()).GetXml());
            TileUpdateManager.CreateTileUpdaterForApplication().Update(notification);
        }

        public async void Monitoring(ObservableCollection<CurrencyInfo> listOfCurrencies)
        {
            var ci = CultureInfo.InvariantCulture.Clone() as CultureInfo;
            ci.NumberFormat.NumberDecimalSeparator = ".";
            HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.IfModifiedSince = DateTime.Now;

            httpClient.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml");
            httpClient.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate");
            httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.2; WOW64; rv:19.0) Gecko/20100101 Firefox/19.0");
            httpClient.DefaultRequestHeaders.Add("Accept-Charset", "ISO-8859-1");

            string httpResponseBody = "";
            Uri requestUri = new Uri("https://poloniex.com/public?command=returnTicker");

            for (;ToMonitor;)
            {
                try
                {
                    var httpResponse = await httpClient.GetAsync(requestUri);
                    httpResponse.EnsureSuccessStatusCode();
                    httpResponseBody = await httpResponse.Content.ReadAsStringAsync();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.StackTrace);
                }

                JObject tickInfo = JObject.Parse(httpResponseBody);
                for(int index = 0; index < _items.Count; index++)
                {
                    dynamic i = tickInfo.GetValue($"USDT_{_items[index].BriefName}");
                    string price = i.last.Value;
                    Items[index].CurrentRate = Double.Parse(price, ci);
                    Debug.WriteLine("Curr "+_items[index].CurrentRateText);
                }
                await Task.Delay(1000);
            }
        }
    }

    public class CurrencyInfo:INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public BitmapImage Pic { get; set; }
        public string Name { get; set; }
        public string BriefName { get; set; }

        private double _currentRate;
        public double CurrentRate
        {
            get => _currentRate;

            set
            {
                _currentRate = value;
                NotifyPropertyChanged("CurrentRateText");
            }
        }

        public string CurrentRateText =>  $"$ {CurrentRate:F4}".Replace(",","."); 

        public CurrencyInfo(string name, string briefname)
        {
            Pic = new BitmapImage(new Uri($"ms-appx:///Assets/icons/{briefname}.png"));
            Name = name;
            BriefName = briefname;
            CurrentRate = 0.00;
        }

        private void NotifyPropertyChanged(string PropertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
        }
    }
}
