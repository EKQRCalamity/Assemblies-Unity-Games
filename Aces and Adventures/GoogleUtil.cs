using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.Configuration.Attributes;
using Google.Api.Gax.ResourceNames;
using Google.Apis.Admin.Directory.directory_v1;
using Google.Apis.Admin.Directory.directory_v1.Data;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using Google.Apis.DriveActivity.v2;
using Google.Apis.DriveActivity.v2.Data;
using Google.Apis.FirebaseDynamicLinks.v1;
using Google.Apis.FirebaseDynamicLinks.v1.Data;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Google.Apis.Groupssettings.v1;
using Google.Apis.Groupssettings.v1.Data;
using Google.Apis.Script.v1;
using Google.Apis.Script.v1.Data;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Storage.v1.Data;
using Google.Apis.Util.Store;
using Google.Cloud.Storage.V1;
using Google.Cloud.Translate.V3;
using Google.LongRunning;
using Google.Protobuf;
using Microsoft.IdentityModel.Tokens;
using MimeKit;
using MimeKit.Text;
using MimeTypes;
using Rebrandly;
using Rebrandly.Models;
using UnityEngine;

public static class GoogleUtil
{
	public static class Auth
	{
		public const string FILE_STORE_PATH = "Library/Google";

		public const string USER_NAME = "user";

		private static UserCredential _GetOAuthCredential<T>(string clientId, string clientSecret, string[] scopes, string keySalt = null) where T : IClientService
		{
			scopes = scopes.OrderBy((string s) => s).ToArray();
			string text = Hash128.Compute("OAuth/" + typeof(T).Name + "/" + clientId + ((keySalt != null) ? ("/" + keySalt) : "")).ToString();
			string text2 = Hash128.Compute(scopes.Aggregate("", (string a, string b) => a + b)).ToString();
			if (Directory.Exists("Library/Google"))
			{
				DirectoryInfo[] directories = new DirectoryInfo("Library/Google").GetDirectories();
				foreach (DirectoryInfo directoryInfo in directories)
				{
					if (directoryInfo.Name.Contains(text) && !directoryInfo.Name.Contains(text2))
					{
						directoryInfo.Delete(recursive: true);
					}
				}
			}
			FileDataStore fileDataStore = new FileDataStore("Library/Google/" + text + "-" + text2, fullPath: true);
			try
			{
				return GoogleWebAuthorizationBroker.AuthorizeAsync(new ClientSecrets
				{
					ClientId = clientId,
					ClientSecret = clientSecret
				}, scopes, "user", CancellationToken.None, fileDataStore).Result;
			}
			catch (Exception message)
			{
				Debug.Log(message);
				fileDataStore.ClearAsync().RunSynchronously();
				return GoogleWebAuthorizationBroker.AuthorizeAsync(new ClientSecrets
				{
					ClientId = clientId,
					ClientSecret = clientSecret
				}, scopes, "user", CancellationToken.None, fileDataStore).Result;
			}
		}

		public static UserCredential GetDefaultOAuthCredential<T>(params string[] scopes) where T : IClientService
		{
			return _GetOAuthCredential<T>(CLIENT_ID, CLIENT_SECRET, scopes);
		}

		public static UserCredential GetDefaultOAuthCredentialSalt<T>(string keySalt, params string[] scopes) where T : IClientService
		{
			return _GetOAuthCredential<T>(CLIENT_ID, CLIENT_SECRET, scopes, keySalt);
		}
	}

	public static class Translate
	{
		private static readonly string GLOSSARY_PATH = "glossaries/";

		private static readonly string FULL_GLOSSARY_PATH = "gs://aces-and-adventures/" + GLOSSARY_PATH;

		public const string NEW_LINE_REPLACE = "<br>";

		private static TranslationServiceClient _Client;

		public static TranslationServiceClient Client => _Client ?? (_Client = new TranslationServiceClientBuilder
		{
			JsonCredentials = CREDENTIALS
		}.Build());

		public static string ToGlossaryName(string name)
		{
			StringBuilder stringBuilder = new StringBuilder(name.Length);
			foreach (char c in name)
			{
				if (char.IsLetter(c))
				{
					stringBuilder.Append(char.ToLower(c));
				}
				else if (char.IsDigit(c))
				{
					stringBuilder.Append(c);
				}
				else if (char.IsWhiteSpace(c))
				{
					stringBuilder.Append('-');
				}
			}
			return stringBuilder.ToString();
		}

		public static string ToGlossaryPath(string glossaryName)
		{
			return GLOSSARY_PATH + glossaryName + ".csv";
		}

		public static IEnumerable<Glossary> Glossaries()
		{
			return Client.ListGlossaries(new ListGlossariesRequest
			{
				ParentAsLocationName = LOCATION_NAME
			});
		}

		public static Glossary NewGlossary(string glossaryName, string targetLanguageCode, string sourceLanguageCode = "en")
		{
			return new Glossary
			{
				GlossaryName = new GlossaryName("aces-and-adventures", "us-central1", glossaryName),
				InputConfig = new GlossaryInputConfig
				{
					GcsSource = new GcsSource
					{
						InputUri = FULL_GLOSSARY_PATH + glossaryName + ".csv"
					}
				},
				LanguagePair = new Glossary.Types.LanguageCodePair
				{
					SourceLanguageCode = sourceLanguageCode,
					TargetLanguageCode = targetLanguageCode
				}
			};
		}

		public static async Task<Glossary> GetGlossaryAsync(Glossary glossary)
		{
			return await Client.GetGlossaryAsync(new GetGlossaryRequest
			{
				GlossaryName = glossary.GlossaryName,
				Name = glossary.Name
			});
		}

		public static async Task<Glossary> TryGetGlossaryAsync(Glossary glossary)
		{
			try
			{
				return await GetGlossaryAsync(glossary);
			}
			catch
			{
				return null;
			}
		}

		public static async Task<Google.Apis.Storage.v1.Data.Object> UploadGlossaryCsvAsync(string glossaryName, Stream csvStream)
		{
			return await Storage.UploadAsync(ToGlossaryPath(glossaryName), csvStream);
		}

		public static async Task<Glossary> CreateGlossaryAsync(Glossary glossary)
		{
			return await (await Client.CreateGlossaryAsync(new CreateGlossaryRequest
			{
				Glossary = glossary,
				Parent = PROJECT_NAME,
				ParentAsLocationName = LOCATION_NAME
			})).ResultAsync();
		}

		public static async Task<Glossary> UpdateGlossaryAsync(Glossary glossary)
		{
			Glossary existingGlossary = await TryGetGlossaryAsync(glossary);
			bool flag = existingGlossary != null;
			DateTime timeGlossaryInputFileWasLastModified = default(DateTime);
			if (flag)
			{
				DateTime? dateTime = await Storage.GetLastModifiedTimeUtcAsync(Storage.GetPathWithoutBucket(existingGlossary.InputConfig.GcsSource.InputUri));
				int num;
				if (dateTime.HasValue)
				{
					timeGlossaryInputFileWasLastModified = dateTime.GetValueOrDefault();
					num = 1;
				}
				else
				{
					num = 0;
				}
				flag = (byte)num != 0;
			}
			if (flag)
			{
				if (!(timeGlossaryInputFileWasLastModified > existingGlossary.SubmitTime.ToDateTime().ToUniversalTime()))
				{
					return existingGlossary;
				}
				await DeleteGlossaryAsync(existingGlossary);
			}
			return await CreateGlossaryAsync(glossary);
		}

		public static async Task<DeleteGlossaryResponse> DeleteGlossaryAsync(Glossary glossary)
		{
			return await (await Client.DeleteGlossaryAsync(new DeleteGlossaryRequest
			{
				GlossaryName = glossary.GlossaryName,
				Name = glossary.Name
			})).ResultAsync();
		}

