using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
#if WINDOWS_UWP
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
#else
using System.Windows.Data;
using System.Windows.Documents;
#endif
using GalaSoft.MvvmLight.Messaging;
using UnIRC.Shared.Messages;

namespace UnIRC.Converters
{
    public class AutoHyperlinkInlinesConverter : IValueConverter
    {
        private const string regex =
            //@"(?i)((?:\\\\[a-z0-9\-]+|\b(?:[a-z][\w-]+:(?:/{1,3}|[a-z0-9%])|www\d{0,3}[.]|[a-z0-9.\-]+[.][a-z]{2,4}/))(?:[^\s()<>]+|\(([^\s()<>]+|(\([^\s()<>]+\)))*\))+(?:\(([^\s()<>]+|(\([^\s()<>]+\)))*\)|[^\s`!()\[\]{};:'"".,<>?«»“”‘’]))";
            @"(?i)\b(?:" +
            @"(?<remote>(?<protocol>[a-z][\w-]+):/+|(?<share>\\\\))" +
            @"(?>(?<username>(?>[\w#$%&'*+-/=?^_`{|}~.]+)(?=:))(?::(?<password>[^\s/@]*)|)@)?" +
            @"(?<host>(?:[\w-]+\.)+(?<tld>[\w-]{2,16}(?=\b))|[\w-]+)(?::(?<port>\d{1,6}))?" +
            @"|" +
            @"(?>(?<username>(?>[\w#$%&'*+-/=?^_`{|}~.]+)(?=:))(?::(?<password>[^\s/@]*)|)@)?" +
            @"(?<host>(?:[\w-]+\.)+(?<tld>[\w-]{2,16}(?=\b)))(?::(?<port>\d{1,6}))?" +
            @")" +
            @"(?<path>[\\/](?:[^\s#?]*[^\s#?""'<>[\](),.;:])?)?" +
            @"(?:\?(?<params>[\w-.]+(?:=[^\s#""'<>[\](),.;:]+)?(?:&[\w-.]+(?:=[^\s#""'<>[\](),.;:]+)?)*))?" +
            @"(?:\#(?<hash>[^\s#""'<>[\](),.;:]*(?:[[(!][^\s#""'<>[\](),.;:]+[\])!])?))?";

        public object Convert(object value, Type targetType, object parameter,
#if WINDOWS_UWP
            string language)
#else
            CultureInfo culture)
#endif
        {
            var str = value as string;
            var inlines = new List<Inline>();
            if (str == null) return inlines;
            //inlines.Add(new Run {Text = str});
            //return inlines;

            MatchCollection matches = Regex.Matches(str, regex, RegexOptions.Compiled);
            int matchCount = matches.Count;
            int currentIndex = 0;

            for (int m = 0; m < matchCount; m++)
            {
                Match match = matches[m];

                int preLength = match.Index - currentIndex;
                string preText = str.Substring(currentIndex, preLength);
                inlines.Add(new Run {Text = preText});
                currentIndex += preLength;
                currentIndex += match.Length;


                string uriText = match.Value;
                string uri = uriText;
                if (match.Groups["share"].Success)
                    uri = "file:" + uri.Replace('\\', '/');
                else if (!match.Groups["protocol"].Success)
                    uri = "http://" + uri;

                Hyperlink link;
                try
                {
#if WINDOWS_UWP
                    link = new Hyperlink
#else
                    link = new Hyperlink
#endif
                    {
                        NavigateUri = new Uri(uri),
                        Inlines = {new Run {Text = uriText}}
                    };
                }
                catch (Exception ex)
                {
                    Messenger.Default.Send(new ErrorMessage($"Error creating hyperlink for '{uri}'.", ex));
                    continue;
                }
#if DESKTOP2
                link.Click += (o, e) =>
                {
                    try
                    {
                        Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
                    }
                    catch (Exception ex)
                    {
                        Messenger.Default.Send(new ErrorMessage(
                            "Couldn't open hyperlink " + e.Uri.AbsoluteUri, ex));
                    }
                };
#endif
                inlines.Add(link);
            }
            int lastLength = str.Length - currentIndex;
            string finalText = str.Substring(currentIndex, lastLength);
            inlines.Add(new Run {Text = finalText});
            return inlines;
        }

        public object ConvertBack(object value, Type targetType, object parameter,
#if WINDOWS_UWP
            string language)
#else
            CultureInfo culture)
#endif
        {
            throw new NotImplementedException();
        }
    }
}
