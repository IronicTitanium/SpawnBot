﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SBPluginInterface;
using Twitterizer;
using xLogger;

namespace SBTwitter
{
    public class Main : SBPlugin
    {
        private SBPluginHost Host;

        #region SBPlugin Members

        public string PluginName
        {
            get
            {
                return "Twitter Message Parser";
            }
        }

        public string Version
        {
            get
            {
                return "1.0";
            }
        }

        public string Author
        {
            get
            {
                return "Michael Schwarz";
            }
        }

        public string Website
        {
            get
            {
                return "";
            }
        }

        public string Description
        {
            get
            {
                return "Parses links to Twitter statuses and verboses message to channel";
            }
        }

        public SBPluginHost PluginHost
        {
            set
            {
                Host = value;

                Host.eventPluginChannelMessageReceived += new ChannelMessage(Host_eventPluginChannelMessageReceived);
            }
        }

        public void Dispose()
        {
            return;
        }

        #endregion

        void Host_eventPluginChannelMessageReceived( string name, string message, string channel )
        {
            if ( message.Contains("twitter.com") && ( message.Contains("/status/") || message.Contains("/statuses/") ) )
            {
                string[] Parameters = message.Split(' ');
                foreach ( string p in Parameters )
                {
                    if ( p.Contains("twitter.com") && ( p.Contains("/status/") || p.Contains("/statuses/") ) )
                    {
                        try
                        {
                            string parse = p.Replace("/statuses/", "/status/");

                            parse = parse.Substring(parse.IndexOf("/status/"));
                            parse = parse.Replace("/status/", "");

                            string text = PollTwitterStatus(Convert.ToDecimal(parse));

                            if ( text == null )
                            {
                                return;
                            }
                            else
                            {
                                Host.PluginResponse(channel, text);
                                break;
                            }
                        }
                        catch ( Exception e )
                        {
                            Logger.WriteLine(String.Format("***** Twitter error. Parsing: {0} ({1})", p, e.Message), ConsoleColor.DarkRed);
                            return;
                        }
                    }
                }
            }
        }

        private string PollTwitterStatus( decimal StatusId )
        {
            OAuthTokens t = new OAuthTokens();
            t.AccessToken = Host.PluginTwitterAccessToken;
            t.AccessTokenSecret = Host.PluginTwitterAccessTokenSecret;
            t.ConsumerKey = Host.PluginTwitterConsumerKey;
            t.ConsumerSecret = Host.PluginTwitterConsumerKeySecret;

            TwitterResponse<TwitterStatus> status = TwitterStatus.Show(t, StatusId);

            if ( status.Result == RequestResult.Success )
            {
                return String.Format("@{0} posted: {1}", status.ResponseObject.User.ScreenName, status.ResponseObject.Text);
            }
            else
            {
                return null;
            }
        }
    }
}
