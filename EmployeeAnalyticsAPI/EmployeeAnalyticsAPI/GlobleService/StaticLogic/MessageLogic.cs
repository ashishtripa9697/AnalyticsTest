namespace EmployeeAnalyticsAPI.GlobleService.StaticLogic
{
    public static class MessageLogic
    {
        //EmployeeClassQuery
        public const string Question = "Question cannot be empty.";
        public const string Ingest = "Ingestion completed.";
        public const string NoEmployeeData = "No employee data found. Please run ingest first.";
        public const string AiAnswerIfNoData = "You are an HR analytics assistant. Answer using only the employee data provided. If the answer is not in the data, say you don't know.";
        public const string ModelAnswer = "No answer returned from the model.";

        //DocumentQuery

        public const string NoFile = "No file uploaded.";
        public const string Supported = "Only PDF files are supported.";
        public const string NoTextExtract = "No text could be extracted from the PDF.";
        public const string NoFileUpload = "No documents found. Please upload a PDF first.";
        public const string CompanyPolicyAssisstent = "You are a company policy assistant. " +
                                        "Answer questions using only the policy document " +
                                        "content provided. Always mention the page number " +
                                        "your answer comes from. " +
                                        "If the answer is not in the document, say so.";
        public const string NoAnswer = "No answer returned.";

        //FileLogger
        public const string NoStackTrace = "No stack trace available.";
        public const string NoStackTraceInnerException = "No inner exception stack trace available.";
        public const string DoubleLine = "══════════════════════════════════════════════════";
        public const string DashedLine = "==============================";
        public const string SignleLine = "-----------------------------";
        public const string PlusLine = "+++++++++++++++++++++++++++++";

    }
}
