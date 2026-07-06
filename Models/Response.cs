using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class Response
    {
        public Response(
            bool? status = true,
            string? message = "",
            object? data = null,
            Pagination? pagination = null)
        {
            this.status = status;
            this.message = message;
            this.data = data;
            this.pagination = pagination;
        }

        public bool? status { get; set; }
        public string? message { get; set; }
        public string? systemMessage { get; set; }
        public object? data { get; set; }
        public Pagination? pagination { get; set; }
    }

    public class Pagination
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalRecords { get; set; }
        public int TotalPages =>
            (int)Math.Ceiling((double)TotalRecords / PageSize);
    }

}
