using ClinicApp.Helpers;
using System;
using System.Collections.Generic;

namespace ClinicApp.Helpers
{
    // Enum ها در فایل Enums.cs تعریف شوند
    // public enum SecurityLevel { Low, Medium, High, Critical }
    // public enum ErrorCategory { General, Validation, NotFound, Unauthorized, BusinessLogic, Database }
    // public enum OperationStatus { Pending, InProgress, Completed, Failed, Canceled }

    public class ServiceResult
    {
        public bool Success { get; protected set; }
        public string Message { get; protected set; }
        public string Code { get; protected set; }
        public SecurityLevel SecurityLevel { get; protected set; }
        public ErrorCategory Category { get; protected set; }
        public DateTime Timestamp { get; protected set; }
        public string TimestampShamsi { get; protected set; }
        public Dictionary<string, object> Metadata { get; protected set; }
        public string OperationId { get; protected set; }
        public string UserId { get; protected set; }
        public string UserName { get; protected set; }
        public string UserFullName { get; protected set; }
        public string UserRole { get; protected set; }
        public string SystemName { get; protected set; }
        public string OperationName { get; protected set; }
        public OperationStatus Status { get; protected set; }

        protected ServiceResult()
        {
            Timestamp = DateTime.UtcNow;
            TimestampShamsi = Timestamp.ToPersianDateTime();
            Metadata = new Dictionary<string, object>();
            OperationId = Guid.NewGuid().ToString();
            Status = OperationStatus.Pending;
            SecurityLevel = SecurityLevel.Medium;
            Category = ErrorCategory.General;
        }

        public static ServiceResult Successful(
            string message = "عملیات با موفقیت انجام شد.",
            string operationName = null,
            string userId = null,
            string userName = null)
        {
            return new ServiceResult
            {
                Success = true,
                Message = message,
                Code = "SUCCESS",
                OperationName = operationName,
                UserId = userId,
                UserName = userName,
                Status = OperationStatus.Completed
            };
        }

        public static ServiceResult Failed(
            string message,
            string code = "GENERAL_ERROR",
            ErrorCategory category = ErrorCategory.General)
        {
            return new ServiceResult
            {
                Success = false,
                Message = message,
                Code = code,
                Category = category,
                Status = OperationStatus.Failed
            };
        }

        public ServiceResult WithMetadata(string key, object value)
        {
            Metadata[key] = value;
            return this;
        }
    }

    public class ServiceResult<T> : ServiceResult
    {
        public T Data { get; private set; } // Setter به private تغییر کرد

        public int? TotalCount { get; private set; } // Setter به private تغییر کرد
        public int? PageNumber { get; private set; } // Setter به private تغییر کرد
        public int? PageSize { get; private set; } // Setter به private تغییر کرد
        public bool HasNextPage { get; private set; } // Setter به private تغییر کرد
        public bool HasPreviousPage { get; private set; } // Setter به private تغییر کرد

        private ServiceResult() : base() { } // سازنده خصوصی برای استفاده توسط Factory Methods

        public static ServiceResult<T> Successful(
            T data,
            string message = "عملیات با موفقیت انجام شد.")
        {
            return new ServiceResult<T>
            {
                Success = true,
                Message = message,
                Data = data,
                Status = OperationStatus.Completed,
                Code = "SUCCESS"
            };
        }

        public static new ServiceResult<T> Failed(
            string message,
            string code = "GENERAL_ERROR",
            ErrorCategory category = ErrorCategory.General)
        {
            return new ServiceResult<T>
            {
                Success = false,
                Message = message,
                Code = code,
                Category = category,
                Data = default(T),
                Status = OperationStatus.Failed
            };
        }

        private void CalculatePagination()
        {
            if (PageNumber.HasValue && PageSize.HasValue && PageSize > 0 && TotalCount.HasValue)
            {
                var totalPages = (int)Math.Ceiling((double)TotalCount.Value / PageSize.Value);
                HasNextPage = PageNumber < totalPages;
                HasPreviousPage = PageNumber > 1;
            }
        }

        public ServiceResult<T> WithPagination(int totalCount, int pageNumber, int pageSize)
        {
            TotalCount = totalCount;
            PageNumber = pageNumber;
            PageSize = pageSize;
            CalculatePagination();
            return this;
        }
    }
}