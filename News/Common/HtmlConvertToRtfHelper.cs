using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml.Documents;

namespace News.Common
{/*
    internal class HtmlConvertToRtfHelper
    {
        public static string HtmlConvertToRtf(string html)
        {
            FlowDocument flowDocument = new FlowDocument();
            TextRange textRange = new TextRange(flowDocument.ContentStart, flowDocument.ContentEnd);
            using (var stringReader = new StringReader(htmlContents))
            {
                textRange.Load(stringReader, DataFormats.Html);
            }

            // 使用MemoryStream来保存RTF内容
            MemoryStream memoryStream = new MemoryStream();
            textRange = new TextRange(flowDocument.ContentStart, flowDocument.ContentEnd);
            textRange.Save(memoryStream, DataFormats.Rtf);
        }
    }*/
}
