using System;
using System.Collections.Generic;
using System.Text;

namespace X.Google
{
    [Serializable]
    public class Video : GoogleEntryBase, ITagged
    {
        private List<string> _tags = new List<string>();

        public override IEnumerable<string> Tags { get { return _tags; } }

        public string SmallImagePreview
        {
            get { return String.Format("http://img.youtube.com/vi/{0}/1.jpg", Id); }
        }

        public string GetEmbededCode(VideoSize size)
        {
            int width = 0;
            int height = 0;

            switch (size)
            {
                case VideoSize.Small:
                    width = 425;
                    height = 344;
                    break;
                case VideoSize.Medium:
                    width = 480;
                    height = 385;
                    break;
                case VideoSize.Large:
                    width = 640;
                    height = 505;
                    break;
                case VideoSize.VeryLarge:
                    width = 960;
                    height = 745;
                    break;
            }

            var url = String.Format("http://www.youtube.com/v/{0}", Id);

            var sb = new StringBuilder();

            sb.AppendFormat("<object width=\"{0}\" height=\"{1}\">", width, height);
            sb.AppendFormat("<param name=\"movie\" value=\"{0}\"></param>", url);
            sb.AppendFormat("<param name=\"allowFullScreen\" value=\"true\"></param><param name=\"allowscriptaccess\" value=\"always\"></param>");
            sb.AppendFormat("<embed src=\"{0}\" type=\"application/x-shockwave-flash\" allowscriptaccess=\"always\" allowfullscreen=\"true\" width=\"{1}\" height=\"{2}\"></embed>", url, width, height);
            sb.AppendFormat("</object>");

            return sb.ToString();
        }

        public void SetTags(IEnumerable<string> tags)
        {
            _tags.Clear();
            _tags.AddRange(tags);
        }
    }
}
