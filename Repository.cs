using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Xml;
using Google.GData.Blogger;
using Google.GData.YouTube;
using X.Google.Caching;

namespace X.Google
{
    [Serializable]
    public class Repository 
    {
        public string Name { get; set; }

        public Cache Cache { get; private set; }

        public bool RetrieveAllPost { get; set; }
        public virtual GoogleAccount GoogleAccount { get; set; }

        public Repository()
        {
            Cache = new FakeCache();
            RetrieveAllPost = false;
            Name = "unknown";
        }

        public Repository(Cache cache)
            : this()
        {
            Cache = cache;
        }

        protected virtual string CreateCacheKey(string key)
        {
            return String.Format("x_google_repository_{0}_{1}", Name, key);
        }

        #region IGoogleRepository

        public virtual IEnumerable<Album> Albums
        {
            get
            {
                var cacheKey = CreateCacheKey("albums");
                var albums = (IEnumerable<Album>)Cache[cacheKey];

                if (albums == null)
                {
                    albums = GetAlbums();
                    Cache.Insert(cacheKey, albums);
                }

                return albums;
            }
        }

        public virtual IEnumerable<Video> FavoriteVideos
        {
            get
            {
                var cacheKey = CreateCacheKey("favorite_videos");
                var favoriteVideos = (IEnumerable<Video>)Cache[cacheKey];

                if (favoriteVideos == null)
                {
                    favoriteVideos = GetFavoriteVideos();
                    Cache.Insert(cacheKey, favoriteVideos);
                }

                return favoriteVideos;
            }
        }

        public virtual IEnumerable<Video> Videos
        {
            get
            {
                var cacheKey = CreateCacheKey("videos");
                var videos = (IEnumerable<Video>)Cache[cacheKey];

                if (videos == null)
                {
                    videos = GetVideos();
                    Cache.Insert(cacheKey, videos);
                }

                return videos;
            }
        }

        public virtual IEnumerable<Publication> Publications
        {
            get
            {
                var cacheKey = CreateCacheKey("publication");
                var publications = (IEnumerable<Publication>)Cache[cacheKey];

                if (publications == null)
                {
                    publications = GetPublications();
                    Cache.Insert(cacheKey, publications);
                }

                return publications;
            }
        }


        public virtual Album GetAlbum(string albumId)
        {
            Album album;

            //if (Albums != null && Albums.Count() != 0)
            //{
            //    album = Albums.FirstOrDefault(x => x.Id == albumId);
            //}
            //else
            //{
                var key = CreateCacheKey(albumId);
                album = Cache[key] as Album;

                if (album == null)
                {
                    var url = String.Format( "https://picasaweb.google.com/data/feed/base/user/{0}/albumid/{1}?alt=rss&kind=photo",
                            GoogleAccount.PicasaUserName, albumId);
                    try
                    {
                        
                        var xmlDocument = LoadXmlDocument(url);
                        var rss = X.Web.RSS.RssDocument.Load(xmlDocument.InnerXml);
                        
                        var title = rss.Channel.Title;
                        var description = rss.Channel.Description;
                        
                        var coverPhoto = new Photo
                                             {
                                                 Url = rss.Channel.Image.Url.UrlString,
                                                 AlbumId = albumId,
                                                 PreviewUrl = rss.Channel.Image.Url.UrlString,
                                             };

                        album = new Album(this)
                                    {
                                        UserName = GoogleAccount.PicasaUserName,
                                        UserId = GoogleAccount.PicasaUserName,
                                        Id = albumId,
                                        Title = title,
                                        CoverPhoto = coverPhoto,
                                        Description = description
                                    };

                        Cache[key] = album;
                        
                    }
                    catch { }

                }
            //}

            return album;
        }

        private static XmlDocument LoadXmlDocument(string url)
        {
            var request = WebRequest.Create(url);

            Stream stream = null;

            try
            {
                var response = request.GetResponse();
                stream = response.GetResponseStream();
            }
            catch { }

            if (stream != null)
            {
                var streamReader = new StreamReader(stream);
                var xml = streamReader.ReadToEnd();
                streamReader.Close();

                var xmlDocument = new XmlDocument();
                xmlDocument.LoadXml(xml);

                return xmlDocument;
            }

            return null;
        }

        public virtual Publication GetPublication(string publicationId)
        {
            return Publications.FirstOrDefault(x => x.Id == publicationId);
        }

        public virtual Video GetVideo(string id)
        {
            var allVideos = new List<Video>();
            allVideos.AddRange(Videos);
            allVideos.AddRange(FavoriteVideos);

            return allVideos.FirstOrDefault(x => x.Id == id);
        }

        public virtual IEnumerable<Photo> GetAlbumPhotos(string albumId)
        {
            var cacheKey = CreateCacheKey(String.Format("photos_of_album_{0}", albumId));

            var photos = (IEnumerable<Photo>)Cache[cacheKey];

            if (photos == null)
            {
                var photoXmlDescriptionUrl = String.Format("http://picasaweb.google.com/data/feed/base/user/{0}/albumid/{1}", GoogleAccount.PicasaUserName, albumId);
                var xmlDocument = LoadXmlDocument(photoXmlDescriptionUrl);

                photos = Factory.LoadPhotoListFromAlbumXml(xmlDocument, albumId, GoogleAccount.PicasaUserName);
                Cache.Insert(cacheKey, photos);
            }

            return photos;
        }

