// This file is used for swagger custom parameters and responses

namespace AccountManagement.Support;

/// <summary>Contains response format classes for API documentation.</summary>
public class ApiDocsResponseFormat
{
    /// <summary>Response for successful API request</summary>
    public class ApiResponseSuccess<T>
    {
        /// <summary>Indicates if the request was successful</summary>
        /// <example>true</example>
        public required bool Success { get; set; }

        /// <summary>Response message</summary>
        /// <example>Account created successfully</example>
        public required string Message { get; set; }

        /// <summary>The response data payload</summary>
        public required T Data { get; set; }
    }

    /// <summary>Response for successful API request</summary>
    public class ApiSearchResponseFormat<T>
    {
        /// <summary>Indicates if the request was successful</summary>
        /// <example>true</example>
        public required bool Success { get; set; }

        /// <summary>Response message</summary>
        /// <example>Account retrieved successfully</example>
        public required string Message { get; set; }

        /// <summary>The response data payload</summary>
        public required T Data { get; set; }
    }


    /// <summary>Response for API errors without data payload</summary>
    public class ApiResponseNull
    {
        /// <summary>Indicates if the request was successful</summary>
        /// <example>false</example>
        public required bool Success { get; set; }

        /// <summary>Response message</summary>
        /// <example>Unable to parse JSON body</example>
        public required string Message { get; set; }
    }

    /// <summary>Response for API errors - JSON data Malformed</summary>
    public class ApiResponseMalformed
    {
        /// <summary>Indicates if the request was successful</summary>
        /// <example>false</example>
        public required bool Success { get; set; }

        /// <summary>Response message</summary>
        /// <example>Malformed JSON in request body</example>
        public required string Message { get; set; }
    }


    /// <summary>Response for API errors for invalid account</summary>
    public class ApiResponseFail
    {
        /// <summary>Indicates if the request was successful </summary>
        /// <example>false</example>
        public required bool Success { get; set; }

        /// <summary>Response message</summary>
        /// <example>'200a' is not a valid account Id</example>
        public required string Message { get; set; }
    }


    /// <summary>Response for API errors - duplicate - email already registered</summary>
    public class ApiResponseDuplicate
    {
        /// <summary>Indicates if the request was successful</summary>
        /// <example>false</example>
        public required bool Success { get; set; }

        /// <summary>Response message</summary>
        /// <example>This email address is already registered to an existing account</example>
        public required string Message { get; set; }
    }

    /// <summary>Response for successful account deletion.</summary>
    public class ApiResponseDeleteSuccess
    {
        /// <summary>Indicates if the request was successful</summary>
        /// <example>true</example>
        public required bool Success { get; set; }

        /// <summary>Response message</summary>
        /// <example>Account deleted successfully</example>
        public required string Message { get; set; }
    }

    /// <summary>Response for failed account deletion.</summary>
    public class ApiResponseDeleteFailed
    {
        /// <summary>Indicates if the request was successful</summary>
        /// <example>true</example>
        public required bool Success { get; set; }

        /// <summary>Response message</summary>
        /// <example>'205a' is not a valid account Id</example>
        public required string Message { get; set; }
    }

}