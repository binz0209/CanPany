namespace CanPany.Application.Common.Constants;

/// <summary>
/// I18N Keys - Định nghĩa tất cả các keys cho I18N
/// Pattern: [Type].[Module].[Action].[SubType]
/// </summary>
public static class I18nKeys
{
    public static class Error
    {
        public static class Common
        {
            public const string NotFound = "Error.Common.NotFound";
            public const string Unauthorized = "Error.Common.Unauthorized";
            public const string InternalServerError = "Error.Common.InternalServerError";
            public const string BadRequest = "Error.Common.BadRequest";
            public const string ValidationFailed = "Error.Common.ValidationFailed";
        }

        public static class User
        {
            public static class Register
            {
                public const string EmailExists = "Error.User.Register.EmailExists";
                public const string Failed = "Error.User.Register.Failed";
            }

            public static class Login
            {
                public const string InvalidCredentials = "Error.User.Login.InvalidCredentials";
                public const string Failed = "Error.User.Login.Failed";
            }

            public const string NotFound = "Error.User.NotFound";
            public const string UpdateFailed = "Error.User.Update.Failed";
            public const string DeleteFailed = "Error.User.Delete.Failed";

            public static class PasswordChange
            {
                public const string Failed = "Error.User.PasswordChange.Failed";
                public const string OldPasswordIncorrect = "Error.User.PasswordChange.OldPasswordIncorrect";
            }

            public static class PasswordReset
            {
                public const string EmailNotFound = "Error.User.PasswordReset.EmailNotFound";
                public const string CodeExpired = "Error.User.PasswordReset.CodeExpired";
                public const string CodeInvalid = "Error.User.PasswordReset.CodeInvalid";
                public const string EmailSendFailed = "Error.User.PasswordReset.EmailSendFailed";
            }
        }

        public static class Project
        {
            public const string NotFound = "Error.Project.NotFound";
            public const string CreateFailed = "Error.Project.Create.Failed";
            public const string UpdateFailed = "Error.Project.Update.Failed";
            public const string DeleteFailed = "Error.Project.Delete.Failed";
            public const string Unauthorized = "Error.Project.Unauthorized";
        }

        public static class BackgroundJob
        {
            public const string ProcessFailed = "Error.BackgroundJob.ProcessFailed";
            public const string HandlerNotFound = "Error.BackgroundJob.HandlerNotFound";
        }

        public static class CV
        {
            public const string NotFound = "Error.CV.NotFound";
            public const string CreateFailed = "Error.CV.CreateFailed";
            public const string UpdateFailed = "Error.CV.UpdateFailed";
            public const string DeleteFailed = "Error.CV.DeleteFailed";
            public const string SetPrimaryFailed = "Error.CV.SetPrimaryFailed";
            public const string AnalysisFailed = "Error.CV.AnalysisFailed";
            public const string GenerateFailed = "Error.CV.GenerateFailed";
            public const string NoExtractedText = "Error.CV.NoExtractedText";
        }

        public static class Job
        {
            public const string NotFound = "Error.Job.NotFound";
            public const string CreateFailed = "Error.Job.CreateFailed";
            public const string UpdateFailed = "Error.Job.UpdateFailed";
            public const string DeleteFailed = "Error.Job.DeleteFailed";
        }

        public static class JobApplication
        {
            public const string NotFound = "Error.JobApplication.NotFound";
            public const string CreateFailed = "Error.JobApplication.CreateFailed";
            public const string AlreadyApplied = "Error.JobApplication.AlreadyApplied";
        }
    }

    public static class Validation
    {
        public static class User
        {
            public const string FullNameRequired = "Validation.User.FullNameRequired";
            public const string EmailRequired = "Validation.User.EmailRequired";
            public const string EmailInvalid = "Validation.User.EmailInvalid";
            public const string PasswordRequired = "Validation.User.PasswordRequired";
            public const string PasswordMinLength = "Validation.User.PasswordMinLength";
            public const string UserIdRequired = "Validation.User.UserIdRequired";
        }

        public static class Project
        {
            public const string TitleRequired = "Validation.Project.TitleRequired";
            public const string DescriptionRequired = "Validation.Project.DescriptionRequired";
            public const string OwnerIdRequired = "Validation.Project.OwnerIdRequired";
            public const string ProjectIdRequired = "Validation.Project.ProjectIdRequired";
        }