        #endregion

        #region internal logic

        private IEnumerable<Publication> GetPublications()
        {
            var url = String.Format("http://www.blogger.com/feeds/{0}/posts/default ", GoogleAccount.BlogId);
            var publications = LoadPublications(url, RetrieveAllPost);
            return publications;
        }

        private IEnumerable<Video> GetVideos()
        {
            var url = String.Format("http://gdata.youtube.com/feeds/api/users/{0}/uploads", GoogleAccount.YouTubeUserName);
            var videos = LoadVideos(url);
            return videos;
        }

        private IEnumerable<Video> GetFavoriteVideos()
        {
            var url = String.Format("http://gdata.youtube.com/feeds/api/users/{0}/favorites", GoogleAccount.YouTubeUserName);
            var favoritesVideos = LoadVideos(url);
            return favoritesVideos;
        }

        private IEnumerable<Album> GetAlbums()
        {
            var url = "http://picasaweb.google.com/data/feed/api/user/" + GoogleAccount.PicasaUserName;
            var albums = LoadAlbums(url, this);
            return albums;
        }

        #endregion

        public virtual Publication GetSinglePublication(string id)
        {
            try
            {
                var query = new BloggerQuery(String.Format("http://www.blogger.com/feeds/{0}/posts/default/{1} ", GoogleAccount.BlogId, id));

                var bloggerService = new BloggerService(Factory.GoogleApplicationName);
                var feed = bloggerService.Query(query);
                var postEntry = (BloggerEntry)feed.Entries.FirstOrDefault();
                return Factory.CreateBlog(postEntry);
            }
            catch
            {
                return null;
            }
        }

        private static IEnumerable<Album> LoadAlbums(string url, Repository googleRepository)
        {
            XmlDocument xmlDocument = LoadXmlDocument(url);

            if (xmlDocument == null)
            {
                return null;
            }

            var nodes = (from n in xmlDocument.ChildNodes[1].ChildNodes.Cast<XmlNode>()
                         where n.Name == "entry"
                         select n).ToList();

            var albums = new List<Album>();

            foreach (var node in nodes)
            {
                var mediaNode = (from n in node.ChildNodes.Cast<XmlNode>()
                                 where n.Name == "media:group"
                                 select n).SingleOrDefault();

                var description = "";

                try
                {
                    description = (from n in mediaNode.ChildNodes.Cast<XmlNode>()
                                   where n.Name == "media:description"
                                   select n).SingleOrDefault().InnerText;

                }
                catch { }


                var thumbnailNode = (from n in mediaNode.ChildNodes.Cast<XmlNode>()
                                     where n.Name == "media:thumbnail"
                                     select n).SingleOrDefault();

                var title = (from n in node.ChildNodes.Cast<XmlNode>()
                             where n.Name == "title"
                             select n.InnerText).SingleOrDefault();

                var albumId = (from n in node.ChildNodes.Cast<XmlNode>()
                               where n.Name == "id"
                               select n.InnerText).SingleOrDefault().ToLower();

                var index = albumId.IndexOf("albumid/");
                albumId = albumId.Remove(0, index + 8);


                var coverUrl = (from a in thumbnailNode.Attributes.Cast<XmlAttribute>()
                                where a.Name == "url"
                                select a.Value).SingleOrDefault();



                var coverPhoto = new Photo
                                     {
                                         Url = coverUrl,
                                         AlbumId = albumId,
                                         PreviewUrl = coverUrl,
                                     };

                var album = new Album(googleRepository)
                                {
                                    UserName = googleRepository.GoogleAccount.PicasaUserName,
                                    UserId = googleRepository.GoogleAccount.PicasaUserName,
                                    Id = albumId,
                                    Title = title,
                                    CoverPhoto = coverPhoto,
                                    Description = description
                                };


                albums.Add(album);
            }

            return albums;
        }

        private static IEnumerable<Video> LoadVideos(string url)
        {
            var youTubeService = new YouTubeService(Factory.GoogleApplicationName);

            var query = new YouTubeQuery(url);

            var feed = youTubeService.Query(query);

            var videos = new List<Video>();

            foreach (YouTubeEntry entry in feed.Entries)
            {
                var video = Factory.CreateVideo(entry);
                videos.Add(video);
            }

            return videos;
        }

        private static IEnumerable<Publication> LoadPublications(string url, bool retrieveAllPost)
        {
            url = url.Trim();

            var publications = new List<Publication>();

            var bloggerService = new BloggerService(Factory.GoogleApplicationName);
            var query = new BloggerQuery(url);

            try
            {
                var feed = bloggerService.Query(query);

                if (retrieveAllPost && feed.TotalResults > feed.Entries.Count)
                {
                    query.NumberToRetrieve = feed.TotalResults;
                    feed = bloggerService.Query(query);
                }

                foreach (BloggerEntry entry in feed.Entries)
                {
                    var blog = Factory.CreateBlog(entry);
                    publications.Add(blog);
                }

                return publications;
            }
            catch
            {
                return new List<Publication>();
            }
        }

        public virtual bool Available
        {
            get { return true; }
        }

        public int SaveChanges()
        {
            throw new NotSupportedException();
        }

        public void Dispose()
        {
            
        }
    }
}
