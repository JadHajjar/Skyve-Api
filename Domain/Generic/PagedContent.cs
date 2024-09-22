using System.Collections.Generic;

namespace SkyveApi.Domain.Generic;
public class PagedContent<T>
{
	public int Page { get; set; }
	public int PageSize { get; set; }
	public int TotalPages { get; set; }
	public List<T>? Items { get; set; }
}
