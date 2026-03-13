namespace SocietyVaccinations
{
    public class ApiResponse<T>
    {
        public bool success {  get; set; }
        public string message { get; set; }
        public T data { get; set; }
        public int status { get; set; }
        public Dictionary<string, string[]> errors { get; set; }

        public static ApiResponse<T> Success(T data, string msg, int code = 200)
        {
            return new ApiResponse<T> {
                success = true,
                data = data,
                message = msg,
                status = code, errors = null
            };
        }

        public static ApiResponse<T> Error(string msg, int code = 400, Dictionary<string, string[]> errs = null)
        {
            return new ApiResponse<T>
            {
                success = false,
                data = default(T),
                message = msg,
                status = code,
                errors = errs ?? new Dictionary<string, string[]>()
            };
        }
    }
}
