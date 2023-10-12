﻿using HtmlAgilityPack;

namespace FomoDog
{
    public class MetadataDownloader
    {
        public async Task<Metadata> DownloadMetadata(string url)
        {
            string html = await DownloadHtml(url);
            return ExtractMetadata(html);
        }

        private async Task<string> DownloadHtml(string url)
        {
            try
            {
                using var client = new HttpClient();
                return await client.GetStringAsync(url);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error downloading HTML: " + ex.Message);
                return null;
            }
        }

        private static Metadata ExtractMetadata(string html)
        {
            Metadata result = new Metadata();

            if (string.IsNullOrEmpty(html))
                return result;

            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(html);

            HtmlNode titleNode = document.DocumentNode.SelectSingleNode("//head//title");
            if (titleNode != null)
                result.Title = titleNode.InnerText.Trim();

            HtmlNode descriptionNode = document.DocumentNode.SelectSingleNode("//head//meta[@name='description']");
            if (descriptionNode != null)
            {
                HtmlAttribute contentAttribute = descriptionNode.Attributes["content"];
                if (contentAttribute != null)
                    result.Description = contentAttribute.Value;
            }

            return result;
        }
    }

    public class Metadata
    {
        public string Title { get; set; }
        public string Description { get; set; }
    }
}
