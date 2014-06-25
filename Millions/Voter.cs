using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace Millions
{
    public class VoterSettings
    {
        public string ProxyAddress { get; set; }
        public string ProxyPort { get; set; }

        public bool HasProxy
        {
            get
            {
                return !string.IsNullOrWhiteSpace(ProxyAddress);
            }
        }
    }

    public class VoterResult
    {
        private readonly List<string> _logs;
        public IReadOnlyCollection<string> Logs
        {
            get
            {
                return _logs;
            }
        }

        public bool Ok { get; internal set; }

        public string VoteResponse { get; internal set; }


        internal VoterResult()
        {
            _logs = new List<string>();
        }

        internal void Log(string message)
        {
            _logs.Add(message);
        }

        internal void LogFormat(string format, params string[] args)
        {
            Log(string.Format(format, args));
        }
    }

    public class Voter
    {
        public static VoterResult VoteOnce(VoterSettings settings)
        {
            var voter = new Voter(settings);
            return voter.Vote();
        }

        private readonly VoterSettings _settings;
        private readonly VoterResult _result;
        private readonly CookieContainer _cookieContainer;
        private const string START_URL = "http://www.30millionsdamis.fr/la-fondation/nos-campagnes/oui-a-la-fidelite/participer.html";
        private const string VOTE_URL = "http://www.30millionsdamis.fr/la-fondation/nos-campagnes/oui-a-la-fidelite/participer.php";

        private Voter(VoterSettings settings)
        {
            _settings = settings ?? new VoterSettings();
            _cookieContainer = new CookieContainer();
            _result = new VoterResult();
        }

        private VoterResult Vote()
        {
            try
            {
                _result.Log("###------------------------------");
                _result.Log("Starting navigation...");
                if (_settings.HasProxy)
                {
                    _result.LogFormat("Using proxy {0}:{1}", _settings.ProxyAddress, _settings.ProxyPort);
                }

                LaunchStartRequest();
                LaunchVoteRequest();
                _result.Log("Vote done successfully");
                _result.Ok = true;
            }
            catch (Exception ex)
            {
                _result.Log("Error while voting");
                _result.Log(ex.Message);
            }
            _result.Log("------------------------------###");
            return _result;
        }

        private void LaunchStartRequest()
        {
            _result.LogFormat("Start request {0}", START_URL);
            var request = (HttpWebRequest)HttpWebRequest.Create(START_URL);
            SetProxy(request);
            request.CookieContainer = _cookieContainer;
            request.Method = "GET";
            SetCommonHeaders(request);

            _result.Log("Getting response...");
            var response = request.GetResponse();
            using (var reader = new StreamReader(response.GetResponseStream()))
            {
                var content = reader.ReadToEnd();
                _result.Log("Response content:");
                _result.Log(content.Length > 100 ? content.Substring(0, 100) : content);
                _result.Log("------------------------------");
            }
            _result.LogFormat("End request {0}", START_URL);
        }

        private void LaunchVoteRequest()
        {
            _result.LogFormat("Start request {0}", VOTE_URL);
            var request = (HttpWebRequest)HttpWebRequest.Create(VOTE_URL);
            SetProxy(request);
            request.CookieContainer = _cookieContainer;
            request.Method = "POST";
            SetCommonHeaders(request);
            //
            var postData = "act=vote&uid=1465";
            var data = Encoding.ASCII.GetBytes(postData);
            request.Accept = "application/json, text/javascript, */*; q=0.01";
            request.Headers.Add("Origin", "http://www.30millionsdamis.fr");
            request.Headers.Add("X-Requested-With", "XMLHttpRequest");
            request.Referer = "http://www.30millionsdamis.fr/la-fondation/nos-campagnes/oui-a-la-fidelite/participer.html";
            request.ContentLength = data.Length;
            request.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";

            _result.Log("Getting request stream...");
            using (var requestStream = request.GetRequestStream())
            {
                _result.Log("Writing to request stream...");
                requestStream.Write(data, 0, data.Length);
            }

            _result.Log("Getting response...");
            var response = request.GetResponse();
            using (var reader = new StreamReader(response.GetResponseStream()))
            {
                var content = reader.ReadToEnd();
                _result.Log("Response content:");
                _result.Log(content.Length > 100 ? content.Substring(0, 100) : content);
                _result.Log("------------------------------");
                _result.VoteResponse = content;
            }
            _result.LogFormat("End request {0}", VOTE_URL);
        }

        private void SetCommonHeaders(HttpWebRequest request)
        {
            request.KeepAlive = true;
            request.AutomaticDecompression = DecompressionMethods.GZip;
            request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8";
            request.UserAgent = "Mozilla/5.0 (Windows NT 6.2; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/35.0.1916.153 Safari/537.36";
            request.Headers.Add(HttpRequestHeader.AcceptLanguage, "en-US,en;q=0.8,fr;q=0.6,es;q=0.4,zh-CN;q=0.2,zh;q=0.2,de;q=0.2");
            request.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip,deflate,sdch");
        }

        private void SetProxy(HttpWebRequest request)
        {
            if (!_settings.HasProxy)
            {
                _result.Log("No proxy");
                return;
            }

            var proxyAddress = string.Format("{0}{1}", _settings.ProxyAddress, string.IsNullOrEmpty(_settings.ProxyPort) ? "" : ":" + _settings.ProxyPort);
            _result.LogFormat("Setting proxy: {0}", proxyAddress);
            var proxy = new WebProxy();
            proxy.Address = new Uri(proxyAddress);
            request.Proxy = proxy;
        }
    }
}
