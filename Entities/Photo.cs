using System;
using System.Collections.Generic;
using System.Drawing;

namespace X.Google
{
    [Serializable]
    public class Photo : GoogleEntryBase
    {
        public string AlbumId { get; set; }

        public string PreviewUrl { get; set; }
        public Size Size { get; set; }

        public Photo()
        {
        }

        public string ClearTitle
        {
            get
            {
                return Title.Replace(".jpeg", String.Empty).Replace(".jpg", String.Empty).Replace(".png", String.Empty).Replace(".PNG", String.Empty).Replace(".JPEG", String.Empty).Replace(".JPG", String.Empty);
            }
        }

        /// <summary>
        /// Return 72px image
        /// </summary>
        public string UltraSmallPreviewUrl
        {
            get { return GetSizeUrl(72); }
        }

        /// <summary>
        /// Return 640px image
        /// </summary>
        public string MediumPreviewUrl
        {
            get { return GetSizeUrl(640); }
        }

        /// <summary>
        /// Return 1024px image
        /// </summary>
        public string BigPreviewUrl
        {
            get { return GetSizeUrl(1024); }
        }
        
        /// <summary>
        /// Get url for required size
        /// </summary>
        /// <param name="size"></param>
        /// <returns></returns>
        public string GetSizeUrl(int size)
        {
            var strSize = String.Format("s{0}", size);

            return PreviewUrl.Replace("s72", strSize)
                      .Replace("s144", strSize)
                      .Replace("s160", strSize)
                      .Replace("s1024", strSize);
        }
        
        public override IEnumerable<string> Tags
        {
            get { return new string[] { Title, ClearTitle }; }
        }
    }
}