		private static async IAsyncEnumerable<string> _TextsAsync(IEnumerable<string> text, string targetLanguageCode, Glossary glossary = null, bool glossaryIgnoreCase = false, string sourceLanguageCode = "en")
		{
			TranslateTextRequest translateTextRequest = new TranslateTextRequest
			{
				Contents = { text.Select(ReplaceNewLinesForInput) },
				SourceLanguageCode = sourceLanguageCode,
				TargetLanguageCode = targetLanguageCode,
				Parent = PROJECT_NAME,
				ParentAsLocationName = LOCATION_NAME
			};
			if (glossary != null)
			{
				translateTextRequest.GlossaryConfig = new TranslateTextGlossaryConfig
				{
					Glossary = glossary.GlossaryName.ToString(),
					IgnoreCase = glossaryIgnoreCase
				};
			}
			TranslateTextResponse translateTextResponse = await Client.TranslateTextAsync(translateTextRequest);
			foreach (Translation item in (glossary != null) ? translateTextResponse.GlossaryTranslations : translateTextResponse.Translations)
			{
				yield return ReplaceNewLinesForOutput(item.TranslatedText);
			}
		}

		public static async IAsyncEnumerable<string> TextsAsync(IEnumerable<string> text, string targetLanguageCode, Glossary glossary = null, bool glossaryIgnoreCase = false, string sourceLanguageCode = "en")
		{
			List<string> texts = new List<string>(text);
			int countMinusOne = texts.Count - 1;
			int num = 0;
			int count = 0;
			int length = 0;
			for (int x = 0; x < texts.Count; x++)
			{
				int num2 = count + 1;
				count = num2;
				length += texts[x].Length;
				if (count < 256 && length < 4096 && x != countMinusOne)
				{
					continue;
				}
				await foreach (string item in _TextsAsync(texts.GetRange(num, x - num + 1), targetLanguageCode, glossary, glossaryIgnoreCase, sourceLanguageCode))
				{
					yield return item;
				}
				num = x + 1;
			}
		}

		public static string ReplaceNewLinesForInput(string input)
		{
			return input.Replace("\n", "<br>");
		}

		public static string ReplaceNewLinesForOutput(string output)
		{
			return output.Replace("<br>", "\n");
		}
	}

	public static class Storage
	{
		public const string BUCKET = "aces-and-adventures";

		private static StorageClient _Client;

		public static StorageClient Client => _Client ?? (_Client = StorageClient.Create(GoogleCredential.FromJson(CREDENTIALS)));

		public static async Task<DateTime?> GetLastModifiedTimeUtcAsync(string path, string bucket = "aces-and-adventures")
		{
			Google.Apis.Storage.v1.Data.Object @object = await Client.GetObjectAsync(bucket, path);
			return (@object == null) ? null : (@object.Updated ?? @object.TimeCreated)?.ToUniversalTime();
		}

		public static string GetPathWithoutBucket(string path)
		{
			return path.Replace("gs://", "").Split(new char[1] { '/' }, 2)[1];
		}

		public static async Task<bool> FileExistsAsync(string path, string bucket = "aces-and-adventures")
		{
			try
			{
				return await Client.GetObjectAsync(bucket, path) != null;
			}
			catch
			{
				return false;
			}
		}

		public static async Task<Google.Apis.Storage.v1.Data.Object> UploadAsync(string path, Stream source, string bucket = "aces-and-adventures")
		{
			return await Client.UploadObjectAsync(bucket, path, MimeTypeMap.GetMimeType(Path.GetExtension(path)), source);
		}

		public static async Task<Stream> DownloadAsync(string path, Stream destination, string bucket = "aces-and-adventures")
		{
			await Client.DownloadObjectAsync(bucket, path, destination);
			return destination;
		}
	}

	public static class Drive
	{
		public static class Activity
		{
			private static readonly string[] SCOPES;

			private static DriveActivityService _Service;

			public static DriveActivityService Service => _Service ?? (_Service = new DriveActivityService(new BaseClientService.Initializer
			{
				ApplicationName = "Aces & Adventures",
				HttpClientInitializer = GoogleCredential.FromJson(CREDENTIALS).CreateScoped(SCOPES).CreateWithUser(SERVICE_ACCOUNT_EMAIL)
					.UnderlyingCredential
			}));

			static Activity()
			{
				SCOPES = new string[0];
			}

			public static IList<DriveActivity> RecentActivity(string fileId)
			{
				return Service.Activity.Query(new QueryDriveActivityRequest
				{
					ItemName = "items/" + fileId
				}).Execute().Activities;
			}
		}

		public enum PermissionType
		{
			user,
			group,
			domain,
			anyone
		}

		public enum PermissionRole
		{
			reader,
			commenter,
			writer,
			fileOrganizer,
			owner
		}

		private const string MIME_FOLDER = "application/vnd.google-apps.folder";

		private static readonly string[] SCOPES;

		private static DriveService _Service;

		private static DriveService _UserService;

		public static string DRIVE_OWNER_EMAIL => BlobData.GetBlob("Drive Email");

		public static DriveService Service => _Service ?? (_Service = new DriveService(new BaseClientService.Initializer
		{
			ApplicationName = "Aces & Adventures",
			HttpClientInitializer = GoogleCredential.FromJson(CREDENTIALS).CreateScoped(SCOPES).CreateWithUser(SERVICE_ACCOUNT_EMAIL)
				.UnderlyingCredential
		}));

		public static DriveService UserService => _UserService ?? (_UserService = GetService(Auth.GetDefaultOAuthCredential<DriveService>(SCOPES)));

		static Drive()
		{
			SCOPES = new string[0];
		}

		public static DriveService GetService(UserCredential userCredential)
		{
			return new DriveService(new BaseClientService.Initializer
			{
				ApplicationName = "Aces & Adventures",
				HttpClientInitializer = userCredential
			});
		}

		public static FileList ListFiles()
		{
			return Service.Files.List().Execute();
		}

		public static async Task<bool> FileIsValidAsync(string fileId, string forEmailAddress = null)
		{
			try
			{
				Google.Apis.Drive.v3.Data.File file = await Service.Files.Get(fileId).SetFields("permissions/emailAddress,trashed").ExecuteAsync();
				return file != null && file.Trashed != true && file.Permissions.Any((Google.Apis.Drive.v3.Data.Permission permission) => string.Equals(forEmailAddress ?? DRIVE_OWNER_EMAIL, permission.EmailAddress, StringComparison.OrdinalIgnoreCase));
			}
			catch
			{
				return false;
			}
		}

		public static DateTime? GetLastModifiedTimeUtc(string fileId)
		{
			return Service.Files.Get(fileId).SetFields("modifiedTime").Execute()?.ModifiedTime?.ToUniversalTime();
		}

		public static async Task<Google.Apis.Drive.v3.Data.File> MoveFileAsync(string fileId, string folderId, DriveService service = null)
		{
			if (service == null)
			{
				service = Service;
			}
			Google.Apis.Drive.v3.Data.File file = await service.Files.Get(fileId).SetFields("parents").ExecuteAsync();
			return (file == null) ? null : (await new FilesResource.UpdateRequest(service, new Google.Apis.Drive.v3.Data.File(), fileId)
			{
				AddParents = folderId,
				RemoveParents = file.Parents.ToCommaSeparatedList(),
				Fields = "id, parents"
			}.ExecuteAsync());
		}

		public static async Task<Google.Apis.Drive.v3.Data.File> GetFolderByNameAsync(string folderName, DriveService service = null)
		{
			return (await new FilesResource.ListRequest(service ?? Service)
			{
				Q = "mimeType='application/vnd.google-apps.folder' and name='" + folderName + "'"
			}.ExecuteAsync()).Files.FirstOrDefault();
		}

		public static Google.Apis.Drive.v3.Data.File CopyFile(string newFileName, string fileToCopyId)
		{
			return Service.Files.Copy(new Google.Apis.Drive.v3.Data.File
			{
				Name = newFileName
			}, fileToCopyId).Execute();
		}

