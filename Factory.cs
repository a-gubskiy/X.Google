using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Xml;
using Google.GData.Blogger;
using Google.GData.YouTube;


namespace X.Google
{
    public static class Factory
    {
        public const string GoogleApplicationName = "x-framework-google-application";

        public static Publication CreateBlog(BloggerEntry bloggerEntry)
        {
            var ids = bloggerEntry.Id.AbsoluteUri.Split(new string[] { "post-" }, StringSplitOptions.RemoveEmptyEntries);
            var blogContent = bloggerEntry.Content.Content;

            var blog = new Publication
                {
                    Content = blogContent,
                    Description = Substring(GetPlainText(blogContent), 300),
                    TimeStamp = bloggerEntry.Published,
                    Id = ids[1],
                    BlogId = ids[0].Replace("tag:blogger.com,1999:blog-", String.Empty).Replace(".", String.Empty),
                    Title = bloggerEntry.Title.Text,
                    //Description = StringProcessor.Substring(StringProcessor.ToPlainText(blog.Content, true), 300, "..."),
                    Url = bloggerEntry.AlternateUri.ToString()

                };

            var tags = (from c in bloggerEntry.Categories
                        select c.Term.ToLower().Trim()).ToArray();

            blog.SetTags(tags);

            return blog;
        }

        private static string Substring(string text, int length)
        {
            if (!String.IsNullOrEmpty(text) && text.Length > length)
            {
                const string endPart = "...";
                return text.Substring(0, length - endPart.Length) + endPart;
            }

            return text;
        }

        private static string GetPlainText(string html)
        {
            var document = new HtmlAgilityPack.HtmlDocument();
            document.LoadHtml(html);

            document.OptionFixNestedTags = true;

            var text = document.DocumentNode.InnerText;
            return text;
        }

        public static Video CreateVideo(YouTubeEntry youTubeEntry)
        {
            var video = new Video();

            video.Title = youTubeEntry.Title.Text;
            video.Description = String.Format("Duration: {0}", youTubeEntry.Duration.IntegerValue);
            video.Url = youTubeEntry.AlternateUri.ToString();
            video.TimeStamp = youTubeEntry.Published;
            video.Id = youTubeEntry.VideoId;

            var tags = (from c in youTubeEntry.Categories
                        select c.Term.ToLower().Trim().Replace("http://gdata.youtube.com/schemas/2007#", String.Empty)).ToArray();

            video.SetTags(tags);

            return video;
        }

        public static IEnumerable<Photo> LoadPhotoListFromAlbumXml(XmlDocument xmlDocument, string albumId, string userId)
        {
            if (xmlDocument == null || String.IsNullOrEmpty(xmlDocument.InnerXml))
            {
                return new List<Photo>();
            }

            var nodes = (from n in xmlDocument.ChildNodes[1].Cast<XmlNode>().ToList()
                         where n.Name == "entry"
                         select n).ToList();

            var result = new List<Photo>();

            foreach (var node in nodes)
            {
                var title = (from n in node.ChildNodes.Cast<XmlNode>()
                             where n.Name == "title"
                             select n.InnerText).SingleOrDefault();

                var timeStamp = (from n in node.ChildNodes.Cast<XmlNode>()
                                 where n.Name == "published"
                                 select DateTime.Parse(n.InnerText)).SingleOrDefault();

                var id = (from n in node.ChildNodes.Cast<XmlNode>()
                          where n.Name == "id"
                          select n.InnerText).SingleOrDefault().ToLower();

                id = id.ToLower().Replace(String.Format("http://picasaweb.google.com/data/entry/base/user/{0}/albumid/{1}/photoid/", userId, albumId).ToLower(), String.Empty);
                id = id.Replace("?hl=en_us", String.Empty);

                var mediaNode = (from n in node.ChildNodes.Cast<XmlNode>()
                                 where n.Name == "media:group"
                                 select n).SingleOrDefault();

                var mediaAttributes = (from n in mediaNode.ChildNodes.Cast<XmlNode>()
                                       where n.Name == "media:content"
                                       select n.Attributes.Cast<XmlAttribute>().ToList()).SingleOrDefault();


                var url = (from a in mediaAttributes
                           where a.Name == "url"
                           select a.Value).SingleOrDefault();

                var width = (from a in mediaAttributes
                             where a.Name == "width"
                             select int.Parse(a.Value)).SingleOrDefault();

                var height = (from a in mediaAttributes
                              where a.Name == "height"
                              select int.Parse(a.Value)).SingleOrDefault();

                var thumbnailNode = (from n in mediaNode.ChildNodes.Cast<XmlNode>()
                                     where n.Name == "media:thumbnail"
                                     select n).FirstOrDefault();

                var smalUrl = thumbnailNode.Attributes.GetNamedItem("url").Value;
                smalUrl = smalUrl.Replace("s72", "s144").Replace("s288", "s144").Replace("s640", "s144");


                var photo = new Photo
                    {
                        Id = id,
                        AlbumId = albumId,
                        Title = title,
                        TimeStamp = timeStamp,
                        Url = url,
                        Size = new Size(width, height),
                        PreviewUrl = smalUrl,
                        //Description = picasaEntry.Summary.Text,
                        Description = ""
                    };

                result.Add(photo);

            }

            return result;
        }
    }
}
