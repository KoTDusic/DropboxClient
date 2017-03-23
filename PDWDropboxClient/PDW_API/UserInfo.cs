using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Net;
using System.IO;

namespace DropboxTest
{
    public class UserInfo
    {
        public string Email {get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string AvatarUri { get; set; }
        public string RefLink { get; set; }
        public string Locale { get; set; }
        public ImageSource Avatar { get; set; }
        public string Country { get; set; }
        public SpaceInfo spaceInfo { get; set; }

        public UserInfo(string email, string name, string surname,string avatarUri,string refLink,string locale,string country)
        {
            Email = email;
            Name = name;
            Surname = surname;
            AvatarUri = avatarUri;
            RefLink = refLink;
            Locale = locale;
            Country = country;
        }
        public ImageSource LoadAvatar()
        {
            try
            {
                HttpWebRequest client = WebRequest.CreateHttp(AvatarUri);
                client.Proxy = null;
                WebResponse response = client.GetResponse();
                Avatar = BitmapFrame.Create(response.GetResponseStream(), BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
            }
            catch (Exception){return null;}
            return Avatar;
        }
    }
}
