using CommandLine;

namespace TwitterMediaBlocker
{
    public class Options
    {
        [Option("accesstoken", Required = true, HelpText = "The AccessToken linked to your Twitter Application.")]
        public string AccessToken { get; set; }

        [Option("accesstokensecret", Required = true, HelpText = "The AccessTokenSecret linked to your Twitter Application.")]
        public string AccessTokenSecret { get; set; }

        [Option("consumerkey", Required = true, HelpText = "The ConsumerKey linked to your Twitter Application.")]
        public string ConsumerKey { get; set; }

        [Option("consumersecret", Required = true, HelpText = "The ConsumerSecret linked to your Twitter Application.")]
        public string ConsumerSecret { get; set; }

        [Option('p', "photos", Required = false, Default = false, HelpText = "When provided, block users that reply with photos.")]
        public bool Photos { get; set; }

        [Option('g', "gifs", Required = false, Default = false, HelpText = "When provided, block users that reply with gifs.")]
        public bool Gifs { get; set; }

        [Option('v', "videos", Required = false, Default = false, HelpText = "When provided, block users that reply with videos.")]
        public bool Videos { get; set; }
    }
}