		public static async Task<string> GetPermissionIdFromEmailAsync(string fileId, string email)
		{
			return (await Service.Files.Get(fileId).SetFields("permissions(emailAddress,id)").ExecuteAsync()).Permissions.FirstOrDefault((Google.Apis.Drive.v3.Data.Permission permission) => string.Equals(email, permission.EmailAddress, StringComparison.OrdinalIgnoreCase))?.Id;
		}

		public static async Task<Google.Apis.Drive.v3.Data.Permission> ShareWithEmail(string fileId, string email, PermissionRole role = PermissionRole.writer, PermissionType type = PermissionType.user, DriveService service = null)
		{
			PermissionsResource.CreateRequest createRequest = (service ?? Service).Permissions.Create(new Google.Apis.Drive.v3.Data.Permission
			{
				Type = EnumUtil.Name(type),
				Role = EnumUtil.Name(role),
				EmailAddress = email
			}, fileId);
			if (role == PermissionRole.owner)
			{
				createRequest.TransferOwnership = true;
			}
			return await createRequest.ExecuteAsync();
		}

		public static async Task<string> DeletePermissionForEmail(string fileId, string email)
		{
			_ = 1;
			try
			{
				PermissionsResource permissions = Service.Permissions;
				return await permissions.Delete(fileId, await GetPermissionIdFromEmailAsync(fileId, email)).ExecuteAsync();
			}
			catch
			{
				return null;
			}
		}

		public static async Task<string> GetPropertyValueAsync(string fileId, string propertyName)
		{
			try
			{
				Google.Apis.Drive.v3.Data.File file = await Service.Files.Get(fileId).SetFields("properties").ExecuteAsync();
				string value;
				return (file != null && file.Properties.TryGetValue(propertyName, out value)) ? value : null;
			}
			catch
			{
				return null;
			}
		}

		public static async Task<IDictionary<string, string>> GetPropertiesAsync(string fileId)
		{
			try
			{
				return (await Service.Files.Get(fileId).SetFields("properties").ExecuteAsync()).Properties;
			}
			catch (Exception message)
			{
				Debug.Log(message);
				return null;
			}
		}

		public static async Task<Google.Apis.Drive.v3.Data.File> SetPropertyValueAsync(string fileId, string propertyName, string propertyValue)
		{
			return await Service.Files.Update(new Google.Apis.Drive.v3.Data.File
			{
				Properties = new Dictionary<string, string> { { propertyName, propertyValue } }
			}, fileId).SetFields("properties").ExecuteAsync();
		}

		public static async Task<string> GetLink(string fileId)
		{
			return (await Service.Files.Get(fileId).SetFields("webViewLink").ExecuteAsync()).WebViewLink;
		}
	}

	public static class Sheets
	{
		public enum WrapStrategy
		{
			DEFAULT,
			WRAP,
			CLIP,
			OVERFLOW_CELL
		}

		private static readonly string[] SCOPES;

		private static SheetsService _Service;

		private static SheetsService _UserService;

		public static SheetsService Service => _Service ?? (_Service = new SheetsService(new BaseClientService.Initializer
		{
			ApplicationName = "Aces & Adventures",
			HttpClientInitializer = GoogleCredential.FromJson(CREDENTIALS).CreateScoped(SCOPES).CreateWithUser(SERVICE_ACCOUNT_EMAIL)
				.UnderlyingCredential
		}).SetTimeout(1800.0));

		public static SheetsService UserService => _UserService ?? (_UserService = GetService(Auth.GetDefaultOAuthCredential<SheetsService>(SCOPES)));

		static Sheets()
		{
			SCOPES = new string[0];
		}

		public static SheetsService GetService(UserCredential userCredential)
		{
			return new SheetsService(new BaseClientService.Initializer
			{
				ApplicationName = "Aces & Adventures",
				HttpClientInitializer = userCredential
			});
		}

		public static async Task<Spreadsheet> CreateSpreadsheetAsync(string spreadsheetTitle, string folderName)
		{
			Spreadsheet spreadsheet = await Service.Spreadsheets.Create(new Spreadsheet
			{
				Properties = new SpreadsheetProperties
				{
					Title = spreadsheetTitle
				}
			}).ExecuteAsync();
			string spreadsheetId = spreadsheet.SpreadsheetId;
			await Drive.MoveFileAsync(spreadsheetId, (await Drive.GetFolderByNameAsync(folderName)).Id);
			return spreadsheet;
		}

		public static async Task<Spreadsheet> CreateSpreadsheetAsDriveOwnerAsync(string spreadsheetTitle, string folderName)
		{
			Spreadsheet spreadsheet = await UserService.Spreadsheets.Create(new Spreadsheet
			{
				Properties = new SpreadsheetProperties
				{
					Title = spreadsheetTitle
				}
			}).ExecuteAsync();
			string spreadsheetId = spreadsheet.SpreadsheetId;
			await Drive.MoveFileAsync(spreadsheetId, (await Drive.GetFolderByNameAsync(folderName, Drive.UserService)).Id, Drive.UserService);
			await Drive.ShareWithEmail(spreadsheet.SpreadsheetId, SERVICE_ACCOUNT_EMAIL, Drive.PermissionRole.writer, Drive.PermissionType.user, Drive.UserService);
			return spreadsheet;
		}

		public static async Task<bool> SpreadsheetExistsAsync(string spreadsheetId)
		{
			return await Drive.FileIsValidAsync(spreadsheetId);
		}

		private static async Task<int?> _CreateSheetAsync(string spreadsheetId, string sheetTitle)
		{
			return (await BatchUpdateAsync(spreadsheetId, new Request
			{
				AddSheet = new AddSheetRequest
				{
					Properties = new SheetProperties
					{
						Title = sheetTitle
					}
				}
			}))[0].AddSheet.Properties.SheetId;
		}

		public static async Task<Sheet> GetOrCreateSheetAsync(string spreadsheetId, string sheetTitle)
		{
			foreach (Sheet item in await GetAllSheets(spreadsheetId))
			{
				if (item.Properties.Title.Equals(sheetTitle, StringComparison.OrdinalIgnoreCase))
				{
					return item;
				}
			}
			int? num = await _CreateSheetAsync(spreadsheetId, sheetTitle);
			Sheet result;
			if (num.HasValue)
			{
				int valueOrDefault = num.GetValueOrDefault();
				result = await GetSheetAsync(spreadsheetId, valueOrDefault);
			}
			else
			{
				result = null;
			}
			return result;
		}

		public static async Task<Sheet> GetSheetAsync(string spreadsheetId, int sheetId, string fields = null, bool includeGridData = false)
		{
			return (await Service.Spreadsheets.GetByDataFilter(new GetSpreadsheetByDataFilterRequest
			{
				DataFilters = new DataFilter[1]
				{
					new DataFilter
					{
						GridRange = new GridRange
						{
							SheetId = sheetId
						}
					}
				},
				IncludeGridData = includeGridData
			}, spreadsheetId).SetFields(fields).ExecuteAsync()).Sheets.FirstOrDefault();
		}

		public static async Task<Sheet> TryGetSheetAsync(string spreadsheetId, int sheetId, string fields = null)
		{
			try
			{
				return await GetSheetAsync(spreadsheetId, sheetId, fields);
			}
			catch
			{
				return null;
			}
		}

		public static async Task<bool> SheetExistsAsync(string spreadsheetId, int sheetId)
		{
			return await TryGetSheetAsync(spreadsheetId, sheetId, "sheets.properties.sheetId") != null;
		}

		public static async Task<IList<Sheet>> GetAllSheets(string spreadsheetId, string fields = null, bool includeGridData = false)
		{
			SpreadsheetsResource.GetRequest getRequest = Service.Spreadsheets.Get(spreadsheetId);
			getRequest.IncludeGridData = includeGridData;
			return (await getRequest.SetFields(fields).ExecuteAsync()).Sheets;
		}

