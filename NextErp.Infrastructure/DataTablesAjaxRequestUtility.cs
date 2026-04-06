using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using System.Text;

namespace NextErp.Infrastructure
{
    public class DataTablesAjaxRequestUtility(HttpRequest request)
    {
        public int Start
        {
            get
            {
                return int.Parse(RequestData.Where(x => x.Key == "start")
                    .FirstOrDefault().Value);
            }
        }
        public int Length
        {
            get
            {
                return int.Parse(RequestData.Where(x => x.Key == "length")
                    .FirstOrDefault().Value);
            }
        }

        public string SearchText
        {
            get
            {
                return RequestData.Where(x => x.Key == "search[value]")
                    .FirstOrDefault().Value;
            }
        }

        public int PageIndex
        {
            get
            {
                if (Length > 0)
                    return (Start / Length) + 1;
                else
                    return 1;
            }
        }

        public int PageSize
        {
            get
            {
                if (Length == 0)
                    return 10;
                else
                    return Length;
            }
        }

        private IEnumerable<KeyValuePair<string, StringValues>> RequestData
        {
            get
            {
                var method = request.Method.ToLower();
                if (method == "get")
                    return request.Query;
                else if (method == "post")
                    return request.Form;
                else
                    throw new InvalidOperationException("Http method not supported, use get or post");
            }
        }

        public static object EmptyResult
        {
            get
            {
                return new
                {
                    recordsTotal = 0,
                    recordsFiltered = 0,
                    data = (new string[] { }).ToArray()
                };
            }
        }

        public string GetSortText(string[] columnNames)
        {
            var sortText = new StringBuilder();
            for (var i = 0; i < columnNames.Length; i++)
            {
                if (RequestData.Any(x => x.Key == $"order[{i}][column]"))
                {
                    if (sortText.Length > 0)
                        sortText.Append(",");

                    var columnValue = RequestData.Where(x => x.Key == $"order[{i}][column]").FirstOrDefault();
                    var directionValue = RequestData.Where(x => x.Key == $"order[{i}][dir]").FirstOrDefault();

                    var column = int.Parse(columnValue.Value.ToArray()[0]);
                    var direction = directionValue.Value.ToArray()[0];
                    var sortDirection = $"{columnNames[column]} {(direction == "asc" ? "asc" : "desc")}";
                    sortText.Append(sortDirection);
                }
            }
            return sortText.ToString();
        }
    }
}