using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace X.Google
{
    [Serializable]
    public class Album : GoogleEntryBase, IEnumerable<Photo>
    {
        private List<Photo> _photos;
        private readonly Repository _repository;

        public bool UseCache { get; set; }
        public string UserName { get; set; }
        public Photo CoverPhoto { get; set; }
        public string UserId { get; set; }

        public Album()
        {
            UseCache = true;
            _photos = new List<Photo>();
        }

        internal Album(Repository repository)
        {
            UseCache = true;
            _repository = repository;
            _photos = new List<Photo>();
        }

        public IEnumerable<Photo> Photos
        {
            get
            {
                if (_photos.Count == 0 || UseCache == false)
                {
                    _photos = _repository.GetAlbumPhotos(this.Id).ToList();
                }

                return _photos;
            }
        }

        public IEnumerator<Photo> GetEnumerator()
        {
            return Photos.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Photos.GetEnumerator();
        }

        public override IEnumerable<string> Tags
        {
            get { return new string[] { Title }; }
        }

        //public void Add(Photo photo)
        //{
        //    _photos.Add(photo);
        //}
    }
}

