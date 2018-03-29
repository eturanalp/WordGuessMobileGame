using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using System.Net.Http;
using Newtonsoft.Json;
using System.IO;
using System.Net;
using System.Runtime.Serialization;
using Newtonsoft.Json.Serialization;
/* 
 *  THIS IS A WORD GUESSSING GAME where the player tries to find the secret word. 
 *  These clues are given to the user:
 *  1. The number of letters in the secret word
 *  2. Up to 5 associated words (mostly words with same or similar meaning)
 *  3. Example sentence where the secret word is a part of.
 *  4. Other clues
 * */

/*    ---- TODO LIST ---
    1. Get all clues including partOfSpeech, hasTypes, typeOf, etc..
        Check: https://wordsapiv1.p.mashape.com/words/

    */

//https://www.twinword.com/api/word-associations.php
//https://market.mashape.com/wordsapi/wordsapi#search
//http://developer.wordnik.com/docs.html#!/words/getRandomWord_get_4
// https://wordsapiv1.p.mashape.com/words/{word}/example
//
// https://developer.xamarin.com/guides/xamarin-forms/cloud-services/consuming/rest/
// https://developer.xamarin.com/recipes/android/web_services/consuming_services/call_a_rest_web_service/
// C:\Users\Emin>powershell -command "& { (New-Object Net.WebClient).DownloadFile('https://gist.githubusercontent.com/h3xx/1976236/raw/bbabb412261386673eff521dddbe1dc815373b1d/wiki-100k.txt', 'c:\temp\somefile.txt') }"
// https://developer.xamarin.com/guides/android/deployment,_testing,_and_metrics/release-prep/

namespace App5
{
    public class RW  // random word class to hold the result of web service call
    {
        public string word { get; set; }
    }
    public class AA  //from mashape.com
    {
        public string entry { get; set; }
        public List<string> assoc_word { get; set; }
        public List<string> assoc_word_ex { get; set; }
        public string result_msg { get; set; }
        [OnError]
        internal void OnError(StreamingContext context, ErrorContext errorContext)
        {
            errorContext.Handled = true;
            //string ss=errorContext.Error.Message;
        }
    }

    public class AAWS
    {
        public string entry { get; set; }
        public Dictionary<string, double> associations_scored { get; set; }

    }

    public class EE
    {
        public string word { get; set; }
        public List<string> examples { get; set; }
    }

    public partial class MainPage : ContentPage
    {

        public enum gamestate { start, end, ingame };
        public gamestate gstate=gamestate.start;
        public HttpClient client;
        HttpClient client1;  // https://forums.xamarin.com/discussion/83079/when-will-monos-tls-1-2-be-merged-into-xamarin-android
                             // GetAsync System.Net.WebException: Error: SecureChannelFailure (The authentication or decryption has failed.) 
        public int windex=0;
        //AA jsonA= null;
        RW json = null;
        EE jsonE = null;
        AAWS jsonAAWS=null;
        string url = ""; string url2 = "";
        string[] aws;
        int letter_counter = 0;

        public MainPage()
        {
            InitializeComponent();
            client = new HttpClient();
            client.DefaultRequestHeaders.Add("X-Mashape-Key", "6ITLSlrg1smsh6ghYbKlJLpSh3Ndp1xL4EZjsnTea4iomp");
            client.DefaultRequestHeaders.Add("X-Twaip-Key", "3DwSdiTO4NkBUzKVP78Be8C4qFGWcyVUBXvDZRyglnPj2aJEAf9Mm2yHRZnXgjl2a+fqGWLFLLUquggsksi0");
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client1 = new HttpClient(new Xamarin.Android.Net.AndroidClientHandler());  // because default handler is not working while debugging
            client1.DefaultRequestHeaders.Add("X-Twaip-Key", "3DwSdiTO4NkBUzKVP78Be8C4qFGWcyVUBXvDZRyglnPj2aJEAf9Mm2yHRZnXgjl2a+fqGWLFLLUquggsksi0");
            client1.DefaultRequestHeaders.Add("Accept", "application/json");
        }