		public static Request ClearSheetRequest(int sheetId)
		{
			return new Request
			{
				UpdateCells = new UpdateCellsRequest
				{
					Range = new GridRange
					{
						SheetId = sheetId
					},
					Fields = "*"
				}
			};
		}

		public static Request DeleteSheetRequest(int sheetId)
		{
			return new Request
			{
				DeleteSheet = new DeleteSheetRequest
				{
					SheetId = sheetId
				}
			};
		}

		public static async Task<IList<Response>> DeleteSheetAsync(string spreadsheetId, int sheetId)
		{
			return await BatchUpdateAsync(spreadsheetId, DeleteSheetRequest(sheetId));
		}

		public static Request MoveSheetRequest(int sheetId, int index)
		{
			return new Request
			{
				UpdateSheetProperties = new UpdateSheetPropertiesRequest
				{
					Properties = new SheetProperties
					{
						SheetId = sheetId,
						Index = index
					},
					Fields = "index"
				}
			};
		}

		public static IList<Response> BatchUpdate(string spreadsheetId, params Request[] requests)
		{
			return Service.Spreadsheets.BatchUpdate(new BatchUpdateSpreadsheetRequest
			{
				Requests = requests
			}, spreadsheetId).Execute().Replies;
		}

		public static IList<Response> BatchUpdate(string spreadsheetId, IEnumerable<Request> requests)
		{
			return BatchUpdate(spreadsheetId, requests.ToArray());
		}

		public static async Task<IList<Response>> BatchUpdateAsync(string spreadsheetId, IAsyncEnumerable<Request> requests)
		{
			SpreadsheetsResource spreadsheets = Service.Spreadsheets;
			BatchUpdateSpreadsheetRequest batchUpdateSpreadsheetRequest = new BatchUpdateSpreadsheetRequest();
			BatchUpdateSpreadsheetRequest batchUpdateSpreadsheetRequest2 = batchUpdateSpreadsheetRequest;
			batchUpdateSpreadsheetRequest2.Requests = await requests.ToListAsync();
			return (await spreadsheets.BatchUpdate(batchUpdateSpreadsheetRequest, spreadsheetId).ExecuteAsync()).Replies;
		}

		public static async Task<IList<Response>> BatchUpdateAsync(string spreadsheetId, params Request[] requests)
		{
			return (await Service.Spreadsheets.BatchUpdate(new BatchUpdateSpreadsheetRequest
			{
				Requests = requests
			}, spreadsheetId).ExecuteAsync()).Replies;
		}

		public static async Task<IList<Response>> BatchUpdateAsync(string spreadsheetId, IList<Request> requests)
		{
			return (await Service.Spreadsheets.BatchUpdate(new BatchUpdateSpreadsheetRequest
			{
				Requests = requests
			}, spreadsheetId).ExecuteAsync()).Replies;
		}

		private static async Task<IList<GridData>> _GetGridDataByColumn(string spreadsheetId, int sheetId, IEnumerable<int> columnIndices, int? startRowIndex = null)
		{
			GetSpreadsheetByDataFilterRequest getSpreadsheetByDataFilterRequest = new GetSpreadsheetByDataFilterRequest
			{
				DataFilters = new List<DataFilter>(),
				IncludeGridData = true
			};
			foreach (int columnIndex in columnIndices)
			{
				getSpreadsheetByDataFilterRequest.DataFilters.Add(new DataFilter
				{
					GridRange = new GridRange
					{
						SheetId = sheetId,
						StartRowIndex = startRowIndex,
						StartColumnIndex = columnIndex,
						EndColumnIndex = columnIndex + 1
					}
				});
			}
			return (await Service.Spreadsheets.GetByDataFilter(getSpreadsheetByDataFilterRequest, spreadsheetId).ExecuteAsync()).Sheets[0].Data;
		}

		private static List<List<CellData>> _GridDataColumnsToCellDataColumns(IList<GridData> gridDataByColumn)
		{
			List<List<CellData>> list = new List<List<CellData>>();
			foreach (GridData item in gridDataByColumn)
			{
				List<CellData> list2 = new List<CellData>();
				foreach (RowData rowDatum in item.RowData)
				{
					list2.Add(rowDatum.Values[0]);
				}
				list.Add(list2);
			}
			return list;
		}

		public static async Task<List<List<CellData>>> GetCellDataByColumnAsync(string spreadsheetId, int sheetId, IEnumerable<int> columnIndices, int? startRowIndex = null)
		{
			return _GridDataColumnsToCellDataColumns(await _GetGridDataByColumn(spreadsheetId, sheetId, columnIndices, startRowIndex));
		}

		public static Request SetCellDataByColumnRequest(int sheetId, int columnIndex, IEnumerable<CellData> data, string fields = "*", int? startRowIndex = null)
		{
			return new Request
			{
				UpdateCells = new UpdateCellsRequest
				{
					Fields = fields,
					Range = new GridRange
					{
						SheetId = sheetId,
						StartColumnIndex = columnIndex,
						EndColumnIndex = columnIndex + 1,
						StartRowIndex = startRowIndex
					},
					Rows = data.Select((CellData cellData) => new RowData
					{
						Values = new CellData[1] { cellData }
					}).ToList()
				}
			};
		}

		public static Request SetBackgroundColorRequest(GridRange range, Google.Apis.Sheets.v4.Data.Color backgroundColor)
		{
			return new Request
			{
				RepeatCell = new RepeatCellRequest
				{
					Range = range,
					Cell = new CellData
					{
						UserEnteredFormat = new CellFormat
						{
							BackgroundColor = backgroundColor
						}
					},
					Fields = "userEnteredFormat.backgroundColor"
				}
			};
		}

		public static Request SetTextFormatRequest(GridRange range, Google.Apis.Sheets.v4.Data.Color textColor = null, bool? bold = null)
		{
			return new Request
			{
				RepeatCell = new RepeatCellRequest
				{
					Range = range,
					Cell = new CellData
					{
						UserEnteredFormat = new CellFormat
						{
							TextFormat = new Google.Apis.Sheets.v4.Data.TextFormat
							{
								ForegroundColor = textColor,
								Bold = bold
							}
						}
					},
					Fields = "userEnteredFormat.textFormat"
				}
			};
		}

		public static Request SetTextHorizontalAlignmentRequest(GridRange range, string horizontalAlignment = "LEFT")
		{
			return new Request
			{
				RepeatCell = new RepeatCellRequest
				{
					Range = range,
					Cell = new CellData
					{
						UserEnteredFormat = new CellFormat
						{
							HorizontalAlignment = horizontalAlignment
						}
					},
					Fields = "userEnteredFormat.horizontalAlignment"
				}
			};
		}

		public static Request SetTextVerticalAlignmentRequest(GridRange range, string verticalAlignment = "TOP")
		{
			return new Request
			{
				RepeatCell = new RepeatCellRequest
				{
					Range = range,
					Cell = new CellData
					{
						UserEnteredFormat = new CellFormat
						{
							VerticalAlignment = verticalAlignment
						}
					},
					Fields = "userEnteredFormat.verticalAlignment"
				}
			};
		}

		public static Request ClearFormattingRequest(int sheetId)
		{
			return new Request
			{
				RepeatCell = new RepeatCellRequest
				{
					Range = new GridRange
					{
						SheetId = sheetId
					},
					Fields = "userEnteredFormat"
				}
			};
		}

		public static Request SetTextWrapping(GridRange range, WrapStrategy wrapStrategy = WrapStrategy.WRAP)
		{
			return new Request
			{
				RepeatCell = new RepeatCellRequest
				{
					Range = range,
					Cell = new CellData
					{
						UserEnteredFormat = new CellFormat
						{
							WrapStrategy = EnumUtil.Name(wrapStrategy)
						}
					},
					Fields = "userEnteredFormat.wrapStrategy"
				}
			};
		}

