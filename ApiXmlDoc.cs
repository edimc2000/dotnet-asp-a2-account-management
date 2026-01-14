using System.Text.Json.Serialization;

namespace AccountManagement
{
    public class ApiXmlDoc
    {
    
        /// <summary>
        /// Response for successful API request
        /// </summary>
        public class ApiResponseSuccess<T> 
        {


            ///// <summary>
            ///// Indicates if the request was successful
            ///// </summary>
            ///// <example>true</example>
            //public required bool Success { get; set; }

            ///// <summary>
            ///// Response message
            ///// </summary>
            ///// <example>Account created successfully</example>
            //public required string Message { get; set; }
        
            ///// <summary>
            ///// The response data payload
            ///// </summary>
            //public required T Data { get; set; }


            public bool Success { get; set; }
            public string Message { get; set; }


            [JsonPropertyName("data")]  // This ensures it serializes as "data"
            public T JsonAccountInput { get; set; }  // Rename this to Data if possible
    
 



        }
    
        //public class AccountData
        //{
        //    /// <summary>
        //    /// Unique identifier of the account
        //    /// </summary>
        //    /// <example>12345</example>
        //    public int Id { get; set; }
        
        //    /// <summary>
        //    /// Account holder's first name
        //    /// </summary>
        //    /// <example>John</example>
        //    public string FirstName { get; set; }
        
        //    /// <summary>
        //    /// Account holder's last name
        //    /// </summary>
        //    /// <example>Doe</example>
        //    public string LastName { get; set; }
        
        //    /// <summary>
        //    /// Account holder's email address
        //    /// </summary>
        //    /// <example>john.doe@example.com</example>
        //    public string EmailAddress { get; set; }
        //}


        /// <summary>
        /// Response for API errors without data payload
        /// </summary>
        public class ApiResponseNull 
        {
            /// <summary>
            /// Indicates if the request was successful
            /// </summary>
            /// <example>false</example>
            public required bool Success { get; set; }

            /// <summary>
            /// Response message
            /// </summary>
            /// <example>Unable to parse JSON body</example>
            public required string Message { get; set; }

        }


        /// <summary>
        /// Response for API errors - JSON data Malformed 
        /// </summary>
        public class ApiResponseMalformed
        {
            /// <summary>
            /// Indicates if the request was successful
            /// </summary>
            /// <example>false</example>
            public required bool Success { get; set; }

            /// <summary>
            /// Response message
            /// </summary>
            /// <example>Malformed JSON in request body</example>
            public required string Message { get; set; }

        }

        /// <summary>
        /// Response for API errors - duplicate - email already registered 
        /// </summary>
        public class ApiResponseDuplicate
        {
            /// <summary>
            /// Indicates if the request was successful
            /// </summary>
            /// <example>false</example>
            public required bool Success { get; set; }

            /// <summary>
            /// Response message
            /// </summary>
            /// <example>This email address is already registered to an existing account</example>
            public required string Message { get; set; }

        }

    }
}