        private async Task<RW> FetchWordAsync(HttpClient client, String uri)
        {
            RW Itemss=null;
            var response = await client.GetAsync(uri);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                Itemss = JsonConvert.DeserializeObject<RW>(content);
            }
            return Itemss;

        }
        public string underline(string s)
        {
            string ss = "";
            for (int i=0; i<s.Length; i++) ss=ss+"_ ";
            return ss;
        }
        private async Task<AA> FetchWordAAsync(HttpClient client, String uri)
        {
            AA Itemss = null;
            var response = await client.GetAsync(uri);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    Itemss = JsonConvert.DeserializeObject<AA>(content);
                }
            return Itemss;

        }

        private async Task<AAWS> FetchWordAWSAsync(HttpClient client, String uri)
        {
            AAWS Itemss = null;
            var response = await client.GetAsync(uri);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                Itemss = JsonConvert.DeserializeObject<AAWS>(content);
            }
            return Itemss;

        }

        private async Task<EE> FetchWordExamplesAsync(HttpClient client, String uri)
        {
            EE Itemss = null;
            var response = await client.GetAsync(uri);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                Itemss = JsonConvert.DeserializeObject<EE>(content);
            }
            return Itemss;

        }

        private async Task<string> WriteSetAsync(HttpClient client, String uri)
        {   // not working probably because of dataP serialized object format
            var dataP = new
            {
                name = "Foo",
                //category = "article"
            };
            var postBodyDataP = JsonConvert.SerializeObject(dataP);
            var response = await client.PostAsync(uri, new StringContent(postBodyDataP, Encoding.UTF8, "application/json"));
            //this.DisplayAlert("LOG RESULT", response.Content, "Yes", "No"); 
            if (response.IsSuccessStatusCode)
            {
                return "Success";
            }
            else return "Fail";

        }

        public string WriteSetAsync2(string uri, string log)
        {
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(uri);
            req.Method = "POST";
            req.ContentType = "application/json";
            Stream stream = req.GetRequestStream();
            string json = "{\"name\": \"" + log + "\" }";
            byte[] buffer = Encoding.UTF8.GetBytes(json);
            stream.Write(buffer, 0, buffer.Length);
            HttpWebResponse res = (HttpWebResponse)req.GetResponse();
            return res.StatusCode.ToString();
        }

        public string blankTheWord(string s, string word)
        {
            string blank="";
            for (int i = 0; i < word.Length; i++) blank += "_";
            string si= s.Replace(word, blank);
            si=char.ToUpper(si[0]) + si.Substring(1); //First letter of the sentence is capitalized
            return si;
        }
        public string[] orderAWs(Dictionary<string, double> aws)
        {
            int le;
            if (aws.Count() < 5) le = aws.Count();
            else le = 5;
            string[] s=new string[le];
            for (int i = 0; (i < le); i++) s[i] = aws.ElementAt(i).Key;
            if (aws.Count()>9)
            {
                //s[4] = aws.ElementAt(10).Key;
                for (int i = le; i > 0; i--) s[i-1] = aws.ElementAt(((aws.Count()/2)*(le-i)/le)).Key;
            }
            return s;

        }
        string set_letter_clue()
        { //returns the clue string based on json.word, and letter_counter and Guess_label.Text
            StringBuilder next = new StringBuilder(Guess_label.Text); 
            if (letter_counter < json.word.Length)
                  next[letter_counter * 2] = json.word.ElementAt(letter_counter);
            return next.ToString();
        }
        private async void OnNewGameWord(object sender, EventArgs e)
        {
            try  // exception thrown when jsonA.assoc_word.Count is zero or the Deserialize gives error
            {
                if (gstate == gamestate.end)
                {
                    string sfrS = sfr.Value.ToString();  //word set rating in the Slider control
                    string log = json.word + "," + H1.Text + "," + H2.Text + "," + H3.Text + "," + H4.Text + "," + H5.Text + "," + sfrS;
                    WriteSetAsync2("https://xxxx.azurewebsites.net/api/HttpPOST-CRUD-guessSetRating", log);
                    H1.Text = H2.Text = H3.Text = H4.Text = H5.Text= "- - -";
                    Guess_label.Text = "**s*e*c*r*e*t  w*o*r*d**";
                    HCount.Text = "";
                    ExampleS1.Text = "<Example Sentence>";
                    ExampleS2.Text = "<Example Sentence>";
                    windex = 0;
                    gstate = gamestate.start;
                    phoneNumberText.Text = "";
                    sfr.Value = 0.22;
                    letter_counter = 0;
                }
                if (gstate == gamestate.start)
                {
                    //String url = "https://wordsapiv1.p.mashape.com/words/?random=true&frequencymin=7.99";
                    url = "http://api.wordnik.com:80/v4/words.json/randomWord?hasDictionaryDef=false&minCorpusCount=20000&maxCorpusCount=-1&minDictionaryCount=5&maxDictionaryCount=-1&minLength=4&maxLength=-1&api_key=a2a73e7b926c924fad7001ca3111acd55af2ffabf50eb";
                    json = (RW)await FetchWordAsync(client, url);
                    Guess_label.Text = underline(json.word);
                    phoneNumberText.Text = "<Type your guess for the secret word here>";
                    //url = "https://twinword-word-graph-dictionary.p.mashape.com/association/?entry=" + json.word;
                    url = "https://api.twinword.com/api/v4/word/associations/?entry=" + json.word;
                    url2 = "https://wordsapiv1.p.mashape.com/words/" + json.word + "/examples";
                    try
                    {
                        jsonAAWS = (AAWS)await FetchWordAWSAsync(client1, url);
                        jsonE = (EE)await FetchWordExamplesAsync(client, url2);
                    }
                    catch (Exception ex)
                    {
                        jsonAAWS = null; jsonE = null;
                        gstate = gamestate.end;
                        System.Diagnostics.Debug.WriteLine(ex);
                        //this.DisplayAlert("EXCEPTION", "Exception thrown for " + json.word + jsonE.ToString(), "Yes", "No");
                    }
                    if (jsonAAWS == null) gstate = gamestate.end;
                    else if (jsonAAWS.associations_scored.Count > 0)
                    {
                        gstate = gamestate.ingame;
                        HCount.Text = jsonAAWS.associations_scored.Count.ToString();
                        aws = orderAWs(jsonAAWS.associations_scored);
                    }
                    else gstate = gamestate.end;
                }
                if (gstate == gamestate.ingame)
                {
                    if ((windex < jsonAAWS.associations_scored.Count) && (windex < 5))
                    {
                        if (windex == 0) { H1.Text = aws[windex]; }
                        else if (windex == 1) { H2.Text = aws[windex]; phoneNumberText.Text = ""; }
                        else if (windex == 2) { H3.Text = aws[windex]; }
                        else if (windex == 3) { H4.Text = aws[windex]; }
                        else if (windex == 4) { H5.Text = aws[windex]; }
                        windex++;
                    }
                    else if (((windex == jsonAAWS.associations_scored.Count) || (windex == 5)) && (jsonE != null) && (jsonE.examples != null) && (jsonE.examples.Count > 0))
                    {
                        ExampleS1.Text = blankTheWord(jsonE.examples.First(), json.word) + ".";
                        windex++;
                    }
                    else if (((windex == (jsonAAWS.associations_scored.Count)+1) || (windex == 6)) && (jsonE != null) && (jsonE.examples != null) && (jsonE.examples.Count > 1))
                    {
                        ExampleS2.Text = blankTheWord(jsonE.examples[1], json.word) + ".";
                        windex++;
                    }
                    else if (letter_counter< json.word.Length){
                        Guess_label.Text = set_letter_clue(); //based on json.word and letter_counter
                        letter_counter++;
                    }
                    else
                    {
                        gstate = gamestate.end;
                        Guess_label.Text = json.word;
                    }
                }
            } catch (Exception ex) {
                //this.DisplayAlert("EXCEPTION", "Exception thrown for " + json.word + jsonE.ToString(), "Yes", "No"); 
                Console.WriteLine("\nMessage ---\n{0}", ex.Message);
                System.Diagnostics.Debug.WriteLine(ex);
            }
        }

        void OnCheckGuess(object sender, EventArgs e)
        {
            if (String.Equals(json.word, phoneNumberText.Text, StringComparison.OrdinalIgnoreCase)) 
            {
                this.DisplayAlert("CONGRATUTATIONS", "You guessed it right. The word was " + json.word, "Yes", "No");
                Guess_label.Text = json.word.ToUpper();
                letter_counter = json.word.Length;
            }
        }

        void OnSetLevel(object sender, EventArgs e)
        {
            var action = DisplayActionSheet("Select Level?", "Easy", "Medium", "Hard", "Email");
            Level.Text = "Set Level = " + action;
        }

        void OnGuess(object sender, EventArgs e)
        {
            //if (String.Equals(phoneNumberText.Text, "<Type your guess for the secret word here>", StringComparison.OrdinalIgnoreCase))
            //{
            //    phoneNumberText.Text = "";
            //    phoneNumberText.TextColor = Color.Gray;
            //}
        }
    }
}

