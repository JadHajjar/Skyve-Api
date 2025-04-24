namespace SkyveApi.Utilities;

public static class GoFileHelper
{
	public static async Task<string> CreateFolder(string token, string rootFolder, string folderName)
	{
		var folder = new CreateFolderPayload
		{
			parentFolderId = rootFolder,
			folderName = folderName
		};

		var result = await ApiUtil.Post<CreateFolderPayload, CreateFolderPayload.Result>("https://api.gofile.io/contents/createFolder", folder, headers: [("Authorization", "Bearer " + token)]);

		return result!.data!.id!;
	}

	public static async Task<string> GetServer()
	{
		var servers = await ApiUtil.Get<ServerPayload>("https://api.gofile.io/servers");

		return servers!.data!.servers![0].name!;
	}

	private class ServerPayload
	{
		public Data? data { get; set; }

		public class Data
		{
			public Server[]? servers { get; set; }
		}

		public class Server
		{
			public string? name { get; set; }
		}
	}

	private class CreateFolderPayload
	{
		public string? parentFolderId { get; set; }
		public string? folderName { get; set; }

		public class Result
		{
			public Data? data { get; set; }

			public class Data
			{
				public string? id { get; set; }
			}
		}
	}
}