		public static Request AddBandingRequest(GridRange range, Google.Apis.Sheets.v4.Data.Color firstBandColor, Google.Apis.Sheets.v4.Data.Color secondBandColor, Google.Apis.Sheets.v4.Data.Color headerColor = null)
		{
			return new Request
			{
				AddBanding = new AddBandingRequest
				{
					BandedRange = new BandedRange
					{
						Range = range,
						RowProperties = new BandingProperties
						{
							FirstBandColor = firstBandColor,
							SecondBandColor = secondBandColor,
							HeaderColor = headerColor
						}
					}
				}
			};
		}

		public static async Task<IList<BandedRange>> GetBandedRangesAsync(string spreadsheetId, int sheetId)
		{
			return (await GetSheetAsync(spreadsheetId, sheetId, "sheets.bandedRanges"))?.BandedRanges ?? new BandedRange[0];
		}

		public static async IAsyncEnumerable<Request> ClearBandedRangesRequestsAsync(string spreadsheetId, int sheetId, IList<BandedRange> bandedRanges = null)
		{
			IList<BandedRange> list = bandedRanges;
			if (list == null)
			{
				list = await GetBandedRangesAsync(spreadsheetId, sheetId);
			}
			foreach (BandedRange item in list)
			{
				yield return new Request
				{
					DeleteBanding = new DeleteBandingRequest
					{
						BandedRangeId = item.BandedRangeId
					}
				};
			}
		}

		public static Request AddProtectedRangeRequest(GridRange range, IList<string> users = null, string description = "Range locked by developer.")
		{
			Request request = new Request
			{
				AddProtectedRange = new AddProtectedRangeRequest
				{
					ProtectedRange = new ProtectedRange
					{
						Description = description,
						Range = range,
						WarningOnly = (users == null || users.Count <= 0)
					}
				}
			};
			if (request.AddProtectedRange.ProtectedRange.WarningOnly != true)
			{
				request.AddProtectedRange.ProtectedRange.Editors = new Editors
				{
					Users = users
				};
			}
			return request;
		}

		public static IEnumerable<Request> AddProtectedRangeRequests(GridRange range, IList<string> users, string description = "Range locked by developer.")
		{
			yield return AddProtectedRangeRequest(range, null, description);
			if (users != null && users.Count > 0)
			{
				yield return AddProtectedRangeRequest(range, users, description);
			}
		}

		public static async Task<IList<ProtectedRange>> GetProtectedRangesAsync(string spreadsheetId, int sheetId)
		{
			return (await GetSheetAsync(spreadsheetId, sheetId, "sheets.protectedRanges"))?.ProtectedRanges ?? new ProtectedRange[0];
		}

		public static async IAsyncEnumerable<Request> ClearProtectedRangesRequestsAsync(string spreadsheetId, int sheetId, IList<ProtectedRange> protectedRanges = null)
		{
			IList<ProtectedRange> list = protectedRanges;
			if (list == null)
			{
				list = await GetProtectedRangesAsync(spreadsheetId, sheetId);
			}
			foreach (ProtectedRange item in list)
			{
				yield return new Request
				{
					DeleteProtectedRange = new DeleteProtectedRangeRequest
					{
						ProtectedRangeId = item.ProtectedRangeId
					}
				};
			}
		}

		public static Request UpdateBordersRequest(GridRange range, Border border)
		{
			return new Request
			{
				UpdateBorders = new UpdateBordersRequest
				{
					Range = range,
					Bottom = border,
					Top = border,
					Left = border,
					Right = border,
					InnerHorizontal = border,
					InnerVertical = border
				}
			};
		}

		public static Request UpdateDimensionRequest(DimensionRange range, int pixelSize)
		{
			return new Request
			{
				UpdateDimensionProperties = new UpdateDimensionPropertiesRequest
				{
					Range = range,
					Properties = new DimensionProperties
					{
						PixelSize = pixelSize
					},
					Fields = "pixelSize"
				}
			};
		}

		public static Request FreezeRowsRequest(int sheetId, int rows)
		{
			return new Request
			{
				UpdateSheetProperties = new UpdateSheetPropertiesRequest
				{
					Properties = new SheetProperties
					{
						SheetId = sheetId,
						GridProperties = new GridProperties
						{
							FrozenRowCount = rows
						}
					},
					Fields = "gridProperties.frozenRowCount"
				}
			};
		}

		public static Request FreezeColumnsRequest(int sheetId, int columns)
		{
			return new Request
			{
				UpdateSheetProperties = new UpdateSheetPropertiesRequest
				{
					Properties = new SheetProperties
					{
						SheetId = sheetId,
						GridProperties = new GridProperties
						{
							FrozenColumnCount = columns
						}
					},
					Fields = "gridProperties.frozenColumnCount"
				}
			};
		}

		public static Request SetRowCount(int sheetId, int rowCount)
		{
			return new Request
			{
				UpdateSheetProperties = new UpdateSheetPropertiesRequest
				{
					Properties = new SheetProperties
					{
						SheetId = sheetId,
						GridProperties = new GridProperties
						{
							RowCount = rowCount
						}
					},
					Fields = "gridProperties.rowCount"
				}
			};
		}

		public static Request SetColumnCount(int sheetId, int columnCount)
		{
			return new Request
			{
				UpdateSheetProperties = new UpdateSheetPropertiesRequest
				{
					Properties = new SheetProperties
					{
						SheetId = sheetId,
						GridProperties = new GridProperties
						{
							ColumnCount = columnCount
						}
					},
					Fields = "gridProperties.columnCount"
				}
			};
		}

		public static string GetMetadataValue(string spreadsheetId, int sheetId, string metadataKey)
		{
			return Service.Spreadsheets.DeveloperMetadata.Search(new SearchDeveloperMetadataRequest
			{
				DataFilters = new DataFilter[1]
				{
					new DataFilter
					{
						DeveloperMetadataLookup = new DeveloperMetadataLookup
						{
							MetadataKey = metadataKey,
							MetadataLocation = new DeveloperMetadataLocation
							{
								SheetId = sheetId
							}
						}
					}
				}
			}, spreadsheetId).Execute().MatchedDeveloperMetadata?.FirstOrDefault()?.DeveloperMetadata.MetadataValue;
		}

		public static string GetMetadataValue(string spreadsheetId, string metadataKey)
		{
			return Service.Spreadsheets.DeveloperMetadata.Search(new SearchDeveloperMetadataRequest
			{
				DataFilters = new DataFilter[1]
				{
					new DataFilter
					{
						DeveloperMetadataLookup = new DeveloperMetadataLookup
						{
							MetadataKey = metadataKey,
							MetadataLocation = new DeveloperMetadataLocation
							{
								Spreadsheet = true
							}
						}
					}
				}
			}, spreadsheetId).Execute().MatchedDeveloperMetadata?.FirstOrDefault()?.DeveloperMetadata.MetadataValue;
		}

		public static Google.Apis.Sheets.v4.Data.Color NewColor(float r, float g, float b, float a = 1f)
		{
			return new Google.Apis.Sheets.v4.Data.Color
			{
				Red = r,
				Green = g,
				Blue = b,
				Alpha = a
			};
		}

		public static Google.Apis.Sheets.v4.Data.Color NewColor(float v)
		{
			return NewColor(v, v, v);
		}
	}

	public static class Script
	{
		public const string SCOPE_SCRIPT_APP = "https://www.googleapis.com/auth/script.scriptapp";

		private static readonly string[] SCOPES;

		private static ScriptService _Service;

		public static ScriptService Service => _Service ?? (_Service = new ScriptService(new BaseClientService.Initializer
		{
			ApplicationName = "Aces & Adventures",
			HttpClientInitializer = Auth.GetDefaultOAuthCredential<ScriptService>(SCOPES)
		}));

		static Script()
		{
			SCOPES = new string[0];
		}