        public static class CV
        {
            public const string CandidateIdRequired = "Validation.CV.CandidateIdRequired";
            public const string FileRequired = "Validation.CV.FileRequired";
        }

        public static class Job
        {
            public const string TitleRequired = "Validation.Job.TitleRequired";
            public const string DescriptionRequired = "Validation.Job.DescriptionRequired";
            public const string CompanyIdRequired = "Validation.Job.CompanyIdRequired";
        }
    }

    public static class Success
    {
        public static class User
        {
            public const string Register = "Success.User.Register";
            public const string Login = "Success.User.Login";
            public const string Update = "Success.User.Update";
            public const string Delete = "Success.User.Delete";
            public const string PasswordChange = "Success.User.PasswordChange";

            public static class PasswordReset
            {
                public const string CodeSent = "Success.User.PasswordReset.CodeSent";
                public const string Reset = "Success.User.PasswordReset.Reset";
            }
        }

        public static class Project
        {
            public const string Create = "Success.Project.Create";
            public const string Update = "Success.Project.Update";
            public const string Delete = "Success.Project.Delete";
        }

        public static class CV
        {
            public const string Create = "Success.CV.Create";
            public const string Update = "Success.CV.Update";
            public const string Delete = "Success.CV.Delete";
            public const string SetPrimary = "Success.CV.SetPrimary";
            public const string AnalysisComplete = "Success.CV.AnalysisComplete";
            public const string Generated = "Success.CV.Generated";
        }

        public static class Job
        {
            public const string Create = "Success.Job.Create";
            public const string Update = "Success.Job.Update";
            public const string Delete = "Success.Job.Delete";
        }

        public static class JobApplication
        {
            public const string Create = "Success.JobApplication.Create";
            public const string Withdrawn = "Success.JobApplication.Withdrawn";
        }
    }

    public static class Logging
    {
        public static class User
        {
            public static class Register
            {
                public const string Start = "Logging.User.Register.Start";
                public const string Complete = "Logging.User.Register.Complete";
            }

            public static class Login
            {
                public const string Start = "Logging.User.Login.Start";
                public const string Complete = "Logging.User.Login.Complete";
            }

            public static class Update
            {
                public const string Start = "Logging.User.Update.Start";
                public const string Complete = "Logging.User.Update.Complete";
            }

            public static class Delete
            {
                public const string Start = "Logging.User.Delete.Start";
                public const string Complete = "Logging.User.Delete.Complete";
            }

            public static class PasswordChange
            {
                public const string Start = "Logging.User.PasswordChange.Start";
                public const string Complete = "Logging.User.PasswordChange.Complete";
            }

            public static class PasswordReset
            {
                public const string Request = "Logging.User.PasswordReset.Request";
                public const string Complete = "Logging.User.PasswordReset.Complete";
            }
        }

        public static class Project
        {
            public static class Create
            {
                public const string Start = "Logging.Project.Create.Start";
                public const string Complete = "Logging.Project.Create.Complete";
            }

            public static class Update
            {
                public const string Start = "Logging.Project.Update.Start";
                public const string Complete = "Logging.Project.Update.Complete";
            }

            public static class Delete
            {
                public const string Start = "Logging.Project.Delete.Start";
                public const string Complete = "Logging.Project.Delete.Complete";
            }
        }

        public static class BackgroundJob
        {
            public const string ProcessStart = "Logging.BackgroundJob.ProcessStart";
            public const string ProcessComplete = "Logging.BackgroundJob.ProcessComplete";
            public const string ProcessFailed = "Logging.BackgroundJob.ProcessFailed";
        }

        public static class CV
        {
            public static class Create
            {
                public const string Start = "Logging.CV.Create.Start";
                public const string Complete = "Logging.CV.Create.Complete";
            }

            public static class Analyze
            {
                public const string Start = "Logging.CV.Analyze.Start";
                public const string Complete = "Logging.CV.Analyze.Complete";
            }

            public static class Generate
            {
                public const string Start = "Logging.CV.Generate.Start";
                public const string Complete = "Logging.CV.Generate.Complete";
            }
        }

        public static class Job
        {
            public static class Recommendation
            {
                public const string Start = "Logging.Job.Recommendation.Start";
                public const string Complete = "Logging.Job.Recommendation.Complete";
            }
        }
    }

    public static class Status
    {
        public const string Processing = "Status.Processing";
        public const string Completed = "Status.Completed";
        public const string Failed = "Status.Failed";
        public const string Pending = "Status.Pending";
    }
}

