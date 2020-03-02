using CommandLine;
using CommandLine.Text;
using System;
using System.Linq;
using System.Threading.Tasks;
using Tweetinvi;
using Tweetinvi.Models;
using Tweetinvi.Streaming;

namespace TwitterMediaBlocker
{
    public class Program
    {
        private static string accessToken;
        private static string accessTokenSecret;
        private static string consumerKey;
        private static string consumerSecret;
        private static bool blockPhotos;
        private static bool blockGifs;
        private static bool blockVideos;
        private static IFilteredStream stream;
        private static IUser user;

        public static async Task Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args)
                .WithParsed<Options>(o =>
                {
                    if (!o.Photos && !o.Gifs && !o.Videos)
                    {
                        Console.WriteLine("\nYou need to provide at least one type of media to be blocked.");
                        Console.WriteLine("\t-p | photos");
                        Console.WriteLine("\t-g | gifs");
                        Console.WriteLine("\t-v | videos");
                        Environment.Exit(0);
                    }

                    accessToken = o.AccessToken;
                    accessTokenSecret = o.AccessTokenSecret;
                    consumerKey = o.ConsumerKey;
                    consumerSecret = o.ConsumerSecret;
                    blockPhotos = o.Photos;
                    blockGifs = o.Gifs;
                    blockVideos = o.Videos;
                })
                .WithNotParsed<Options>(error =>
                {
                    Environment.Exit(0);
                });

            try
            {
                Authenticate();
                InitializeStream();
                await stream.StartStreamMatchingAnyConditionAsync();
            }
            catch (Exception)
            {
                Console.WriteLine("Failed to authenticate. Are your credentials correct?");
            }
        }

        private static void Authenticate()
        {
            Auth.SetCredentials(new TwitterCredentials
            {
                AccessToken = accessToken,
                AccessTokenSecret = accessTokenSecret,
                ConsumerKey = consumerKey,
                ConsumerSecret = consumerSecret
            });

            user = User.GetAuthenticatedUser();
        }

        private static void InitializeStream()
        {
            stream = Stream.CreateFilteredStream();
            stream.AddFollow(user);
            stream.StreamStopped += StreamStopped;
            stream.StreamStarted += StreamStarted;
            stream.MatchingTweetReceived += MatchingTweetReceived;
        }

        private static void MatchingTweetReceived(object sender, Tweetinvi.Events.MatchedTweetReceivedEventArgs e)
        {
            var tweet = e.Tweet;
            if (tweet.InReplyToUserId == user.Id && tweet.Media.Any())
            {
                string mediaType = GetMediaType(tweet);
                if ((mediaType == "photo" && blockPhotos)
                 || (mediaType == "video" && blockVideos)
                 || (mediaType == "gif"   && blockGifs))
                {
                    Console.WriteLine($"Blocking @{tweet.CreatedBy.ScreenName} because they replied with a {mediaType} to https://www.twitter.com/i/status/{tweet.InReplyToStatusId}");
                    User.BlockUser(tweet.CreatedBy);
                }
            }
        }

        private static void RestartStream()
        {
            Console.WriteLine("Restarting...");
            stream.StopStream();
            InitializeStream();
            stream.StartStreamMatchingAnyConditionAsync();
        }

        private static void StreamStarted(object sender, EventArgs e)
        {
            Console.WriteLine("Tweet monitoring has been started...");
        }

        private static void StreamStopped(object sender, Tweetinvi.Events.StreamExceptionEventArgs e)
        {
            Console.WriteLine("Tweet monitoring has been stopped...");
            Console.Write("Error(s): ");
            if (e != null && e.DisconnectMessage != null)
                Console.WriteLine(e.DisconnectMessage.Reason);
            else
                Console.WriteLine();

            RestartStream();
        }

        private static string GetMediaType(ITweet tweet)
        {
            if (tweet.Media.First().MediaType == "photo")
                return "photo";
            if (tweet.Media.First().MediaType == "video")
                return "video";
            if (tweet.Media.First().MediaType == "animated_gif")
                return "gif";
            return string.Empty;
        }
    }
}
