using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Proteus;

namespace BuildGui
{
    public class WebVideo
    {

        private string _strBaseUrl = "http://xhamster.com/new/1.html";

        private const int _intMaxMediaTimeMilliseconds = 60000;
        private System.Windows.Threading.DispatcherTimer _objTimer;
        private MediaElement _objMediaElement;

        public WebVideo( MediaElement me2)
        {

            _objMediaElement = me2;
            _objMediaElement.LoadedBehavior = MediaState.Manual;
            _objMediaElement.UnloadedBehavior = MediaState.Stop;
        }
        public List<string> _objUrlList;

        public void Start()
        {
            NextVideo();
            CreateTimer();
        }
        private void NextVideo()
        {
            BuildUrlList();

            if (_objUrlList.Count == 0)
                return;

            _objMediaElement.Stop();
            

            string html="";
            while (html == "")
            {
                System.Random rnd = new System.Random();
                int r = rnd.Next(_objUrlList.Count);

                string selectedUrl = _objUrlList[r];

                html = NetworkUtils.GetHtmlDataFromUrl(selectedUrl);
            }

            System.Text.RegularExpressions.Match match = System.Text.RegularExpressions.Regex.Match(html, "http.*\\.mp4");
            if (match != null)
            {
                _objMediaElement.Source = new Uri(match.Value, UriKind.RelativeOrAbsolute);
                _objMediaElement.MediaOpened += MediaOpenedHandler;
                _objMediaElement.MediaEnded += MediaEndedHandler;
                _objMediaElement.Play();
            }
        }
        private string EatNextUrl(ref string html)
        {
            int ind = 0;
            List<string> urlExtensions = new List<string>
            {
                "html",
                "htm",
                "php",
                "aspx",
                "asp"
            };

            string ret;
            foreach (string ext in urlExtensions)
            {
                ret = StringUtils.FindAndEatSubstringRange("http", ext, ref html, false);

                if ((ret != "") && (NetworkUtils.ValidateUrl(ret) == true))
                {
                    //call second time to eat the url
                    ret = StringUtils.FindAndEatSubstringRange("http", ext, ref html, true);
                    return ret;
                }
            }

            // **There are no more URLs
            html = "";

            return "";
        }
        // we can also use the channels
        private void BuildUrlList()
        {
            // Get list of video urls
            _objUrlList = new List<string>();

            HtmlAgilityPack.HtmlWeb hw = new HtmlAgilityPack.HtmlWeb();

            HtmlAgilityPack.HtmlDocument htmlDoc = hw.Load(_strBaseUrl);
            foreach(HtmlAgilityPack.HtmlNode link in htmlDoc.DocumentNode.SelectNodes("//a[@href]"))
            {
                string linkText = link.GetAttributeValue("href", null);
                if(!linkText.Equals("#"))
                    if (linkText.Contains("/movies"))
                        _objUrlList.Add(linkText);
            }

        }

        private void CreateTimer()
        {
            _objTimer = new System.Windows.Threading.DispatcherTimer();
            _objTimer.Interval = new TimeSpan(0, 0, 0, 0, _intMaxMediaTimeMilliseconds);
            _objTimer.Tick += new EventHandler(MediaTimeLimitHandler);
            _objTimer.Start();
        }
        private void MediaTimeLimitHandler(object sender, EventArgs e)
        {
            NextVideo();
        }
        private void MediaEndedHandler(object sender, System.Windows.RoutedEventArgs e)
        {
            // Play Another
        //    NextVideo();
        }
        private void MediaOpenedHandler(object sender, System.Windows.RoutedEventArgs e)
        {
            //seek - see https://msdn.microsoft.com/en-us/library/system.windows.controls.mediaelement.position.aspx
        
            int seconds = (int)_objMediaElement.NaturalDuration.TimeSpan.TotalSeconds;
            
            // **Seek 1/3 way through
            int seekSeconds = (int)((double)seconds * 0.3);
            _objMediaElement.Position = new TimeSpan(0, 0, seekSeconds);
        }


    

    }
}