		public static async Task<IDictionary<string, object>> RunFunctionAsync(string scriptId, string functionName, params object[] parameters)
		{
			return (await Service.Scripts.Run(new ExecutionRequest
			{
				Function = functionName,
				Parameters = parameters
			}, scriptId).ExecuteAsync()).Response;
		}
	}

	public static class Mail
	{
		public class User
		{
			private string _email;

			private GmailService _service;

			public string email => _email;

			private string me => "me";

			public GmailService service
			{
				get
				{
					GmailService gmailService = _service;
					if (gmailService == null)
					{
						BaseClientService.Initializer obj = new BaseClientService.Initializer
						{
							ApplicationName = "Aces & Adventures"
						};
						ICredential httpClientInitializer;
						if (!string.Equals(_email.Split('@')[1], ORG_EMAIL.Split('@')[1], StringComparison.OrdinalIgnoreCase))
						{
							ICredential defaultOAuthCredentialSalt = Auth.GetDefaultOAuthCredentialSalt<GmailService>(_email, SCOPES);
							httpClientInitializer = defaultOAuthCredentialSalt;
						}
						else
						{
							httpClientInitializer = GoogleCredential.FromJson(CREDENTIALS).CreateScoped(SCOPES).CreateWithUser(_email)
								.UnderlyingCredential;
						}
						obj.HttpClientInitializer = httpClientInitializer;
						gmailService = (_service = new GmailService(obj));
					}
					return gmailService;
				}
			}

			public User(string email)
			{
				_email = email;
			}

			public async Task<Message> Send(string toEmail, string subject, string body)
			{
				MimeMessage mimeMessage = new MimeMessage();
				mimeMessage.From.Add(new MailboxAddress("", _email));
				mimeMessage.To.Add(new MailboxAddress("", toEmail));
				mimeMessage.Subject = subject;
				mimeMessage.Body = new TextPart(MimeKit.Text.TextFormat.Html)
				{
					Text = body
				};
				return await service.Users.Messages.Send(new Message
				{
					Raw = mimeMessage.ToRawMessage()
				}, me).ExecuteAsync();
			}

			public async Task<Message> Trash(string emailId)
			{
				return await service.Users.Messages.Trash(me, emailId).ExecuteAsync();
			}

			public async Task<string> Delete(string emailId)
			{
				return await service.Users.Messages.Delete(me, emailId).ExecuteAsync();
			}

			public async Task<string> DeleteThread(string threadId)
			{
				return await service.Users.Threads.Delete(me, threadId).ExecuteAsync();
			}

			public async Task<ListMessagesResponse> List(string filter = null, int maxResults = 100, params string[] labelIds)
			{
				Google.Apis.Gmail.v1.UsersResource.MessagesResource.ListRequest listRequest = service.Users.Messages.List(me);
				listRequest.MaxResults = Math.Max(1, Math.Min(maxResults, 500));
				if (filter.HasVisibleCharacter())
				{
					listRequest.Q = filter;
				}
				if (labelIds != null && labelIds.Length != 0)
				{
					listRequest.LabelIds = labelIds;
				}
				return await listRequest.ExecuteAsync();
			}

			public async Task<ListThreadsResponse> ListThreads(string filter = null, int maxResults = 100, params string[] labelIds)
			{
				Google.Apis.Gmail.v1.UsersResource.ThreadsResource.ListRequest listRequest = service.Users.Threads.List(me);
				listRequest.MaxResults = Math.Max(1, Math.Min(maxResults, 500));
				if (filter.HasVisibleCharacter())
				{
					listRequest.Q = filter;
				}
				if (labelIds != null && labelIds.Length != 0)
				{
					listRequest.LabelIds = labelIds;
				}
				return await listRequest.ExecuteAsync();
			}

			public async Task<Message> Get(string emailId, Google.Apis.Gmail.v1.UsersResource.MessagesResource.GetRequest.FormatEnum format = Google.Apis.Gmail.v1.UsersResource.MessagesResource.GetRequest.FormatEnum.Full)
			{
				Google.Apis.Gmail.v1.UsersResource.MessagesResource.GetRequest getRequest = service.Users.Messages.Get(me, emailId);
				getRequest.Format = format;
				return await getRequest.ExecuteAsync();
			}
		}

		private static readonly string[] SCOPES;

		private static Dictionary<string, User> _UsersByEmail;

		private static Dictionary<string, User> UsersByEmail => _UsersByEmail ?? (_UsersByEmail = new Dictionary<string, User>(StringComparer.OrdinalIgnoreCase));

		static Mail()
		{
			SCOPES = new string[0];
		}

		private static void _CreateFilters(string valueType, StringBuilder builder, params string[] values)
		{
			if (values == null || values.Length == 0)
			{
				return;
			}
			builder.Append("{");
			for (int i = 0; i < values.Length; i++)
			{
				if (i > 0)
				{
					builder.Append(" ");
				}
				builder.Append(valueType + ":" + values[i]);
			}
			builder.Append("} ");
		}

		public static User AsUser(string userEmail)
		{
			return UsersByEmail.GetValueOrDefault(userEmail) ?? (UsersByEmail[userEmail] = new User(userEmail));
		}

		public static string ToEmailName(string name)
		{
			StringBuilder stringBuilder = new StringBuilder(name.Length);
			foreach (char c in name)
			{
				if (char.IsLetter(c))
				{
					stringBuilder.Append(char.ToLower(c));
				}
				else if (char.IsDigit(c))
				{
					stringBuilder.Append(c);
				}
				else if (stringBuilder.Last() != '-')
				{
					stringBuilder.Append('-');
				}
			}
			return stringBuilder.ToString();
		}

		public static string CreateListFilter(string[] from = null, string[] to = null, string[] subject = null, bool? hasSpreadsheet = null)
		{
			StringBuilder stringBuilder = new StringBuilder();
			_CreateFilters("from", stringBuilder, from);
			_CreateFilters("to", stringBuilder, to);
			_CreateFilters("subject", stringBuilder, subject);
			if (hasSpreadsheet == true)
			{
				stringBuilder.Append("has:spreadsheet");
			}
			if (stringBuilder.Length <= 0)
			{
				return null;
			}
			return stringBuilder.ToString().Trim();
		}
	}

	public static class AdminDirectory
	{
		public enum MemberRole
		{
			MEMBER,
			MANAGER,
			OWNER
		}

		public enum DeliverySetting
		{
			NONE,
			DISABLED,
			DIGEST,
			DAILY,
			ALL_MAIL
		}

		private static readonly string[] SCOPES;

		private static DirectoryService _Service;

		public static DirectoryService Service => _Service ?? (_Service = new DirectoryService(new BaseClientService.Initializer
		{
			ApplicationName = "Aces & Adventures",
			HttpClientInitializer = Auth.GetDefaultOAuthCredential<DirectoryService>(SCOPES)
		}));

		static AdminDirectory()
		{
			SCOPES = new string[0];
		}

		public static async Task<Google.Apis.Admin.Directory.directory_v1.Data.Group> CreateGroup(string email, string name, string description)
		{
			return await Service.Groups.Insert(new Google.Apis.Admin.Directory.directory_v1.Data.Group
			{
				Email = email,
				Name = name,
				Description = description,
				AdminCreated = true
			}).ExecuteAsync();
		}

		public static async Task<Google.Apis.Admin.Directory.directory_v1.Data.Group> GetGroup(string groupKey)
		{
			return await Service.Groups.Get(groupKey).ExecuteAsync();
		}

		public static async Task<Try<Google.Apis.Admin.Directory.directory_v1.Data.Group>> GetOrCreateGroup(string email, string name, string description)
		{
			Try<Google.Apis.Admin.Directory.directory_v1.Data.Group> @try = await GetGroup(email).Try(setSuccessTo: false);
			if (@try == null)
			{
				@try = await CreateGroup(email, name, description).Try();
			}
			return @try;
		}

		public static async Task<string> DeleteGroup(string groupKey)
		{
			return await Service.Groups.Delete(groupKey).ExecuteAsync();
		}

