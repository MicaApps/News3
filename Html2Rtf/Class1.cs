namespace Html2Rtf;

public class ConvertHtmlToRtfHelper
{
    public static string ConvertHtmlToRtf(string html)
    {
        string rtf;
        using (RichTextBox richTextBox = new RichTextBox())
        {
            richTextBox.Text = html;
            using (MemoryStream stream = new MemoryStream())
            {
                richTextBox.SaveFile(stream, RichTextBoxStreamType.RichText);
                stream.Seek(0, SeekOrigin.Begin);
                using (StreamReader reader = new StreamReader(stream))
                {
                    rtf = reader.ReadToEnd();
                }
            }
        }
        return rtf;
    }
}