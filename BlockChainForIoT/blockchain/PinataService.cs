// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Text.Json;
// using System.Threading.Tasks;

// namespace BlockChainForIoT.blockchain
// {
//     public class PinataService
//     {
//         private static readonly HttpClient client = new HttpClient();
//         private string PinataApiKey; // Thay bằng API Key của bạn
//         private string PinataApiSecret; // Thay bằng API Secret của bạn
//         private string PinataJwt; // Thay bằng JWT nếu sử dụng
//         private string PinataBaseUrl = "https://api.pinata.cloud";
//         private const string MetadataCidFile = "metadata_cid.json";
//         private readonly HttpClient _httpClient;
//         public string MetadataCid { get; private set; }
//         public string LatestIpfsCid { get; private set; }
//         public Dictionary<int, List<string>> CropCodeToCids { get; private set; } = new Dictionary<int, List<string>>();
//         public Dictionary<int, List<string>> SensorIdToCids { get; private set; } = new Dictionary<int, List<string>>();
//         public Dictionary<int, List<string>> ActionIdToCids { get; private set; } = new Dictionary<int, List<string>>();
//         public PinataService(IConfiguration config, HttpClient httpClient)
//         {
//             PinataApiKey = config["pinata:pinata_key"];
//             PinataApiSecret = config["pinata:pinata_secret"];
//             PinataJwt = config["pinata:pinata_jwt"];
//             Console.WriteLine($"Pinata JWT: {PinataJwt}");
//             _httpClient = httpClient;
//         }

//         public async Task<List<string>> GetFileInfo(string cid)
//         {
//             try
//             {
//                 //add header to request
//                 _httpClient.DefaultRequestHeaders.Clear();
//                 _httpClient.DefaultRequestHeaders.Add("pinata_api_key", PinataApiKey);
//                 _httpClient.DefaultRequestHeaders.Add("pinata_secret_api_key", PinataApiSecret);

//                 // Gửi yêu cầu GET tới endpoint pinList với tham số cid
//                 string url = $"{PinataBaseUrl}/data/pinList?hashContains={cid}";
//                 var response = await client.GetAsync(url);
//                 response.EnsureSuccessStatusCode();

//                 // Đọc phản hồi
//                 string result = await response.Content.ReadAsStringAsync();
//                 Console.WriteLine("File info: " + result);
//                 List<string> stringList = JsonSerializer.Deserialize<List<string>>(result);
//                 return stringList;
//             }
//             catch (Exception ex)
//             {
//                 Console.WriteLine("Error: " + ex.Message);
//                 throw ex;
//             }
//         }

//         //Ghi file lên pinata
//         public async Task<string> UploadFileToPinata(string filePath)
//         {
//             try
//             {
//                 _httpClient.DefaultRequestHeaders.Clear();
//                 _httpClient.DefaultRequestHeaders.Add("pinata_api_key", PinataApiKey);
//                 _httpClient.DefaultRequestHeaders.Add("pinata_secret_api_key", PinataApiSecret);

//                 // Tạo multipart form data
//                 using var content = new MultipartFormDataContent();
//                 var fileContent = new ByteArrayContent(File.ReadAllBytes(filePath));
//                 fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");
//                 content.Add(fileContent, "file", Path.GetFileName(filePath));

//                 // Gửi yêu cầu POST
//                 var response = await client.PostAsync($"{PinataBaseUrl}/pinning/pinFileToIPFS", content);
//                 response.EnsureSuccessStatusCode();

//                 // Đọc phản hồi và lấy CID
//                 string result = await response.Content.ReadAsStringAsync();
//                 Console.WriteLine("Upload successful! Response: " + result);
//             }
//             catch (Exception ex)
//             {

//             }
//         }
//     }
// }