		public static async Task<Member> AddMemberToGroup(string groupKey, string memberEmail, MemberRole role = MemberRole.MEMBER, DeliverySetting delivery = DeliverySetting.NONE)
		{
			return await Service.Members.Insert(new Member
			{
				Email = memberEmail,
				Role = EnumUtil.Name(role),
				DeliverySettings = EnumUtil.Name(delivery)
			}, groupKey).ExecuteAsync();
		}

		public static async Task<Member> UpdateMemberInGroup(string groupKey, string memberEmail, MemberRole role = MemberRole.MEMBER, DeliverySetting delivery = DeliverySetting.NONE)
		{
			return await Service.Members.Update(new Member
			{
				Email = memberEmail,
				Role = EnumUtil.Name(role),
				DeliverySettings = EnumUtil.Name(delivery)
			}, groupKey, memberEmail).ExecuteAsync();
		}

		public static async Task<Try<Member>> UpdateOrAddMemberInGroup(string groupKey, string memberEmail, MemberRole role = MemberRole.MEMBER, DeliverySetting delivery = DeliverySetting.NONE)
		{
			Try<Member> @try = await UpdateMemberInGroup(groupKey, memberEmail, role, delivery).Try(setSuccessTo: false);
			if (@try == null)
			{
				@try = await AddMemberToGroup(groupKey, memberEmail, role, delivery).Try();
			}
			return @try;
		}

		public static async Task<Try<Member>[]> UpdateOrAddMembersToGroup(string groupKey, IEnumerable<string> memberEmails, MemberRole role = MemberRole.MEMBER, DeliverySetting delivery = DeliverySetting.NONE)
		{
			return await Task.WhenAll(memberEmails.Select((string email) => UpdateOrAddMemberInGroup(groupKey, email, role, delivery)));
		}
	}

	public static class GroupSettings
	{
		public const string TEMPLATE = "localization-template@triplebtitles.com";

		private static readonly string[] SCOPES;

		private static GroupssettingsService _Service;

		public static GroupssettingsService Service => _Service ?? (_Service = new GroupssettingsService(new BaseClientService.Initializer
		{
			ApplicationName = "Aces & Adventures",
			HttpClientInitializer = Auth.GetDefaultOAuthCredential<GroupssettingsService>(SCOPES)
		}));

		static GroupSettings()
		{
			SCOPES = new string[0];
		}

		public static async Task<Google.Apis.Groupssettings.v1.Data.Groups> Get(string groupEmail)
		{
			return await new Google.Apis.Groupssettings.v1.GroupsResource.GetRequest(Service, groupEmail)
			{
				Alt = GroupssettingsBaseServiceRequest<Google.Apis.Groupssettings.v1.Data.Groups>.AltEnum.Json
			}.ExecuteAsync();
		}

		public static async Task<Google.Apis.Groupssettings.v1.Data.Groups> Set(string groupEmail, Google.Apis.Groupssettings.v1.Data.Groups settings)
		{
			return await new Google.Apis.Groupssettings.v1.GroupsResource.UpdateRequest(Service, settings, groupEmail)
			{
				Alt = GroupssettingsBaseServiceRequest<Google.Apis.Groupssettings.v1.Data.Groups>.AltEnum.Json
			}.ExecuteAsync();
		}

		public static async Task<Google.Apis.Groupssettings.v1.Data.Groups> Patch(string groupEmail, Google.Apis.Groupssettings.v1.Data.Groups settings)
		{
			return await new Google.Apis.Groupssettings.v1.GroupsResource.PatchRequest(Service, settings, groupEmail)
			{
				Alt = GroupssettingsBaseServiceRequest<Google.Apis.Groupssettings.v1.Data.Groups>.AltEnum.Json
			}.ExecuteAsync();
		}

		public static async Task<Google.Apis.Groupssettings.v1.Data.Groups> CopyTo(string copyToGroupEmail, string copyFromGroupEmail = "localization-template@triplebtitles.com")
		{
			return await Patch(copyToGroupEmail, (await Get(copyFromGroupEmail)).GetCopyPatch());
		}
	}

	public static class FirebaseLink
	{
		private const string DOMAIN_PREFIX = "acesandadventures.page.link";

		private static readonly string[] SCOPES;

		private static FirebaseDynamicLinksService _Service;

		public static FirebaseDynamicLinksService Service => _Service ?? (_Service = new FirebaseDynamicLinksService(new BaseClientService.Initializer
		{
			ApplicationName = "Aces & Adventures",
			HttpClientInitializer = GoogleCredential.FromJson(CREDENTIALS).CreateScoped(SCOPES).CreateWithUser(SERVICE_ACCOUNT_EMAIL)
				.UnderlyingCredential
		}));

		static FirebaseLink()
		{
			SCOPES = new string[0];
		}

		public static async Task<CreateShortDynamicLinkResponse> CreateAsync(string longURL)
		{
			return await Service.ShortLinks.Create(new CreateShortDynamicLinkRequest
			{
				DynamicLinkInfo = new DynamicLinkInfo
				{
					Link = longURL,
					DomainUriPrefix = "acesandadventures.page.link"
				}
			}).ExecuteAsync();
		}

		public static async Task<CreateManagedShortLinkResponse> CreateManagedAsync(string name, string longURL)
		{
			return await Service.ManagedShortLinks.Create(new CreateManagedShortLinkRequest
			{
				DynamicLinkInfo = new DynamicLinkInfo
				{
					Link = longURL,
					DomainUriPrefix = "acesandadventures.page.link"
				},
				Name = name
			}).ExecuteAsync();
		}
	}

	public static class Rebrandly
	{
		public static class Links
		{
			public static async Task<global::Rebrandly.Models.Link> UpdateOrCreate(string destination, string slashtag)
			{
				global::Rebrandly.Links links = Client.Links;
				string slashtag2 = slashtag;
				global::Rebrandly.Models.Link link = (await links.ListAll(null, null, slashtag2, (await Account()).ID)).FirstOrDefault((global::Rebrandly.Models.Link l) => string.Equals(l.SlashTag, slashtag, StringComparison.OrdinalIgnoreCase));
				if (link != null)
				{
					Debug.Log("Updating Link: " + link.ShortURL + " to point to " + destination);
					if (string.Equals(link.Destination, destination, StringComparison.OrdinalIgnoreCase))
					{
						return link;
					}
					return await Client.Links.Update(link.ID, link.Title, link.Favorite == true, destination);
				}
				global::Rebrandly.Models.Link link2 = await Client.Links.Create(destination, slashtag);
				Debug.Log("Creating Link: " + link2.ShortURL + " to point to " + destination);
				return link2;
			}
		}

		private static RebrandlyClient _Client;

		private static Task<global::Rebrandly.Models.Account> _Account;

		public static RebrandlyClient Client => _Client ?? (_Client = new RebrandlyClient(BlobData.GetBlob("Rebrandly API")));

		private static async Task<global::Rebrandly.Models.Account> Account()
		{
			return await (_Account ?? (_Account = Client.Account.Get()));
		}
	}

	public struct CsvKeyValue
	{
		[Index(0, -1)]
		public string key { get; set; }

		[Index(1, -1)]
		public string value { get; set; }
	}

	private const string PROJECT_ID = "aces-and-adventures";

	private static string _ProjectName;

	private const string LOCATION_ID = "us-central1";

	private static LocationName _LocationName;

	public const string PROJECT_DRIVE_FOLDER = "Aces & Adventures";

	private static string CREDENTIALS => BlobData.GetBlob("SA");

	public static string SERVICE_ACCOUNT_EMAIL => BlobData.GetBlob("SA Email");

	public static string ORG_EMAIL => BlobData.GetBlob("Org Email");

	private static string CLIENT_ID => BlobData.GetBlob("Oauth Id");

	private static string CLIENT_SECRET => BlobData.GetBlob("Oauth S");