/*
 * 
//using Android.App;
//using Android.Content.PM;
//using Android.Runtime;
//using Android.Views;
//using Android.Widget;
//using Android.OS;
 * 
 * */
//this.DisplayAlert( "RANDOM WORD","The word you are given: " + json.word, "Yes",  "No");
//json.word = "symmetry class";  // throws exceprion: returns array of associated words tith respective rates
//json.word = "trotski";  // throws exception: "{\"result_code\":\"462\",\"result_msg\":\"Entry word not found\"}"
//json.word = "sound";
//H1.Text = jsonA.result_msg;
////https://guesssetrating.azurewebsites.net/api/HttpPOST-CRUD-guessSetRating
////WriteSetAsync
//string url = "https://guesssetrating.azurewebsites.net/api/HttpPOST-CRUD-CSharp1?code=bQWPYtoMAzfAdHgxXLJysTOECkMXau54cDAuWuuPmc9pCpRihOpx";
//client.DefaultRequestHeaders.Add("Accept", "application/json");
//                string logresult = (string)await WriteSetAsync(client, url);
//                if (logresult=="Success")
//                    this.DisplayAlert("LOG RESULT", "Success!", "Yes", "No");
//System.Diagnostics.Debug.WriteLine("End of gamestate.end");
/*
 *        public string read_random_from_file()
        {
            //var assembly = Assembly.GetExecutingAssembly();
            //var resourceName = "Wictionary_top10Kwords_text.txt";
            //string word_file;
            //var auxList = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceNames();

            //using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            //using (StreamReader reader = new StreamReader(stream))
            //{
            //    word_file = reader.ReadToEnd();
            //}
            //string resource_data = "Footnote beyond again laws";
            //List<string> words = word_file.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries).ToList();
            //return words.ElementAt(500);
            //return auxList.First();
            //string[] dict = { "afadsds", "sgfsgf", "fadsfdsf" };
            var assembly = typeof(MainPage).GetTypeInfo().Assembly;
            foreach (var res in assembly.GetManifestResourceNames())
            {
                System.Diagnostics.Debug.WriteLine("found resource: " + res);
            }
            return "";
        }
 *             
            RatingBar ratingbar = FindViewById<RatingBar>(Resource.Id.ratingbar);

            ratingbar.RatingBarChange += (o, e) => {
                Toast.MakeText(this, "New Rating: " + ratingbar.Rating.ToString(), ToastLength.Short).Show();
            };
                   
    <RatingBar android:id="@+id/ratingbar"
        android:layout_width="wrap_content"
        android:layout_height="wrap_content"
        android:numStars="5"
        android:stepSize="1.0"/>


                            //jsonA = (AA)await FetchWordAAsync(client, url);
                        //Task.Run(async () =>
                        //{
                        //    jsonAAWS = (AAWS)await FetchWordAWSAsync(client, url);
                        //    // Do any async anything you need here without worry
                        //}).GetAwaiter().GetResult();
https://forums.xamarin.com/discussion/10405/the-authentication-or-decryption-has-failed-in-the-web-request

 * */
