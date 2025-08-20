using System;
using System.Collections.Generic;

namespace ClinicApp.Interfaces
{
    public class PagedResult<T>
    {
        public List<T> Items { get; set; } = new List<T>();
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalItems { get; set; }

        // پراپرتی‌های محاسباتی جدید
        public int TotalPages => (int)Math.Ceiling((double)TotalItems / PageSize);
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;
        public int PreviousPageNumber => HasPreviousPage ? PageNumber - 1 : 1;
        public int NextPageNumber => HasNextPage ? PageNumber + 1 : TotalPages;

        // سازنده اصلی
        public PagedResult(List<T> items, int totalItems, int pageNumber, int pageSize)
        {
            Items = items;
            TotalItems = totalItems;
            PageNumber = pageNumber;
            PageSize = pageSize;
        }

        // سازنده بدون پارامتر
        public PagedResult() { }
    }
}