	private static string PROJECT_NAME => _ProjectName ?? (_ProjectName = new ProjectName("aces-and-adventures").ToString());

	private static LocationName LOCATION_NAME => _LocationName ?? (_LocationName = new LocationName("aces-and-adventures", "us-central1"));

	public static Stream WriteToCsvStream(this IEnumerable<IEnumerable<string>> rows, Stream streamToWriteInto = null)
	{
		if (streamToWriteInto == null)
		{
			streamToWriteInto = new MemoryStream(1024);
		}
		CsvWriter csvWriter = new CsvWriter(new StreamWriter(streamToWriteInto), CultureInfo.InvariantCulture);
		foreach (IEnumerable<string> row in rows)
		{
			foreach (string item in row)
			{
				csvWriter.WriteField(item);
			}
			csvWriter.NextRecord();
		}
		csvWriter.Flush();
		return streamToWriteInto;
	}

	public static Stream WriteToCsvStream(this Dictionary<string, string> map, Stream streamToWriteInto = null)
	{
		if (streamToWriteInto == null)
		{
			streamToWriteInto = new MemoryStream(1024);
		}
		CsvWriter csvWriter = new CsvWriter(new StreamWriter(streamToWriteInto), CultureInfo.InvariantCulture);
		foreach (KeyValuePair<string, string> item in map)
		{
			csvWriter.WriteField(item.Key);
			csvWriter.WriteField(item.Value);
			csvWriter.NextRecord();
		}
		csvWriter.Flush();
		return streamToWriteInto;
	}

	public static Dictionary<string, string> CsvStreamToDictionary(Stream csvStream)
	{
		csvStream.Position = 0L;
		using CsvReader csvReader = new CsvReader(new StreamReader(csvStream), new CsvConfiguration(CultureInfo.InvariantCulture)
		{
			HasHeaderRecord = false
		});
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		foreach (CsvKeyValue record in csvReader.GetRecords<CsvKeyValue>())
		{
			dictionary[record.key] = record.value;
		}
		return dictionary;
	}

	public static string Combine(this IEnumerable<string> items, string delimiter = ", ")
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (string item in items)
		{
			stringBuilder.Append(item);
			stringBuilder.Append(delimiter);
		}
		if (stringBuilder.Length > 0)
		{
			stringBuilder.Remove(stringBuilder.Length - delimiter.Length, delimiter.Length);
		}
		return stringBuilder.ToString();
	}

	public static string ToCommaSeparatedList(this IEnumerable<string> items)
	{
		return items.Combine(",");
	}

	public static DriveBaseServiceRequest<T> SetFields<T>(this DriveBaseServiceRequest<T> request, string fields)
	{
		if (string.IsNullOrWhiteSpace(fields))
		{
			return request;
		}
		request.Fields = fields;
		return request;
	}

	public static SheetsBaseServiceRequest<T> SetFields<T>(this SheetsBaseServiceRequest<T> request, string fields)
	{
		if (string.IsNullOrWhiteSpace(fields))
		{
			return request;
		}
		request.Fields = fields;
		return request;
	}

	public static bool HasValue(this CellData cellData)
	{
		return !string.IsNullOrWhiteSpace(cellData?.FormattedValue);
	}

	public static string Value(this CellData cellData)
	{
		return cellData?.FormattedValue ?? "";
	}

	public static async Task<R> ResultAsync<R, M>(this Operation<R, M> operation) where R : class, IMessage<R>, new() where M : class, IMessage<M>, new()
	{
		return (await operation.PollUntilCompletedAsync()).Result;
	}

	public static string ToUrl64(this string s)
	{
		return Base64UrlEncoder.Encode(s);
	}

	public static string ToRawMessage(this MimeMessage message)
	{
		using MemoryStream memoryStream = new MemoryStream();
		message.WriteTo(memoryStream);
		memoryStream.Position = 0L;
		using StreamReader streamReader = new StreamReader(memoryStream);
		return streamReader.ReadToEnd().ToUrl64();
	}

	public static async Task<Try<T>> Try<T>(this Task<T> task, bool setSuccessTo = true)
	{
		try
		{
			return new Try<T>(await task, setSuccessTo);
		}
		catch
		{
			return null;
		}
	}

	public static async Task<T> TryRepeat<T>(this Func<Task<T>> createTask, float waitTimeBetweenAttemptsInSeconds = 1f, int maxAttempts = 60, bool waitBeforeFirstAttempt = false)
	{
		TimeSpan delayInSeconds = TimeSpan.FromSeconds(waitTimeBetweenAttemptsInSeconds);
		if (waitBeforeFirstAttempt && waitTimeBetweenAttemptsInSeconds > 0f)
		{
			await Task.Delay(delayInSeconds);
		}
		try
		{
			return await createTask();
		}
		catch (Exception innerException)
		{
			int num = maxAttempts - 1;
			maxAttempts = num;
			if (num <= 0)
			{
				throw new InvalidOperationException("Try Repeat<T> ran out of attempts.", innerException);
			}
			if (waitTimeBetweenAttemptsInSeconds > 0f)
			{
				await Task.Delay(delayInSeconds);
			}
			return await createTask.TryRepeat(waitTimeBetweenAttemptsInSeconds, maxAttempts);
		}
	}

	public static bool Out<T>(this Try<T> t, out T result)
	{
		result = t;
		return t;
	}

	public static Google.Apis.Groupssettings.v1.Data.Groups GetCopyPatch(this Google.Apis.Groupssettings.v1.Data.Groups g)
	{
		return new Google.Apis.Groupssettings.v1.Data.Groups
		{
			AllowExternalMembers = g.AllowExternalMembers,
			AllowGoogleCommunication = g.AllowGoogleCommunication,
			AllowWebPosting = g.AllowWebPosting,
			ArchiveOnly = g.ArchiveOnly,
			CustomFooterText = g.CustomFooterText,
			CustomReplyTo = g.CustomReplyTo,
			CustomRolesEnabledForSettingsToBeMerged = g.CustomRolesEnabledForSettingsToBeMerged,
			DefaultMessageDenyNotificationText = g.DefaultMessageDenyNotificationText,
			DefaultSender = g.DefaultSender,
			EnableCollaborativeInbox = g.EnableCollaborativeInbox,
			FavoriteRepliesOnTop = g.FavoriteRepliesOnTop,
			IncludeCustomFooter = g.IncludeCustomFooter,
			IncludeInGlobalAddressList = g.IncludeInGlobalAddressList,
			IsArchived = g.IsArchived,
			MembersCanPostAsTheGroup = g.MembersCanPostAsTheGroup,
			MessageModerationLevel = g.MessageModerationLevel,
			PrimaryLanguage = g.PrimaryLanguage,
			ReplyTo = g.ReplyTo,
			SendMessageDenyNotification = g.SendMessageDenyNotification,
			SpamModerationLevel = g.SpamModerationLevel,
			WhoCanApproveMembers = g.WhoCanApproveMembers,
			WhoCanAssistContent = g.WhoCanAssistContent,
			WhoCanBanUsers = g.WhoCanBanUsers,
			WhoCanContactOwner = g.WhoCanContactOwner,
			WhoCanDiscoverGroup = g.WhoCanDiscoverGroup,
			WhoCanJoin = g.WhoCanJoin,
			WhoCanLeaveGroup = g.WhoCanLeaveGroup,
			WhoCanModerateContent = g.WhoCanModerateContent,
			WhoCanModerateMembers = g.WhoCanModerateMembers,
			WhoCanPostMessage = g.WhoCanPostMessage,
			WhoCanViewGroup = g.WhoCanViewGroup,
			WhoCanViewMembership = g.WhoCanViewMembership
		};
	}

	public static T SetTimeout<T>(this T baseClientService, double timeoutSeconds) where T : BaseClientService
	{
		baseClientService.HttpClient.Timeout = TimeSpan.FromSeconds(timeoutSeconds);
		return baseClientService;
	}
}
