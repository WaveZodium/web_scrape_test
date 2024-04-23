using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HtmlAgilityPack;

class Program {
    static async Task Main(string[] args) {
        string baseUrl = "http://books.toscrape.com"; // Change this to the desired website URL
        //string baseUrl = "https://wavezodium.dev";
        //string baseUrl = "https://wavey.se";
        await DownloadWebsite(baseUrl);
    }

    #nullable enable
	public static string CreateValidFilePath(string url) {
		// Define the regular expression pattern to match the URL components.
        string pattern = @"^(https?://)?([^/]+)(/.*)?$";

        // Match the URL against the pattern.
        Match match = Regex.Match(url, pattern);

        // Extract the FQDN and subdirectories.
        if (match.Success) {
            string fqdn = match.Groups[2].Value;
            string subdirectories = match.Groups[3].Value.TrimStart('/');

			// Find the index of "?"
			int idx_query_string_start = subdirectories.IndexOf("?");

			// Remove potential query string(s)
			subdirectories = idx_query_string_start >= 0 ? subdirectories.Substring(0, idx_query_string_start) : subdirectories;

			// Define a list of characters not allowed in filenames or folder names
			char[] invalidCharacters = { '/', '\\', ':', '*', '?', '"', '<', '>', '|' };
			
            // TODO: Implement Path.GetInvalidPathChars() instead of manual regex replace list.
			// Replace invalid filename characters.
        	subdirectories = Regex.Replace(subdirectories, @"[\\:*?""<>|]", "_");

			// Split the URL by "/"
			string[] subdirectoriesParts = subdirectories.Split('/');

			// Join the string array with the FQDN to create a full filepath.
			subdirectories = Path.Combine(subdirectoriesParts);
			string fullPath = Path.Combine(fqdn, subdirectories);
			
			return fullPath;
        }
        else {
            Console.WriteLine("Invalid URL format.");
			return string.Empty;
        }
	}

    public static string? GetFqdn(string url) {
		// Define the regular expression pattern to match the URL components.
        string pattern = @"^(https?://)?([^/]+)(/.*)?$";

        // Match the URL against the pattern.
        Match match = Regex.Match(url, pattern);

        // Extract the FQDN and subdirectories.
        if (match.Success) {
            return match.Groups[2].Value;
        }
        else {
            Console.WriteLine("Invalid URL format.");
			return null;
        }
	}

    static async Task DownloadWebsite(string baseUrl) {
        var httpClient = new HttpClient();
        var visitedUrls = new HashSet<string>();
        var queue = new Queue<string>();
        queue.Enqueue(baseUrl);
int max_loop = 10;
int loop_no = 0;
        while (queue.Count > 0 && loop_no < max_loop) {
            string url = queue.Dequeue();
            if (!visitedUrls.Contains(url)) {
                visitedUrls.Add(url);
                Console.WriteLine("Visiting: " + url);

                try {
                    HttpResponseMessage response = await httpClient.GetAsync(url);
                    response.EnsureSuccessStatusCode();
                    string htmlContent = await response.Content.ReadAsStringAsync();

                    // Save the HTML content
                    await SaveHtml(url, htmlContent);

                    var htmlDocument = new HtmlDocument();
                    htmlDocument.LoadHtml(htmlContent);

                    // Extract links from the HTML
                    var links = htmlDocument.DocumentNode.SelectNodes("//a[@href]");
                    if (links != null) {
                        foreach (var link in links) {
                            string href = link.GetAttributeValue("href", "");
                            string absoluteUrl = MakeAbsoluteUrl(url, href);
                            if (IsInDomain(absoluteUrl, baseUrl)) {
                                queue.Enqueue(absoluteUrl);
                            }
                        }
                    }

                    // Download resources (images, stylesheets, scripts, etc.)
                    await DownloadResources(htmlDocument, url);
                }
                catch (Exception ex) {
                    Console.WriteLine($"Failed to download {url}: {ex.Message}");
                }
            }

loop_no++;
        }
    }

    static async Task SaveHtml(string url, string htmlContent) {
        string fileName1 = Path.GetFileName(url);
        if (fileName1 != GetFqdn(url)) {
            try {
                string directoryUrl = url.Substring(0, url.LastIndexOf('/') + 1);
                //Console.WriteLine("[0] directoryUrl: " + directoryUrl);

                string fullPath = Path.Combine("downloaded_sites", CreateValidFilePath(directoryUrl));
                //Console.WriteLine("[1] fullPath: " + fullPath);

                //Console.WriteLine("[2] fileName1: " + fileName1);

                //Console.WriteLine("[3] Is this directory valid?: " + directoryUrl);
                //Console.WriteLine("[4] Is this path valid?: " + fullPath);

                string filePath1 = Path.Combine(fullPath, fileName1);
                //Console.WriteLine("[5] filePath1: " + filePath1);
                
                Console.WriteLine("Creating " + fullPath + " ...");
                Directory.CreateDirectory(fullPath);

                Console.WriteLine("Saving " + filePath1 + " ...");
                await File.WriteAllTextAsync(filePath1, htmlContent);

            }
            catch (Exception ex) {
                Console.WriteLine($"Failed to download {url}: {ex.Message}");
            }
        }
    }

    static async Task DownloadResources(HtmlDocument htmlDocument, string baseUrl) {
        Console.WriteLine("Base URL: " + baseUrl);
        var httpClient = new HttpClient();
        var resources = new HashSet<string>();

        // Extract resource URLs (images, stylesheets, scripts, etc.)
        //var imageUrls = htmlDocument.DocumentNode.SelectNodes("//img[@src]");
        //var styleUrls = htmlDocument.DocumentNode.SelectNodes("//link[@rel='stylesheet']");
        //var scriptUrls = htmlDocument.DocumentNode.SelectNodes("//script[@src]");

        /*if (imageUrls != null) {
            foreach (var imageUrl in imageUrls) {
                string url = MakeAbsoluteUrl(baseUrl, imageUrl.GetAttributeValue("src", ""));
                resources.Add(url);
                Console.WriteLine("Image url: " + url);
            }
        }*/

        /*if (styleUrls != null) {
            foreach (var styleUrl in styleUrls) {
                string url = MakeAbsoluteUrl(baseUrl, styleUrl.GetAttributeValue("href", ""));
                resources.Add(url);
                Console.WriteLine("Style url: " + url);
            }
        }*/

        /*if (scriptUrls != null) {
            foreach (var scriptUrl in scriptUrls) {
                string url = MakeAbsoluteUrl(baseUrl, scriptUrl.GetAttributeValue("src", ""));
                resources.Add(url);
                Console.WriteLine("Script url: " + url);
            }
        }*/

        // Download resources
        /*foreach (var resourceUrl in resources) {
            try {
                HttpResponseMessage response = await httpClient.GetAsync(resourceUrl);
                response.EnsureSuccessStatusCode();
                byte[] data = await response.Content.ReadAsByteArrayAsync();
                string fileName = Path.GetFileName(resourceUrl);
                string directory = Path.Combine("downloaded_sites", baseUrl.Substring(baseUrl.IndexOf("://") + 3).Split('/')[0]);
                string directory2 = Path.Combine("downloaded_sites", CreateValidFilePath(resourceUrl));
                Console.WriteLine("[0] Is this valid?: " + directory);
                Console.WriteLine("[0] resourceUrl: " + resourceUrl);
                Console.WriteLine("[1] Is this valid?: " + directory2);
                Directory.CreateDirectory(directory);
                string filePath = Path.Combine(directory, fileName);
                await File.WriteAllBytesAsync(filePath, data);
            }
            catch (Exception ex) {
                Console.WriteLine($"Failed to download {resourceUrl}: {ex.Message}");
            }
        }*/

    }

    static string MakeAbsoluteUrl(string baseUrl, string href) {
        Uri baseUri = new Uri(baseUrl);
        Uri absoluteUri = new Uri(baseUri, href);
        return absoluteUri.ToString();
    }

    static bool IsInDomain(string url, string baseUrl) {
        Uri baseUri = new Uri(baseUrl);
        Uri uri = new Uri(url);
        return uri.Host == baseUri.Host;
    }
}
